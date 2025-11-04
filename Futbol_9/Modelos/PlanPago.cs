using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace Futbol_9.Modelos
{
    [Table("PlanesPago")]
    public class PlanPago
    {
        [PrimaryKey, AutoIncrement]
        public int PlanPagoId { get; set; }

        [Indexed]
        public int EquipoId { get; set; }

        public TipoPlanPago Tipo { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime LimitePago { get; set; } = DateTime.Today.AddMonths(1);

        // Info estado
        public decimal MontoPagado { get; set; } = 0m;
    }
}
