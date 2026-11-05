namespace TarjetaSUBE;

public class EstacionDeBicicletas
{
    public List<Bicicleta> Bicicletas { get; set; } = new();

    public EstacionDeBicicletas() { }

    public EstacionDeBicicletas(int cantidadBicicletas)
    {
        for (int i = 0; i < cantidadBicicletas; i++)
        {
            Bicicletas.Add(new Bicicleta { Id = i + 1 });
        }
    }

    public Bicicleta? RetirarBicicleta(Tarjeta tarjeta, DateTime fechaRetiro)
    {
        if (tarjeta is null)
            throw new ArgumentNullException(nameof(tarjeta));

        var tarifaServicio = 2418.75m;
        var totalACobrar = tarifaServicio + tarjeta.MultasPendientes;

        if (!tarjeta.PuedeDescontar(totalACobrar))
            return null;

        var biciDisponible = Bicicletas.FirstOrDefault(b => b.Disponible);
        if (biciDisponible is null)
            return null;

        tarjeta.Descontar(totalACobrar);
        tarjeta.MultasPendientes = 0m;

        biciDisponible.Disponible = false;
        biciDisponible.HoraRetiro = fechaRetiro;

        return biciDisponible;
    }

    public void DevolverBicicleta(Bicicleta bici, Tarjeta tarjeta, DateTime fechaDevolucion)
    {
        if (bici is null)
            throw new ArgumentNullException(nameof(bici));

        if (tarjeta is null)
            throw new ArgumentNullException(nameof(tarjeta));

        if (bici.Disponible || bici.HoraRetiro is null)
            throw new InvalidOperationException("La bicicleta no está en uso.");

        var tiempoUso = fechaDevolucion - bici.HoraRetiro.Value;

        if (tiempoUso.TotalHours > 1)
        {
            tarjeta.MultasPendientes += 1250m;
        }

        bici.Disponible = true;
        bici.HoraRetiro = null;
    }
}