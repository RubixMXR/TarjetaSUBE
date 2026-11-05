namespace TarjetaSUBE;

public class Tarjeta
{
    private decimal _saldo;
    private decimal _saldoPendiente;
    private decimal _multasPendientes;

    public int Id { get; set; }

    public int? TarjetaTipoId { get; set; }

    public TarjetaTipo? Tipo { get; set; }

    public List<Boleto> Boletos { get; set; } = new();

    public decimal Saldo => _saldo;

    public decimal SaldoPendiente => _saldoPendiente;

    public decimal MultasPendientes => _multasPendientes;

    public const decimal LimiteSaldo = 56000m;

    public const decimal LimiteNegativo = -2000m;

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

        var nuevoSaldo = _saldo + monto;

        if (nuevoSaldo > LimiteSaldo)
        {
            _saldoPendiente += nuevoSaldo - LimiteSaldo;
            _saldo = LimiteSaldo;
        }
        else
        {
            _saldo = nuevoSaldo;
        }
    }

    public void Descontar(decimal monto)
    {
        if (monto <= 0)
            throw new ArgumentException("El monto a descontar debe ser mayor que cero.");

        if (_saldo - monto < LimiteNegativo)
            throw new InvalidOperationException("Saldo insuficiente.");

        _saldo -= monto;
        AcreditarCarga();
    }

    public bool PuedeDescontar(decimal monto)
    {
        return monto > 0 && _saldo - monto >= LimiteNegativo;
    }

    public void AcreditarCarga()
    {
        if (_saldoPendiente <= 0)
            return;

        var espacio = LimiteSaldo - _saldo;

        if (espacio <= 0)
            return;

        var aAcreditar = Math.Min(_saldoPendiente, espacio);

        _saldo += aAcreditar;
        _saldoPendiente -= aAcreditar;
    }

    public void AgregarMulta(decimal monto)
    {
        _multasPendientes += monto;
    }

    public void LimpiarMultas()
    {
        _multasPendientes = 0m;
    }
}