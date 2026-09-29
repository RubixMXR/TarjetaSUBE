using TarjetaSUBE;

using var db = new SubeDbContext();
db.Database.EnsureCreated();

Console.WriteLine("=== Simulación Tarjeta SUBE - Iteración 1 ===");
Console.WriteLine();

Console.WriteLine("--- Creando tarjeta y cargando saldo ---");
var tarjeta = new Tarjeta();
tarjeta.Cargar(5000m);
Console.WriteLine($"Tarjeta creada. Saldo inicial: ${tarjeta.Saldo}");
Console.WriteLine();

Console.WriteLine("--- Creando colectivos ---");
var colectivo102 = new Colectivo("Línea 102");
var colectivoK = new Colectivo("Línea K");
Console.WriteLine($"Colectivo {colectivo102.Linea} - Tarifa: ${colectivo102.Tarifa}");
Console.WriteLine($"Colectivo {colectivoK.Linea} - Tarifa: ${colectivoK.Tarifa}");
Console.WriteLine();

Console.WriteLine("--- Primer viaje ---");
var boleto1 = colectivo102.PagarCon(tarjeta);
Console.WriteLine($"Boleto 1: {boleto1.Colectivo!.Linea} - ${boleto1.Monto} - {boleto1.Fecha}");
Console.WriteLine($"Saldo restante: ${tarjeta.Saldo}");
Console.WriteLine();

Console.WriteLine("--- Segundo viaje ---");
var boleto2 = colectivoK.PagarCon(tarjeta);
Console.WriteLine($"Boleto 2: {boleto2.Colectivo!.Linea} - ${boleto2.Monto} - {boleto2.Fecha}");
Console.WriteLine($"Saldo restante: ${tarjeta.Saldo}");
Console.WriteLine();

db.Tarjetas.Add(tarjeta);
db.Colectivos.Add(colectivo102);
db.Colectivos.Add(colectivoK);
db.Boletos.Add(boleto1);
db.Boletos.Add(boleto2);
db.SaveChanges();

Console.WriteLine("--- Datos guardados en Sube.db ---");
Console.WriteLine($"Tarjetas: {db.Tarjetas.Count()}");
Console.WriteLine($"Colectivos: {db.Colectivos.Count()}");
Console.WriteLine($"Boletos: {db.Boletos.Count()}");
Console.WriteLine();

Console.WriteLine("Presioná una tecla para salir.");
Console.ReadKey();