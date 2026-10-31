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
    public void BoletoConMontoCero_EsValido()
    {
        var boleto = new Boleto { Monto = 0m };
        Assert.That(boleto.Monto, Is.EqualTo(0m));
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
    public void Boleto_ExponeCorrectamenteLosDatosDelViaje()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        _db.Tarjetas.Add(tarjeta);
        _db.Colectivos.Add(colectivo);
        _db.SaveChanges();

        var boleto = colectivo.PagarCon(tarjeta);
        _db.Boletos.Add(boleto!);
        _db.SaveChanges();

        Assert.That(boleto!.ObtenerTipoTarjeta(), Is.EqualTo("Normal"));
        Assert.That(boleto.ObtenerLineaColectivo(), Is.EqualTo("Línea 102"));
        Assert.That(boleto.ObtenerTarjetaId(), Is.EqualTo(tarjeta.Id));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(3420m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(1580m));
    }

    [Test]
    public void Boleto_TipoTarjetaMedioBoleto_DevuelveNombreCorrecto()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto!.ObtenerTipoTarjeta(), Is.EqualTo("MedioBoletoEstudiantil"));
        Assert.That(boleto.Monto, Is.EqualTo(790m));
    }

    [Test]
    public void Boleto_ConSaldoNegativo_InformaTotalAbonadoSuperior()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Descontar(400m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(-1980m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(1980m));
    }

    [Test]
    public void Boleto_ConTarjetaGratuita_TotalAbonadoEsCero()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        tarjeta.Descontar(1000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(-1000m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
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

        _db.Boletos.Add(boleto!);
        _db.SaveChanges();

        var recuperado = _db.Boletos.Find(boleto!.Id);

        Assert.That(recuperado, Is.Not.Null);
        Assert.That(recuperado!.Monto, Is.EqualTo(1580m));
        Assert.That(recuperado.SaldoRestante, Is.EqualTo(3420m));
        Assert.That(recuperado.TotalAbonado, Is.EqualTo(1580m));
        Assert.That(recuperado.TarjetaId, Is.EqualTo(tarjeta.Id));
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
        _db.Boletos.Add(boleto!);
        _db.SaveChanges();

        var recuperado = _db.Boletos
            .Include(b => b.Tarjeta)
            .Include(b => b.Colectivo)
            .FirstOrDefault(b => b.Id == boleto!.Id);

        Assert.That(recuperado, Is.Not.Null);
        Assert.That(recuperado!.Tarjeta!.Id, Is.EqualTo(tarjeta.Id));
        Assert.That(recuperado.Colectivo!.Id, Is.EqualTo(colectivo.Id));
    }
}