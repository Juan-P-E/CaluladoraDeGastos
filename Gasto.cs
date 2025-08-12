using System;

namespace CalculadoraDeGastos
{
    public class Gasto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
    }
}
