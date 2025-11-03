using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futbol_9.Modelos
{
    public class ResumenEquipo
    {


        public int EquipoId { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string Delegado { get; set; } = string.Empty;
        public int Jugadores { get; set; }
        public bool Habilitado { get; set; } 
        public string EstadoTexto => Habilitado ? "Habilitado" : "No habilitado";
    }
}
