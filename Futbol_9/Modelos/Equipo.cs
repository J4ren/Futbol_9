using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Futbol_9.Modelos
{
    [Table("Equipos")]
    public class Equipo
    {
        [PrimaryKey, AutoIncrement]
        public int EquipoId { get; set; }

        [Indexed, NotNull]
        public int CampeonatoId { get; set; }     

        [Unique, NotNull]
        public string Nombre { get; set; } = string.Empty;

        // Delegado 
        [NotNull]
        public string DelegadoNombre { get; set; } = string.Empty;

        public string? DelegadoDocumento { get; set; }  
        public string? DelegadoTelefono { get; set; }

        // Fechas informativas 
        public DateTime FechaInicioInscripcion { get; set; } = DateTime.Today;
        public DateTime FechaFinInscripcion { get; set; } = DateTime.Today.AddMonths(1);
    }
}
