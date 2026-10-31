namespace TarjetaSUBE;

public class Colectivo
{
    private string _linea = string.Empty;
    private decimal _tarifa = 1580m;

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
        var esMedioBoleto = tarjeta.Tipo?.Nombre == "MedioBoletoEstudiantil";
        var coeficiente = tarjeta.Tipo?.Coeficiente ?? 1m;

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
            TipoTarjeta = tarjeta.Tipo?.Nombre ?? "Normal",
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