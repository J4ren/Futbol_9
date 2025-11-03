using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace Futbol_9.Modelos
{
    [Table("Campeonatos")]
    public class Campeonato
    {
        [PrimaryKey, AutoIncrement]
        public int CampeonatoId { get; set; }

        [Unique, NotNull]
        public string Nombre { get; set; } = "Campeonato Local";

        // Ventana del campeonato 
        public DateTime FechaInicioCampeonato { get; set; } = DateTime.Today;
        public DateTime FechaFinCampeonato { get; set; } = DateTime.Today.AddMonths(2);

        // Ventana de inscripciones/pagos 
        public DateTime InscripcionInicio { get; set; } = DateTime.Today;
        public DateTime InscripcionFin { get; set; } = DateTime.Today.AddMonths(1);

        // Reglas básicas
        public decimal MontoInscripcion { get; set; } = 100m;
        public bool PermiteCuotas { get; set; } = true;

        // Para manejar uno activo
        public bool Activo { get; set; } = true;
    }
}