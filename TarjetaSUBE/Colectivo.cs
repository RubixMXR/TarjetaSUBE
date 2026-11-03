namespace TarjetaSUBE;

public class Colectivo
{
    private string _linea = string.Empty;
    private decimal _tarifa = 2150m;

    public int Id { get; set; }

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

    public Colectivo() { }

    public Colectivo(string linea)
    {
        Linea = linea;
    }

    public Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaSimulada = null)
    {
        if (tarjeta is null)
            throw new ArgumentNullException(nameof(tarjeta));

        var fechaActual = fechaSimulada ?? DateTime.Now;
        var tipoNombre = tarjeta.Tipo?.Nombre ?? "Normal";
        var esMedioBoleto = tipoNombre == "MedioBoletoEstudiantil";
        var coeficiente = tarjeta.Tipo?.Coeficiente ?? 1m;

        if (tipoNombre != "Normal")
        {
            var dia = fechaActual.DayOfWeek;
            var hora = fechaActual.TimeOfDay;
            var esFinDeSemana = dia == DayOfWeek.Saturday || dia == DayOfWeek.Sunday;
            var fueraDeHorario = hora < new TimeSpan(6, 0, 0) || hora >= new TimeSpan(22, 0, 0);

            if (esFinDeSemana || fueraDeHorario)
                return null;
        }

        if (esMedioBoleto)
        {
            var ultimoViaje = tarjeta.Boletos.OrderByDescending(b => b.Fecha).FirstOrDefault();
            if (ultimoViaje != null && (fechaActual - ultimoViaje.Fecha).TotalMinutes < 5)
            {
                return null;
            }

            var viajesHoy = tarjeta.Boletos.Count(b => b.Fecha.Date == fechaActual.Date);
            if (viajesHoy >= 2)
            {
                coeficiente = 1m;
            }
        }

        var esBoletoGratuito = tipoNombre == "BoletoGratuitoEstudiantil";

        if (esBoletoGratuito)
        {
            var viajesGratisHoy = tarjeta.Boletos
                .Count(b => b.Fecha.Date == fechaActual.Date && b.Monto == 0m);

            if (viajesGratisHoy >= 2)
            {
                coeficiente = 1m;
            }
        }

        if (tipoNombre == "Normal")
        {
            var viajesMes = tarjeta.Boletos.Count(b => b.Fecha.Year == fechaActual.Year && b.Fecha.Month == fechaActual.Month);
            var numeroViaje = viajesMes + 1;

            if (numeroViaje >= 80)
                coeficiente *= 0.75m;
            else if (numeroViaje >= 30)
                coeficiente *= 0.80m;
        }

        var montoBase = _tarifa * coeficiente;

        if (montoBase > 0 && !tarjeta.PuedeDescontar(montoBase))
            return null;

        var saldoAnterior = tarjeta.Saldo;

        if (montoBase > 0)
            tarjeta.Descontar(montoBase);

        var totalAbonado = montoBase;

        if (montoBase > 0 && saldoAnterior < 0)
        {
            totalAbonado += Math.Abs(saldoAnterior);
        }

        var boleto = new Boleto
        {
            Tarjeta = tarjeta,
            TarjetaId = tarjeta.Id,
            TipoTarjeta = tipoNombre,
            Colectivo = this,
            Monto = montoBase,
            SaldoRestante = tarjeta.Saldo,
            TotalAbonado = totalAbonado,
            Fecha = fechaActual
        };

        tarjeta.Boletos.Add(boleto);

        return boleto;
    }
}