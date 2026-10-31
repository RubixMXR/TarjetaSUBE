namespace TarjetaSUBE;

public class TarjetaTipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Coeficiente { get; set; } = 1m;

    public TarjetaTipo() { }

    public TarjetaTipo(string nombre, decimal coeficiente)
    {
        Nombre = nombre;
        Coeficiente = coeficiente;
    }
}