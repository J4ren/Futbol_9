using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futbol_9.Modelos
{
    public class GrupoConEquipos
    {
        public int GrupoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<Equipo> Equipos { get; set; } = new();
    }
}