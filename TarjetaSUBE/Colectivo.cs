namespace TarjetaSUBE;

public class Colectivo
{
    private string _linea = string.Empty;
    private decimal _tarifa = 2150m;
    private readonly IProveedorDeFecha _reloj;

    public int Id { get; set; }
    public bool EsInterurbana { get; set; }

    public string Linea
    {
        get => _linea;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("La línea del colectivo es obligatoria.");
            _linea = value.Trim();
        }
    }

    public decimal Tarifa
    {
        get => _tarifa;
        set
        {
            if (value <= 0)
                throw new ArgumentException("La tarifa debe ser mayor que cero.");
            _tarifa = value;
        }
    }

    public Colectivo()
    {
        _reloj = new ProveedorDeFechaSistema();
    }

    public Colectivo(string linea, bool esInterurbana = false, IProveedorDeFecha? reloj = null)
    {
        Linea = linea;
        EsInterurbana = esInterurbana;
        _reloj = reloj ?? new ProveedorDeFechaSistema();
    }

    public Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaSimulada = null)
    {
        if (tarjeta is null)
            throw new ArgumentNullException(nameof(tarjeta));

        var fechaActual = fechaSimulada ?? _reloj.Ahora;
        var tipoNombre = tarjeta.Tipo?.Nombre ?? "Normal";

        if (!EsHorarioValido(tipoNombre, fechaActual))
            return null;

        if (!CumpleReglaCincoMinutos(tarjeta, tipoNombre, fechaActual))
            return null;

        var esTrasbordo = EsTrasbordoAplicable(tarjeta, fechaActual);
        var coeficiente = ObtenerCoeficiente(tarjeta, tipoNombre, fechaActual, esTrasbordo);

        var tarifaBase = EsInterurbana ? 3000m : _tarifa;
        var montoBase = tarifaBase * coeficiente;

        if (montoBase > 0 && !tarjeta.PuedeDescontar(montoBase))
            return null;

        var totalAbonado = ProcesarPago(tarjeta, montoBase);
        var boleto = GenerarBoleto(tarjeta, montoBase, totalAbonado, fechaActual, tipoNombre, esTrasbordo);

        tarjeta.Boletos.Add(boleto);
        return boleto;
    }

    private bool EsHorarioValido(string tipoNombre, DateTime fecha)
    {
        if (tipoNombre == "Normal") return true;

        var esFinDeSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday;
        var fueraDeHorario = fecha.TimeOfDay < new TimeSpan(6, 0, 0) || fecha.TimeOfDay >= new TimeSpan(22, 0, 0);

        return !esFinDeSemana && !fueraDeHorario;
    }

    private bool CumpleReglaCincoMinutos(Tarjeta tarjeta, string tipoNombre, DateTime fecha)
    {
        if (tipoNombre != "MedioBoletoEstudiantil") return true;

        var ultimoViaje = tarjeta.Boletos.OrderByDescending(b => b.Fecha).FirstOrDefault();
        return ultimoViaje == null || (fecha - ultimoViaje.Fecha).TotalMinutes >= 5;
    }

    private bool EsTrasbordoAplicable(Tarjeta tarjeta, DateTime fecha)
    {
        var ultimoBoleto = tarjeta.Boletos.OrderByDescending(b => b.Fecha).FirstOrDefault();
        if (ultimoBoleto?.Colectivo == null || Linea == ultimoBoleto.Colectivo.Linea)
            return false;

        var esDiaPermitido = fecha.DayOfWeek >= DayOfWeek.Monday && fecha.DayOfWeek <= DayOfWeek.Saturday;
        var esHoraPermitida = fecha.TimeOfDay >= new TimeSpan(7, 0, 0) && fecha.TimeOfDay <= new TimeSpan(22, 0, 0);

        if (!esDiaPermitido || !esHoraPermitida) return false;

        var primerBoleto = tarjeta.Boletos.OrderByDescending(b => b.Fecha).FirstOrDefault(b => !b.EsTrasbordo);
        return primerBoleto != null && (fecha - primerBoleto.Fecha).TotalHours <= 1.0;
    }

    private decimal ObtenerCoeficiente(Tarjeta tarjeta, string tipoNombre, DateTime fecha, bool esTrasbordo)
    {
        if (esTrasbordo) return 0m;

        var coeficiente = tarjeta.Tipo?.Coeficiente ?? 1m;

        if (tipoNombre == "MedioBoletoEstudiantil")
        {
            var viajesHoy = tarjeta.Boletos.Count(b => b.Fecha.Date == fecha.Date);
            if (viajesHoy >= 2) coeficiente = 1m;
        }
        else if (tipoNombre == "BoletoGratuitoEstudiantil")
        {
            var viajesGratisHoy = tarjeta.Boletos.Count(b => b.Fecha.Date == fecha.Date && b.Monto == 0m);
            if (viajesGratisHoy >= 2) coeficiente = 1m;
        }
        else if (tipoNombre == "Normal")
        {
            var viajesMes = tarjeta.Boletos.Count(b => b.Fecha.Year == fecha.Year && b.Fecha.Month == fecha.Month);
            var numeroViaje = viajesMes + 1;
            if (numeroViaje >= 80) coeficiente *= 0.75m;
            else if (numeroViaje >= 30) coeficiente *= 0.80m;
        }

        return coeficiente;
    }

    private decimal ProcesarPago(Tarjeta tarjeta, decimal montoBase)
    {
        var saldoAnterior = tarjeta.Saldo;
        if (montoBase > 0) tarjeta.Descontar(montoBase);

        return montoBase > 0 && saldoAnterior < 0
            ? montoBase + Math.Abs(saldoAnterior)
            : montoBase;
    }

    private Boleto GenerarBoleto(Tarjeta tarjeta, decimal montoBase, decimal totalAbonado, DateTime fecha, string tipo, bool trasbordo)
    {
        return new Boleto
        {
            Tarjeta = tarjeta,
            TarjetaId = tarjeta.Id,
            TipoTarjeta = tipo,
            Colectivo = this,
            Monto = montoBase,
            SaldoRestante = tarjeta.Saldo,
            TotalAbonado = totalAbonado,
            Fecha = fecha,
            EsTrasbordo = trasbordo
        };
    }
}