using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace Futbol_9.Modelos
{
    [Table("Cuotas")]
    public class Cuota
    {
        [PrimaryKey, AutoIncrement]
        public int CuotaId { get; set; }

        [Indexed]
        public int PlanPagoId { get; set; }

        public int Numero { get; set; }              
        public decimal Importe { get; set; }        
        public DateTime FechaVencimiento { get; set; }
        public bool Pagada { get; set; } = false;
        public DateTime? FechaPago { get; set; }
    }
}
