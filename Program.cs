using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using CalculadoraDeGastos;
using System.Text; // StringBuilder para armar el CSV


List<Gasto> gastos = new();
int siguienteId = 1;
string archivo = "gastos.json";
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

// ==== Carga inicial ====
Cargar();

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Calculadora de Gastos ===");
    Console.WriteLine("1) Agregar gasto");
    Console.WriteLine("2) Listar gastos");
    Console.WriteLine("3) Ver total gastado");
    Console.WriteLine("4) Eliminar gasto por ID");
    Console.WriteLine("5) Editar gasto por ID");
    Console.WriteLine("6) Ver gastos de ESTE MES");
    Console.WriteLine("7) Ver gastos ENTRE FECHAS");
    Console.WriteLine("8) Exportar TODOS los gastos a CSV");
    Console.WriteLine("0) Salir");
    Console.Write("Elegí una opción: ");
    var op = Console.ReadLine()?.Trim();

    switch (op)
    {
        case "1":
            AgregarGasto();
            break;
        case "2":
            ListarGastos();
            Pausa();
            break;
        case "3":
            MostrarTotal();
            Pausa();
            break;
        case "4":
            EliminarGasto();
            break;
        case "5":
            EditarGasto();
            break;
        case "6":
            ListarGastosEsteMes();
            Pausa();
            break;
        case "7":
            ListarGastosEntreFechas();
            Pausa();
            break;
        case "8":
            ExportarCSV(); // ya hace Pausa() adentro
            break;

        case "0":
            Console.WriteLine("¡Hasta luego!");
            Guardar(); // guardo por las dudas
            return;
        default:
            Console.WriteLine("Opción inválida.");
            Pausa();
            break;
    }
}

// ===== Funciones =====

void AgregarGasto()
{
    Console.Clear();
    Console.WriteLine("=== Nuevo gasto ===");

    Console.Write("Descripción: ");
    string? desc = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(desc))
    {
        Console.WriteLine("La descripción no puede estar vacía.");
        Pausa();
        return;
    }

    decimal monto;
    while (true)
    {
        Console.Write("Monto (usar coma o punto): ");
        string? entrada = Console.ReadLine();

        // Intento con formato local (es-AR) y con invariante
        if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out monto) ||
            decimal.TryParse(entrada?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out monto))
        {
            break;
        }

        Console.WriteLine("Monto inválido, probá de nuevo.");
    }

    // Fecha opcional (ENTER = hoy)
    Console.Write("Fecha (AAAA-MM-DD) o ENTER para hoy: ");
    string? f = Console.ReadLine();
    DateTime fecha = DateTime.Today;
    if (!string.IsNullOrWhiteSpace(f) && !DateTime.TryParse(f, out fecha))
    {
        Console.WriteLine("Fecha inválida. Se usará la de hoy.");
        fecha = DateTime.Today;
    }

    var gasto = new Gasto
    {
        Id = siguienteId++,
        Descripcion = desc.Trim(),
        Monto = monto,
        Fecha = fecha
    };

    gastos.Add(gasto);
    Guardar();
    Console.WriteLine("✅ Gasto agregado.");
    Pausa();
}
void ExportarCSV()
{
    Console.Clear();
    Console.WriteLine("=== Exportar a CSV ===");

    if (gastos.Count == 0)
    {
        Console.WriteLine("No hay gastos para exportar.");
        Pausa();
        return;
    }

    // Elegimos separador ';' (Excel en es-AR lo abre perfecto)
    // Monto en formato numérico (sin $), con coma decimal (es-AR)
    var cultura = CultureInfo.GetCultureInfo("es-AR");
    var sb = new StringBuilder();

    // Encabezados
    sb.AppendLine("Id;Fecha;Monto;Descripcion");

    foreach (var g in gastos.OrderBy(g => g.Fecha).ThenBy(g => g.Id))
    {
        string fecha = g.Fecha.ToString("yyyy-MM-dd"); // fecha clara
        string monto = g.Monto.ToString("0.##", cultura); // número sin símbolo

        // La descripción la “escapamos” si trae ;, " o saltos de línea
        string desc = CsvEscape(g.Descripcion);

        sb.AppendLine($"{g.Id};{fecha};{monto};{desc}");
    }

    string archivoCsv = "gastos.csv";
    File.WriteAllText(archivoCsv, sb.ToString(), Encoding.UTF8);

    Console.WriteLine($" Exportado a {Path.GetFullPath(archivoCsv)}");
    Console.WriteLine("Abrilo con Excel o Google Sheets.");
    Pausa();
}

// En CSV, si el texto tiene ;, comillas o saltos de línea, lo encerramos entre comillas
// y duplicamos las comillas internas.
string CsvEscape(string? texto)
{
    texto ??= "";
    bool requiereComillas = texto.Contains(';') || texto.Contains('"') || texto.Contains('\n') || texto.Contains('\r');
    if (requiereComillas)
    {
        string escapado = texto.Replace("\"", "\"\"");
        return $"\"{escapado}\"";
    }
    return texto;
}

void ListarGastosEsteMes()
{
    Console.Clear();
    Console.WriteLine("=== Gastos de ESTE MES ===");

    var hoy = DateTime.Today;
    var inicio = new DateTime(hoy.Year, hoy.Month, 1);
    var finExclusivo = inicio.AddMonths(1); // [inicio, fin)

    var cultura = CultureInfo.GetCultureInfo("es-AR");
    var lista = gastos
        .Where(g => g.Fecha >= inicio && g.Fecha < finExclusivo)
        .OrderBy(g => g.Fecha)
        .ThenBy(g => g.Id)
        .ToList();

    if (lista.Count == 0)
    {
        Console.WriteLine("(Sin registros en este mes)");
        return;
    }

    Console.WriteLine("ID  | Fecha       | Monto        | Descripción");
    Console.WriteLine("----+-------------+--------------+-------------------------");
    foreach (var g in lista)
    {
        Console.WriteLine($"{g.Id,-3} | {g.Fecha:yyyy-MM-dd} | {g.Monto.ToString("C", cultura),-12} | {g.Descripcion}");
    }

    decimal total = lista.Sum(g => g.Monto);
    Console.WriteLine("\n-----------------------------------------------");
    Console.WriteLine($"Total del mes: {total.ToString("C", cultura)}");
}

void ListarGastosEntreFechas()
{
    Console.Clear();
    Console.WriteLine("=== Gastos ENTRE FECHAS ===");
    Console.WriteLine("Formato: AAAA-MM-DD (ej: 2025-08-10)\n");

    // 1) Pedir fechas con validación
    DateTime desde;
    while (true)
    {
        Console.Write("Desde: ");
        string? sDesde = Console.ReadLine();
        if (DateTime.TryParse(sDesde, out desde))
            break;
        Console.WriteLine("Fecha inválida. Probá de nuevo.");
    }

    DateTime hasta;
    while (true)
    {
        Console.Write("Hasta (INCLUSIVE): ");
        string? sHasta = Console.ReadLine();
        if (DateTime.TryParse(sHasta, out hasta))
            break;
        Console.WriteLine("Fecha inválida. Probá de nuevo.");
    }

    // Normalizo por si el usuario invierte el orden
    if (hasta < desde)
    {
        var tmp = desde;
        desde = hasta;
        hasta = tmp;
    }

    var cultura = CultureInfo.GetCultureInfo("es-AR");

    // 2) Incluimos "hasta" de forma inclusiva
    var finExclusivo = hasta.AddDays(1);

    var lista = gastos
        .Where(g => g.Fecha >= desde && g.Fecha < finExclusivo)
        .OrderBy(g => g.Fecha)
        .ThenBy(g => g.Id)
        .ToList();

    Console.WriteLine($"\nRango: {desde:yyyy-MM-dd} a {hasta:yyyy-MM-dd}");

    if (lista.Count == 0)
    {
        Console.WriteLine("(Sin registros en el rango elegido)");
        return;
    }

    Console.WriteLine("\nID  | Fecha       | Monto        | Descripción");
    Console.WriteLine("----+-------------+--------------+-------------------------");
    foreach (var g in lista)
    {
        Console.WriteLine($"{g.Id,-3} | {g.Fecha:yyyy-MM-dd} | {g.Monto.ToString("C", cultura),-12} | {g.Descripcion}");
    }

    decimal total = lista.Sum(g => g.Monto);
    Console.WriteLine("\n-----------------------------------------------");
    Console.WriteLine($"Total en el rango: {total.ToString("C", cultura)}");
}

void EditarGasto()
{
    if (gastos.Count == 0)
    {
        Console.WriteLine("\nNo hay gastos para editar.");
        Pausa();
        return;
    }

    // 1) Mostrar para elegir bien el ID
    ListarGastos();
    Console.WriteLine();
    Console.Write("Ingresá el ID a editar: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("ID inválido.");
        Pausa();
        return;
    }

    var gasto = gastos.FirstOrDefault(g => g.Id == id);
    if (gasto == null)
    {
        Console.WriteLine("No se encontró un gasto con ese ID.");
        Pausa();
        return;
    }

    // 2) Pedir nuevos valores (ENTER = dejar igual)
    Console.WriteLine($"\nEditando ID {gasto.Id}:");
    Console.WriteLine($"Descripción actual: {gasto.Descripcion}");
    Console.Write("Nueva descripción (ENTER para dejar igual): ");
    string? nuevaDesc = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(nuevaDesc))
        nuevaDesc = nuevaDesc.Trim();

    decimal nuevoMonto = gasto.Monto;
    while (true)
    {
        Console.WriteLine($"\nMonto actual: {gasto.Monto}");
        Console.Write("Nuevo monto (ENTER para dejar igual): ");
        string? entradaMonto = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entradaMonto))
            break; // mantiene

        if (decimal.TryParse(entradaMonto, NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var m) ||
            decimal.TryParse(entradaMonto.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out m))
        {
            nuevoMonto = m;
            break;
        }
        Console.WriteLine("Monto inválido, probá de nuevo.");
    }

    DateTime nuevaFecha = gasto.Fecha;
    while (true)
    {
        Console.WriteLine($"\nFecha actual: {gasto.Fecha:yyyy-MM-dd}");
        Console.Write("Nueva fecha (AAAA-MM-DD, ENTER para dejar igual): ");
        string? entradaFecha = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entradaFecha))
            break; // mantiene

        if (DateTime.TryParse(entradaFecha, out var f))
        {
            nuevaFecha = f;
            break;
        }
        Console.WriteLine("Fecha inválida, probá de nuevo.");
    }

    // 3) Confirmación
    string descFinal = string.IsNullOrWhiteSpace(nuevaDesc) ? gasto.Descripcion : nuevaDesc;
    Console.WriteLine("\nResumen de cambios:");
    Console.WriteLine($"Descripción: {gasto.Descripcion}  =>  {descFinal}");
    Console.WriteLine($"Monto:       {gasto.Monto}       =>  {nuevoMonto}");
    Console.WriteLine($"Fecha:       {gasto.Fecha:yyyy-MM-dd} =>  {nuevaFecha:yyyy-MM-dd}");
    Console.Write("\n¿Guardar cambios? (s/n): ");
    var resp = Console.ReadLine()?.Trim().ToLower();
    if (resp != "s" && resp != "si" && resp != "sí")
    {
        Console.WriteLine("Operación cancelada.");
        Pausa();
        return;
    }

    // 4) Aplicar y persistir
    gasto.Descripcion = descFinal;
    gasto.Monto = nuevoMonto;
    gasto.Fecha = nuevaFecha;
    Guardar();

    Console.WriteLine("✅ Cambios guardados.");
    Pausa();
}

void ListarGastos()
{
    Console.Clear();
    Console.WriteLine("=== Lista de gastos ===");

    if (gastos.Count == 0)
    {
        Console.WriteLine("(Sin registros)");
        return;
    }

    var cultura = CultureInfo.GetCultureInfo("es-AR");
    Console.WriteLine("ID  | Fecha       | Monto        | Descripción");
    Console.WriteLine("----+-------------+--------------+-------------------------");

    foreach (var g in gastos.OrderBy(g => g.Fecha).ThenBy(g => g.Id))
    {
        Console.WriteLine($"{g.Id,-3} | {g.Fecha:yyyy-MM-dd} | {g.Monto.ToString("C", cultura),-12} | {g.Descripcion}");
    }
}

void MostrarTotal()
{
    var cultura = CultureInfo.GetCultureInfo("es-AR");
    decimal total = gastos.Sum(g => g.Monto);
    Console.WriteLine($"Total gastado: {total.ToString("C", cultura)}");
}

void EliminarGasto()
{
    if (gastos.Count == 0)
    {
        Console.WriteLine("\nNo hay gastos para eliminar.");
        Pausa();
        return;
    }

    ListarGastos();
    Console.WriteLine();
    Console.Write("Ingresá el ID a eliminar: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("ID inválido.");
        Pausa();
        return;
    }

    var gasto = gastos.FirstOrDefault(g => g.Id == id);
    if (gasto == null)
    {
        Console.WriteLine("No se encontró un gasto con ese ID.");
        Pausa();
        return;
    }

    Console.Write($"¿Eliminar '{gasto.Descripcion}' de {gasto.Fecha:yyyy-MM-dd} por {gasto.Monto}? (s/n): ");
    var resp = Console.ReadLine()?.Trim().ToLower();
    if (resp != "s" && resp != "si" && resp != "sí")
    {
        Console.WriteLine("Operación cancelada.");
        Pausa();
        return;
    }

    gastos.Remove(gasto);
    Guardar();
    Console.WriteLine("🗑️  Gasto eliminado.");
    Pausa();
}


void Guardar()
{
    string json = JsonSerializer.Serialize(gastos, jsonOptions);
    File.WriteAllText(archivo, json);
}

void Cargar()
{
    if (!File.Exists(archivo)) return;

    try
    {
        var json = File.ReadAllText(archivo);
        var lista = JsonSerializer.Deserialize<List<Gasto>>(json) ?? new List<Gasto>();
        gastos = lista;
        siguienteId = (gastos.Count == 0) ? 1 : gastos.Max(g => g.Id) + 1;
    }
    catch
    {
        // Si algo falla al leer, inicializo vacío para no romper la app
        gastos = new List<Gasto>();
        siguienteId = 1;
    }
}

void Pausa()
{
    Console.WriteLine("\nPresioná ENTER para continuar...");
    Console.ReadLine();
}
