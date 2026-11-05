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
        Assert.That(() => new Tarjeta(60000m),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Cargar_AceptaTodosLosMontosValidos()
    {
        var montosAceptados = new decimal[] { 2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m };

        foreach (var monto in montosAceptados)
        {
            var tarjeta = new Tarjeta();
            tarjeta.Cargar(monto);
            Assert.That(tarjeta.Saldo, Is.EqualTo(monto), $"Falló al cargar el monto de ${monto}");
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
    public void Cargar_QueSuperaElLimite_AcreditaHastaElMaximoYDejaPendiente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(30000m);
        tarjeta.Cargar(30000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
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

        tarjeta.Descontar(2150m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));
    }

    [Test]
    public void Descontar_SuperaElLimiteNegativo_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();

        Assert.That(() => tarjeta.Descontar(2001m),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Descontar_MontoQueSuperaElLimiteNegativo_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        Assert.That(() => tarjeta.Descontar(4001m),
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

    [Test]
    public void Descontar_NoPuedeSuperarElLimiteNegativo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Descontar(2000m);

        Assert.That(() => tarjeta.Descontar(0.01m),
            Throws.TypeOf<InvalidOperationException>());
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
    }

    [Test]
    public void Cargar_DescuentaElSaldoConsumidoEnViajePlusDentroDelLimite()
    {
        var tarjeta = new Tarjeta(150m);
        var colectivo = new Colectivo("Línea 102");

        colectivo.PagarCon(tarjeta);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));

        tarjeta.Cargar(3000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void Cargar_QueSuperaElMaximo_DejaElExcedentePendiente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(30000m);
        tarjeta.Cargar(30000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void Descontar_ConSaldoPendiente_ReactivaLaCargaHastaElMaximo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(30000m);
        tarjeta.Cargar(30000m);

        var colectivo = new Colectivo("Línea 102");
        colectivo.PagarCon(tarjeta);

        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(1850m));
    }
}