namespace TarjetaSUBE;

public class Boleto
{
    private decimal _monto;

    public int Id { get; set; }

    public Tarjeta? Tarjeta { get; set; }

    public Colectivo? Colectivo { get; set; }

    public decimal Monto
    {
        get => _monto;
        set
        {
            if (value < 0)
                throw new ArgumentException("El monto del boleto no puede ser negativo.");

            _monto = value;
        }
    }

    public DateTime Fecha { get; set; }
}