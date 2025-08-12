# Calculadora de Gastos (C# / .NET 8)

Aplicación de consola para registrar gastos simples. Permite anotar, listar, editar y eliminar gastos; ver el total; filtrar por fechas y exportar todo a CSV. Los datos se guardan en `gastos.json`.

## Funcionalidades
- Agregar gasto (descripción, monto y fecha opcional).
- Listar gastos en una tabla ordenada por fecha.
- Ver total gastado.
- Editar y eliminar por ID (con confirmación).
- Filtros: **este mes** y **entre fechas** (incluye el día “hasta”).
- Exportar **todos** los gastos a `gastos.csv` para abrir en Excel/Sheets.

## Requisitos
- .NET 8 (SDK) instalado.
- Windows, Linux o macOS.

## Cómo ejecutar
Desde la carpeta del proyecto:
```bash
dotnet run


También podés abrir el proyecto en Visual Studio y presionar F5.

Uso
El menú te guía con opciones numeradas.

El monto acepta coma o punto (ej.: 123,45 o 123.45).

Si dejás la fecha vacía, se usa la de hoy.

Cada gasto tiene un ID incremental para poder editar/eliminar.

El guardado es automático en gastos.json.

Exportar a CSV
Opción del menú: “Exportar TODOS los gastos a CSV”.

Genera gastos.csv con separador ; y montos en formato numérico (sin símbolo $).

Excel/Sheets lo abre sin configuración adicional.

Archivos principales
Program.cs: lógica del menú y operaciones.

Gasto.cs: modelo con Id, Descripcion, Monto, Fecha.

gastos.json: almacenamiento local de datos.

gastos.csv: exportación para planillas.

Ideas para mejorar (a futuro)
Categorías de gastos y totales por categoría.

Presupuesto mensual y alerta al superarlo.

Resumen por mes/año.

Copiar
Editar


