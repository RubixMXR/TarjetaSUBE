namespace TarjetaSUBE;

public interface IProveedorDeFecha
{
    DateTime Ahora { get; }
}

public class ProveedorDeFechaSistema : IProveedorDeFecha
{
    public DateTime Ahora => DateTime.Now;
}