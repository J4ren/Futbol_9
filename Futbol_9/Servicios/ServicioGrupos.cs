using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Futbol_9.Modelos;


namespace Futbol_9.Servicios
{
    public static class ServicioGrupos
    {
        // Genera nombres A, B, C...; si hay más de 26, usa G1..GN
        public static List<string> GenerarNombres(int cantidad)
        {
            var nombres = new List<string>();
            for (int i = 0; i < cantidad; i++)
                nombres.Add(i < 26 ? ((char)('A' + i)).ToString() : $"G{i + 1}");
            return nombres;
        }

        // Distribución "round-robin" simple: baraja y reparte 1,2,3... en grupos
        public static Dictionary<string, List<Equipo>> RepartirEquipos(List<Equipo> equipos, List<string> grupos)
        {
            var rnd = Random.Shared;
            equipos = equipos.OrderBy(_ => rnd.Next()).ToList();

            var asignacion = grupos.ToDictionary(g => g, _ => new List<Equipo>());
            int i = 0;
            foreach (var e in equipos)
            {
                var g = grupos[i % grupos.Count];
                asignacion[g].Add(e);
                i++;
            }
            return asignacion;
        }
    }
}