using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

 namespace Futbol_9.Modelos
 {
    [Table("Grupos")]
    public class Grupo
    {
        [PrimaryKey, AutoIncrement]
        public int GrupoId { get; set; }

        [Indexed, NotNull]
        public int CampeonatoId { get; set; }

        [NotNull]
        public string Nombre { get; set; } = string.Empty; // "A","B","C"...
    }
 }

