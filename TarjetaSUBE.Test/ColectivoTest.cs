using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class ColectivoTests
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
    public void ColectivoNuevo_TieneLaTarifaBasica()
    {
        var colectivo = new Colectivo("Línea 102");
        Assert.That(colectivo.Tarifa, Is.EqualTo(1580m));
    }

    [Test]
    public void Colectivo_GuardaLaLinea()
    {
        var colectivo = new Colectivo("Línea 102");
        Assert.That(colectivo.Linea, Is.EqualTo("Línea 102"));
    }

    [Test]
    public void Colectivo_RecortaLosEspaciosDeLaLinea()
    {
        var colectivo = new Colectivo("  Línea 102  ");
        Assert.That(colectivo.Linea, Is.EqualTo("Línea 102"));
    }

    [Test]
    public void ColectivoConLineaVacia_LanzaExcepcion()
    {
        Assert.That(() => new Colectivo(""),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ColectivoConLineaEnBlanco_LanzaExcepcion()
    {
        Assert.That(() => new Colectivo("   "),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ColectivoConLineaNula_LanzaExcepcion()
    {
        Assert.That(() => new Colectivo(null!),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void TarifaCero_LanzaExcepcion()
    {
        var colectivo = new Colectivo("Línea 102");
        Assert.That(() => colectivo.Tarifa = 0m,
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void TarifaNegativa_LanzaExcepcion()
    {
        var colectivo = new Colectivo("Línea 102");
        Assert.That(() => colectivo.Tarifa = -100m,
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void PagarCon_GeneraUnBoletoConElMontoDeLaTarifa()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto.Monto, Is.EqualTo(1580m));
    }

    [Test]
    public void PagarCon_DescuentaLaTarifaDelSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        colectivo.PagarCon(tarjeta);

        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void PagarCon_VinculaElBoletoConLaTarjetaYElColectivo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto.Tarjeta, Is.SameAs(tarjeta));
        Assert.That(boleto.Colectivo, Is.SameAs(colectivo));
    }

    [Test]
    public void PagarCon_AsignaLaFechaDelBoleto()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        var antes = DateTime.Now;
        var boleto = colectivo.PagarCon(tarjeta);
        var despues = DateTime.Now;

        Assert.That(boleto.Fecha, Is.InRange(antes, despues));
    }

    [Test]
    public void PagarCon_SinSaldoSuficiente_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);
        var colectivo = new Colectivo("Línea 102");

        tarjeta.Descontar(1000m);

        Assert.That(() => colectivo.PagarCon(tarjeta),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void PagarCon_TarjetaNula_LanzaExcepcion()
    {
        var colectivo = new Colectivo("Línea 102");
        Assert.That(() => colectivo.PagarCon(null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Colectivo_SePersisteYSeRecuperaDeLaBase()
    {
        var colectivo = new Colectivo("Línea 102");

        _db.Colectivos.Add(colectivo);
        _db.SaveChanges();

        var recuperado = _db.Colectivos.Find(colectivo.Id);

        Assert.That(recuperado, Is.Not.Null);
        Assert.That(recuperado!.Linea, Is.EqualTo("Línea 102"));
        Assert.That(recuperado.Tarifa, Is.EqualTo(1580m));
    }
}