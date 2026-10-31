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

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1580m));
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

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarjeta, Is.SameAs(tarjeta));
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

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Fecha, Is.InRange(antes, despues));
    }

    [Test]
    public void PagarCon_ConSaldoEnElLimiteNegativo_DevuelveNull()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Descontar(2000m);

        var colectivo = new Colectivo("Línea 102");
        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-2000m));
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

    [Test]
    public void PagarCon_SinSaldoPeroDentroDelLimite_PuedeViajar()
    {
        var tarjeta = new Tarjeta();
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void PagarCon_ConMedioBoleto_PagaLaMitadDeLaTarifa()
    {
        var tarjetaNormal = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjetaNormal.Cargar(5000m);

        var tarjetaMedio = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjetaMedio.Cargar(5000m);

        var colectivo = new Colectivo("Línea 102");

        var boletoNormal = colectivo.PagarCon(tarjetaNormal);
        var boletoMedio = colectivo.PagarCon(tarjetaMedio);

        Assert.That(boletoNormal, Is.Not.Null);
        Assert.That(boletoMedio, Is.Not.Null);
        Assert.That(boletoNormal!.Monto, Is.EqualTo(1580m));
        Assert.That(boletoMedio!.Monto, Is.EqualTo(790m));
    }

    [Test]
    public void PagarCon_ConFranquiciaCompleta_SiemprePuedePagar()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_ConBoletoGratuitoEstudiantil_PagaCero()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(2000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m));
    }

    [Test]
    public void PagarCon_MedioBoleto_AntesDe5Minutos_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 10, 31, 10, 0, 0);

        var primerBoleto = colectivo.PagarCon(tarjeta, fecha);
        Assert.That(primerBoleto, Is.Not.Null);

        var segundoBoleto = colectivo.PagarCon(tarjeta, fecha.AddMinutes(4));
        Assert.That(segundoBoleto, Is.Null);
    }

    [Test]
    public void PagarCon_MedioBoleto_DespuesDe5Minutos_PermiteViajar()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 10, 31, 10, 0, 0);

        colectivo.PagarCon(tarjeta, fecha);

        var segundoBoleto = colectivo.PagarCon(tarjeta, fecha.AddMinutes(5));
        Assert.That(segundoBoleto, Is.Not.Null);
    }

    [Test]
    public void PagarCon_MedioBoleto_TercerViajeEnElDia_NoAplicaDescuento()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 10, 31, 10, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(6));
        var boleto3 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(12));

        Assert.That(boleto1!.Monto, Is.EqualTo(790m));
        Assert.That(boleto2!.Monto, Is.EqualTo(790m));
        Assert.That(boleto3!.Monto, Is.EqualTo(1580m));
    }

    [Test]
    public void PagarCon_MedioBoleto_DiaSiguiente_ReseteaContadorYPagaMitad()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(10000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 10, 31, 10, 0, 0);

        colectivo.PagarCon(tarjeta, fecha);
        colectivo.PagarCon(tarjeta, fecha.AddMinutes(6));

        var boletoDiaSiguiente = colectivo.PagarCon(tarjeta, fecha.AddDays(1));

        Assert.That(boletoDiaSiguiente!.Monto, Is.EqualTo(790m));
    }
}