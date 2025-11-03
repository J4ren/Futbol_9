using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futbol_9.Modelos
{
    public class ResumenPlan
    {
        public int PlanPagoId { get; set; }
        public int EquipoId { get; set; }
        public string EquipoNombre { get; set; } = string.Empty;

        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal Saldo => Total - Pagado;

        public DateTime LimitePago { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pagado | Pendiente | Vencido
    }
}