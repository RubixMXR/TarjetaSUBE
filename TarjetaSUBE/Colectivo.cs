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

    public Boleto? PagarCon(Tarjeta tarjeta)
    {
        if (tarjeta is null)
            throw new ArgumentNullException(nameof(tarjeta));

        var monto = CalcularMonto(tarjeta);

        if (monto > 0 && !tarjeta.PuedeDescontar(monto))
            return null;

        if (monto > 0)
            tarjeta.Descontar(monto);

        return new Boleto
        {
            Tarjeta = tarjeta,
            Colectivo = this,
            Monto = monto,
            Fecha = DateTime.Now
        };
    }

    private decimal CalcularMonto(Tarjeta tarjeta)
    {
        var coeficiente = tarjeta.Tipo?.Coeficiente ?? 1m;
        return _tarifa * coeficiente;
    }
}