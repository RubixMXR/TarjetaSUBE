using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class BoletoTests
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
    public void BoletoNuevo_TieneMontoCero()
    {
        var boleto = new Boleto();
        Assert.That(boleto.Monto, Is.EqualTo(0m));
    }

    [Test]
    public void Boleto_GuardaElMonto()
    {
        var boleto = new Boleto { Monto = 1580m };
        Assert.That(boleto.Monto, Is.EqualTo(1580m));
    }

    [Test]
    public void BoletoConMontoCero_LanzaExcepcion()
    {
        Assert.That(() => new Boleto { Monto = 0m },
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void BoletoConMontoNegativo_LanzaExcepcion()
    {
        Assert.That(() => new Boleto { Monto = -100m },
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Boleto_GuardaLaFecha()
    {
        var fecha = new DateTime(2026, 9, 21, 14, 30, 0);
        var boleto = new Boleto { Monto = 1580m, Fecha = fecha };
        Assert.That(boleto.Fecha, Is.EqualTo(fecha));
    }

    [Test]
    public void Boleto_SePersisteYSeRecuperaDeLaBase()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);

        var colectivo = new Colectivo("Línea 102");

        _db.Tarjetas.Add(tarjeta);
        _db.Colectivos.Add(colectivo);
        _db.SaveChanges();

        var boleto = colectivo.PagarCon(tarjeta);

        _db.Boletos.Add(boleto);
        _db.SaveChanges();

        var recuperado = _db.Boletos.Find(boleto.Id);

        Assert.That(recuperado, Is.Not.Null);
        Assert.That(recuperado!.Monto, Is.EqualTo(1580m));
    }

    [Test]
    public void Boleto_SeVinculaConTarjetaYColectivoEnLaBase()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);

        var colectivo = new Colectivo("Línea 102");

        _db.Tarjetas.Add(tarjeta);
        _db.Colectivos.Add(colectivo);
        _db.SaveChanges();

        var boleto = colectivo.PagarCon(tarjeta);
        _db.Boletos.Add(boleto);
        _db.SaveChanges();

        var recuperado = _db.Boletos
            .Include(b => b.Tarjeta)
            .Include(b => b.Colectivo)
            .FirstOrDefault(b => b.Id == boleto.Id);

        Assert.That(recuperado, Is.Not.Null);
        Assert.That(recuperado!.Tarjeta!.Id, Is.EqualTo(tarjeta.Id));
        Assert.That(recuperado.Colectivo!.Id, Is.EqualTo(colectivo.Id));
    }
}