using NUnit.Framework;
using TarjetaSUBE;

namespace TarjetaSUBE.Tests;

public class TarjetaTipoTests
{
    [Test]
    public void TarjetaTipoNuevo_TieneValoresPorDefecto()
    {
        var tipo = new TarjetaTipo();

        Assert.That(tipo.Id, Is.EqualTo(0));
        Assert.That(tipo.Nombre, Is.EqualTo(string.Empty));
        Assert.That(tipo.Coeficiente, Is.EqualTo(1m));
    }

    [Test]
    public void Constructor_GuardaElNombreYElCoeficiente()
    {
        var tipo = new TarjetaTipo("MedioBoletoEstudiantil", 0.5m);

        Assert.That(tipo.Nombre, Is.EqualTo("MedioBoletoEstudiantil"));
        Assert.That(tipo.Coeficiente, Is.EqualTo(0.5m));
    }

    [Test]
    public void Propiedades_SePuedenModificar()
    {
        var tipo = new TarjetaTipo();

        tipo.Nombre = "FranquiciaCompleta";
        tipo.Coeficiente = 0m;

        Assert.That(tipo.Nombre, Is.EqualTo("FranquiciaCompleta"));
        Assert.That(tipo.Coeficiente, Is.EqualTo(0m));
    }
}