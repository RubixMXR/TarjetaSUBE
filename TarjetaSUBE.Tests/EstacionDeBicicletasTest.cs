using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class EstacionDeBicicletasTests
{
    private EstacionDeBicicletas _estacion = null!;

    [SetUp]
    public void Setup()
    {
        _estacion = new EstacionDeBicicletas(3);
    }

    [Test]
    public void RetirarBicicleta_ConSaldoSuficiente_RetornaBicicletaYDescuentaTarifa()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        Assert.That(bici, Is.Not.Null);
        Assert.That(bici!.Disponible, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m));
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBicicleta_SinSaldoSuficiente_RetornaNull()
    {
        var tarjeta = new Tarjeta();
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        Assert.That(bici, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBicicleta_ConMultaPendienteYSaldoSuficiente_RetornaBicicletaYPagaMulta()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        tarjeta.MultasPendientes = 1250m;
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        Assert.That(bici, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - 2418.75m - 1250m));
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBicicleta_ConMultaPendienteSinSaldoSuficiente_RetornaNull()
    {
        var tarjeta = new Tarjeta();
        tarjeta.MultasPendientes = 1250m;
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        Assert.That(bici, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
    }

    [Test]
    public void RetirarBicicleta_ConVariasMultasAcumuladas_RetornaBicicletaYPagaTodas()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(10000m);
        tarjeta.MultasPendientes = 2500m;
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        Assert.That(bici, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m - 2500m));
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void DevolverBicicleta_AntesDeUnaHora_NoAplicaMulta()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);
        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        _estacion.DevolverBicicleta(bici!, tarjeta, fecha.AddMinutes(59));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
        Assert.That(bici!.Disponible, Is.True);
    }

    [Test]
    public void DevolverBicicleta_DespuesDeUnaHora_AplicaMulta()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);
        var bici = _estacion.RetirarBicicleta(tarjeta, fecha);

        _estacion.DevolverBicicleta(bici!, tarjeta, fecha.AddMinutes(61));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
        Assert.That(bici!.Disponible, Is.True);
    }

    [Test]
    public void DevolverBicicleta_MultiplesVecesDespuesDeUnaHora_AcumulaMultas()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(10000m);
        var fecha = new DateTime(2026, 11, 4, 10, 0, 0);

        var bici1 = _estacion.RetirarBicicleta(tarjeta, fecha);
        _estacion.DevolverBicicleta(bici1!, tarjeta, fecha.AddMinutes(61));

        var bici2 = _estacion.RetirarBicicleta(tarjeta, fecha.AddMinutes(70));
        _estacion.DevolverBicicleta(bici2!, tarjeta, fecha.AddMinutes(132));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - 2418.75m - 1250m - 2418.75m));
    }

    [Test]
    public void DevolverBicicleta_NoEstaEnUso_LanzaExcepcion()
    {
        var tarjeta = new Tarjeta();
        var bici = new Bicicleta();

        Assert.That(() => _estacion.DevolverBicicleta(bici, tarjeta, DateTime.Now),
            Throws.TypeOf<InvalidOperationException>());
    }
}