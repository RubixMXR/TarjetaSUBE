namespace TarjetaSUBE;

public class Tarjeta
{
    private decimal _saldo;

    public int Id { get; set; }

    public decimal Saldo => _saldo;

    public const decimal LimiteSaldo = 40000m;

    public static readonly IReadOnlyList<decimal> CargasAceptadas = new List<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m,
        10000m, 15000m, 20000m, 25000m, 30000m
    };

    public Tarjeta() { }

    public Tarjeta(decimal saldoInicial)
    {
        if (saldoInicial < 0)
            throw new ArgumentException("El saldo inicial no puede ser negativo.");

        if (saldoInicial > LimiteSaldo)
            throw new ArgumentException($"El saldo no puede superar el límite de ${LimiteSaldo}.");

        _saldo = saldoInicial;
    }

    public void Cargar(decimal monto)
    {
        if (!CargasAceptadas.Contains(monto))
            throw new ArgumentException($"El monto ${monto} no es una carga aceptada.");

        if (_saldo + monto > LimiteSaldo)
            throw new InvalidOperationException($"La carga supera el límite de saldo de ${LimiteSaldo}.");

        _saldo += monto;
    }

    public void Descontar(decimal monto)
    {
        if (monto <= 0)
            throw new ArgumentException("El monto a descontar debe ser mayor que cero.");

        if (monto > _saldo)
            throw new InvalidOperationException("Saldo insuficiente.");

        _saldo -= monto;
    }
}