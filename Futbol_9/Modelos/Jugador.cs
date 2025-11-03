using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Futbol_9.Modelos
{
    [Table("Jugadores")]
    public class Jugador
    {
        [PrimaryKey, AutoIncrement]
        public int JugadorId { get; set; }

        [Indexed]
        public int EquipoId { get; set; }

        [NotNull]
        public string NombreCompleto { get; set; } = string.Empty;

        [NotNull]
        public string Documento { get; set; } = string.Empty;      
        [NotNull]
        public string TelefonoMovil { get; set; } = string.Empty;  
       
        [NotNull]
        public int NumeroCamiseta { get; set; }
    }
}
