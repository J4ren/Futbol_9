using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace Futbol_9.Modelos
{
    [Table("EquiposGrupos")]
    public class EquipoGrupo
    {
        [PrimaryKey, AutoIncrement]
        public int EquipoGrupoId { get; set; }

        [Indexed]
        public int GrupoId { get; set; }

        [Indexed]
        public int EquipoId { get; set; }
    }
}
