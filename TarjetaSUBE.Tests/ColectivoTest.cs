using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class ColectivoTests
{
    private SubeDbContext _db = null!;

    private Tarjeta CrearTarjetaNormalConViajes(int cantidadViajes, DateTime fechaReferencia)
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjeta.Cargar(5000m);
        var primerDiaMes = new DateTime(fechaReferencia.Year, fechaReferencia.Month, 1);
        for (int i = 0; i < cantidadViajes; i++)
        {
            tarjeta.Boletos.Add(new Boleto { Fecha = primerDiaMes, Monto = 2150m });
        }
        return tarjeta;
    }

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
        Assert.That(colectivo.Tarifa, Is.EqualTo(2150m));
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
        Assert.That(boleto!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_DescuentaLaTarifaDelSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        colectivo.PagarCon(tarjeta);

        Assert.That(tarjeta.Saldo, Is.EqualTo(2850m));
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
        Assert.That(recuperado.Tarifa, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_SinSaldoSuperaElLimiteNegativo_DevuelveNull()
    {
        var tarjeta = new Tarjeta();
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_ConMedioBoleto_PagaLaMitadDeLaTarifa()
    {
        var tarjetaNormal = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjetaNormal.Cargar(5000m);

        var tarjetaMedio = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjetaMedio.Cargar(5000m);

        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boletoNormal = colectivo.PagarCon(tarjetaNormal, fecha);
        var boletoMedio = colectivo.PagarCon(tarjetaMedio, fecha);

        Assert.That(boletoNormal, Is.Not.Null);
        Assert.That(boletoMedio, Is.Not.Null);
        Assert.That(boletoNormal!.Monto, Is.EqualTo(2150m));
        Assert.That(boletoMedio!.Monto, Is.EqualTo(1075m));
    }

    [Test]
    public void PagarCon_ConFranquiciaCompleta_SiemprePuedePagar()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

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
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

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
        var fecha = new DateTime(2026, 10, 30, 10, 0, 0);

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
        var fecha = new DateTime(2026, 10, 30, 10, 0, 0);

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
        var fecha = new DateTime(2026, 10, 30, 10, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(6));
        var boleto3 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(12));

        Assert.That(boleto1!.Monto, Is.EqualTo(1075m));
        Assert.That(boleto2!.Monto, Is.EqualTo(1075m));
        Assert.That(boleto3!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_MedioBoleto_DiaSiguiente_ReseteaContadorYPagaMitad()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(10000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 10, 29, 10, 0, 0);

        colectivo.PagarCon(tarjeta, fecha);
        colectivo.PagarCon(tarjeta, fecha.AddMinutes(6));

        var boletoDiaSiguiente = colectivo.PagarCon(tarjeta, fecha.AddDays(1));

        Assert.That(boletoDiaSiguiente!.Monto, Is.EqualTo(1075m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_NoPermiteMasDeDosViajesGratisPorDia()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 3, 10, 0, 0);

        colectivo.PagarCon(tarjeta, fecha);
        colectivo.PagarCon(tarjeta, fecha.AddMinutes(10));
        var tercero = colectivo.PagarCon(tarjeta, fecha.AddMinutes(20));

        var viajesGratis = tarjeta.Boletos.Count(b => b.Fecha.Date == fecha.Date && b.Monto == 0m);

        Assert.That(viajesGratis, Is.EqualTo(2));
        Assert.That(tercero, Is.Not.Null);
        Assert.That(tercero!.Monto, Is.Not.EqualTo(0m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_ViajesPosterioresAlSegundoSeCobranCompletos()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(10000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 3, 10, 0, 0);

        var boleto1 = colectivo.PagarCon(tarjeta, fecha);
        var boleto2 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(10));
        var boleto3 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(20));
        var boleto4 = colectivo.PagarCon(tarjeta, fecha.AddMinutes(30));

        Assert.That(boleto1!.Monto, Is.EqualTo(0m));
        Assert.That(boleto2!.Monto, Is.EqualTo(0m));
        Assert.That(boleto3!.Monto, Is.EqualTo(2150m));
        Assert.That(boleto4!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_PrimerViajeDelMes_PagaTarifaCompleta()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(0, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_Viaje29_PagaTarifaCompleta()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(28, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_Viaje30_Aplica20PorcientoDescuento()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(29, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1720m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_Viaje79_Aplica20PorcientoDescuento()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(78, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1720m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_Viaje80_Aplica25PorcientoDescuento()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(79, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1612.5m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_Viaje100_Aplica25PorcientoDescuento()
    {
        var fecha = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(99, fecha);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1612.5m));
    }

    [Test]
    public void PagarCon_TarjetaNormal_MesSiguiente_ReseteaContador()
    {
        var fechaOctubre = new DateTime(2026, 10, 15);
        var fechaNoviembre = new DateTime(2026, 11, 15);
        var tarjeta = CrearTarjetaNormalConViajes(29, fechaOctubre);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fechaNoviembre);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_TarjetaMedioBoleto_NoAplicaDescuentoPorUsoFrecuente()
    {
        var fecha = new DateTime(2026, 11, 16, 12, 0, 0);
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var primerDiaMes = new DateTime(2026, 11, 1, 10, 0, 0);
        for (int i = 0; i < 29; i++)
        {
            tarjeta.Boletos.Add(new Boleto { Fecha = primerDiaMes, Monto = 1075m });
        }
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1075m));
    }

    [Test]
    public void PagarCon_TarjetaBoletoGratuito_NoAplicaDescuentoPorUsoFrecuente()
    {
        var fecha = new DateTime(2026, 11, 16, 12, 0, 0);
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(5000m);
        tarjeta.Boletos.Add(new Boleto { Fecha = fecha.AddHours(-1), Monto = 0m });
        tarjeta.Boletos.Add(new Boleto { Fecha = fecha.AddHours(-2), Monto = 0m });
        var primerDiaMes = new DateTime(2026, 11, 1, 10, 0, 0);
        for (int i = 0; i < 27; i++)
        {
            tarjeta.Boletos.Add(new Boleto { Fecha = primerDiaMes, Monto = 0m });
        }
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(2150m));
    }

    [Test]
    public void PagarCon_TarjetaFranquiciaCompleta_NoAplicaDescuentoPorUsoFrecuente()
    {
        var fecha = new DateTime(2026, 11, 16, 12, 0, 0);
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_FranquiciaCompleta_AntesDeLas6_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 5, 59, 59);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void PagarCon_FranquiciaCompleta_DespuesDeLas22_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 22, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void PagarCon_FranquiciaCompleta_Sabado_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 7, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void PagarCon_MedioBoleto_AntesDeLas6_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 5, 59, 59);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void PagarCon_BoletoGratuito_Domingo_DevuelveNull()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 8, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Null);
    }

    [Test]
    public void PagarCon_TarjetaNormal_AntesDeLas6_PuedePagar()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 4, 5, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
    }

    [Test]
    public void PagarCon_TarjetaNormal_Sabado_PuedePagar()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea 102");
        var fecha = new DateTime(2026, 11, 7, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
    }

    [Test]
    public void PagarCon_ColectivoInterurbano_TarjetaNormal_Cobra3000()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("Normal", 1m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea Interurbana", esInterurbana: true);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(3000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m));
    }

    [Test]
    public void PagarCon_ColectivoInterurbano_ConMedioBoleto_CobraMitadDe3000()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m) };
        tarjeta.Cargar(5000m);
        var colectivo = new Colectivo("Línea Interurbana", esInterurbana: true);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(1500m));
    }

    [Test]
    public void PagarCon_ColectivoInterurbano_ConFranquiciaCompleta_CobraCero()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("FranquiciaCompleta", 0m) };
        var colectivo = new Colectivo("Línea Interurbana", esInterurbana: true);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_ColectivoInterurbano_ConBoletoGratuito_CobraCero()
    {
        var tarjeta = new Tarjeta { Tipo = new TarjetaTipo("BoletoGratuitoEstudiantil", 0m) };
        tarjeta.Cargar(2000m);
        var colectivo = new Colectivo("Línea Interurbana", esInterurbana: true);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var boleto = colectivo.PagarCon(tarjeta, fecha);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Monto, Is.EqualTo(0m));
    }
}