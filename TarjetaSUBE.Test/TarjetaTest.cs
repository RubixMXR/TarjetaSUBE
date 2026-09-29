using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class TarjetaTests
{
    private SubeDbContext _db = null!;

    [SetUp]
    public void Setup()
    {
        var opciones = new DbContextOptionsBuilder<SubeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new SubeDbContext(opciones);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void TarjetaNueva_TieneSaldoCero()
    {
        var tarjeta = new Tarjeta();
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void TarjetaConSaldoInicial_GuardaElSaldo()
    {
        var tarjeta = new Tarjeta(5000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
    }

    [Test]
    public void TarjetaConSaldoInicialNegativo_LanzaExcepcion()
    {
        Assert.That(() => new Tarjeta(-100m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void TarjetaConSaldoInicialMayorAlLimite_LanzaExcepcion()
    {
        Assert.That(() => new Tarjeta(50000m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Cargar_AceptaTodosLosMontosValidos()
    {
        var montos = new decimal[]
        {
            2000m, 3000m, 4000m, 5000m, 8000m,
            10000m, 15000m, 20000m, 25000m, 30000m
        };

        foreach (var monto in montos)
        {
            var tarjeta = new Tarjeta();
            tarjeta.Cargar(monto);
            Assert.That(tarjeta.Saldo, Is.EqualTo(monto),
                $"Debería aceptar la carga de ${monto}");
        }
    }

    [Test]
    public void Cargar_RechazaMontoNoAceptado()
    {
        var tarjeta = new Tarjeta();
        Assert.That(() => tarjeta.Cargar(2500m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Cargar_RechazaMontoCero()
    {
        var tarjeta = new Tarjeta();
        Assert.That(() => tarjeta.Cargar(0m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Cargar_RechazaMontoNegativo()
    {
        var tarjeta = new Tarjeta();
        Assert.That(() => tarjeta.Cargar(-2000m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Cargar_NoSuperaElLimiteDeSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(30000m);

        Assert.That(() => tarjeta.Cargar(15000m),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Cargar_AcumulaVariasCargas()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);
        tarjeta.Cargar(3000m);
        tarjeta.Cargar(5000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m));
    }

    [Test]
    public void Descontar_ConSaldoSuficiente_RestaElMonto()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);

        tarjeta.Descontar(1580m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void Descontar_SinSaldo_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        Assert.That(() => tarjeta.Descontar(1580m),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Descontar_MontoMayorAlSaldo_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        Assert.That(() => tarjeta.Descontar(3000m),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Descontar_MontoCero_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        Assert.That(() => tarjeta.Descontar(0m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Descontar_MontoNegativo_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        Assert.That(() => tarjeta.Descontar(-500m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Tarjeta_SePersisteYSeRecuperaDeLaBase()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);

        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        var recuperada = _db.Tarjetas.Find(tarjeta.Id);

        Assert.That(recuperada, Is.Not.Null);
        Assert.That(recuperada!.Saldo, Is.EqualTo(5000m));
    }
}