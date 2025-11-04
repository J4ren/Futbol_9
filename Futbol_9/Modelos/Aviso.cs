using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Futbol_9.Modelos
{
    [Table("Avisos")]
    public class Aviso
    {
        [PrimaryKey, AutoIncrement]
        public int AvisoId { get; set; }

        [NotNull]
        public string Titulo { get; set; } = string.Empty;

        [NotNull]
        public string Mensaje { get; set; } = string.Empty;

        [NotNull]
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
