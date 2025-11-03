using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Futbol_9.Datos;
using Futbol_9.Modelos;
using SQLite;

namespace Futbol_9.Servicios
{
    public class ServicioBaseDatos
    {
        private SQLiteAsyncConnection _db = null!;

        public async Task InicializarAsync()
        {
            _db = await ConexionSqlite.ObtenerConexionAsync();
            await _db.CreateTableAsync<Equipo>();
            await _db.CreateTableAsync<PlanPago>();
            await _db.CreateTableAsync<Cuota>();
            await _db.CreateTableAsync<Campeonato>();
            await _db.CreateTableAsync<Jugador>();
            await _db.CreateTableAsync<Grupo>();
            await _db.CreateTableAsync<EquipoGrupo>();
        }

        // --CRUD --
        public Task<int> InsertarEquipoAsync(Equipo e) => _db.InsertAsync(e);
        public Task<Equipo?> ObtenerEquipoPorNombreAsync(string nombre) =>
            _db.Table<Equipo>().Where(x => x.Nombre == nombre).FirstOrDefaultAsync();
        public Task<List<Equipo>> ListarEquiposAsync() =>
            _db.Table<Equipo>().OrderBy(x => x.Nombre).ToListAsync();
        public Task<int> InsertarPlanAsync(PlanPago p) => _db.InsertAsync(p);
        public Task<int> ActualizarPlanAsync(PlanPago p) => _db.UpdateAsync(p);
        public Task<PlanPago?> ObtenerPlanPorIdAsync(int id) =>
            _db.Table<PlanPago>().Where(x => x.PlanPagoId == id).FirstOrDefaultAsync();

        public Task<List<PlanPago>> ListarPlanesPorEquipoAsync(int equipoId) =>
            _db.Table<PlanPago>().Where(p => p.EquipoId == equipoId).ToListAsync();

        // -- Cuotas --
        public Task<int> InsertarCuotasAsync(IEnumerable<Cuota> cuotas) => _db.InsertAllAsync(cuotas);
        public Task<List<Cuota>> ListarCuotasPorPlanAsync(int planId) =>
            _db.Table<Cuota>().Where(c => c.PlanPagoId == planId).OrderBy(c => c.Numero).ToListAsync();
        public async Task RegistrarPagoDeCuotaAsync(int cuotaId)
        {
            var cuota = await _db.Table<Cuota>().Where(c => c.CuotaId == cuotaId).FirstOrDefaultAsync();
            if (cuota is null || cuota.Pagada) return;

            cuota.Pagada = true;
            cuota.FechaPago = DateTime.Now;
            await _db.UpdateAsync(cuota);

            // Actualiza acumulado en el plan
            var plan = await ObtenerPlanPorIdAsync(cuota.PlanPagoId);
            if (plan is not null)
            {
                plan.MontoPagado += cuota.Importe;
                await _db.UpdateAsync(plan);
            }
        }
        // -- planes --
        public Task<List<PlanPago>> ListarPlanesAsync() =>
            _db.Table<PlanPago>().ToListAsync();
        public async Task<decimal> CalcularPagadoDesdeCuotasAsync(int planId)
        {
            var cuotas = await ListarCuotasPorPlanAsync(planId);
            return cuotas.Where(c => c.Pagada).Sum(c => c.Importe);
        }
        public async Task<List<ResumenPlan>> ListarResumenesAsync()
        {
            var planes = await ListarPlanesAsync();
            var equipos = await ListarEquiposAsync();

            var tareas = planes.Select(async p =>
            {
                var equipo = equipos.FirstOrDefault(e => e.EquipoId == p.EquipoId);
                var pagado = await CalcularPagadoDesdeCuotasAsync(p.PlanPagoId);
                var saldo = p.MontoTotal - pagado;

                var estado = "Pendiente";
                if (saldo <= 0m) estado = "Pagado";
                else if (DateTime.Today > p.LimitePago) estado = "Vencido";

                return new ResumenPlan
                {
                    PlanPagoId = p.PlanPagoId,
                    EquipoId = p.EquipoId,
                    EquipoNombre = equipo?.Nombre ?? $"Equipo {p.EquipoId}",
                    Total = p.MontoTotal,
                    Pagado = pagado,
                    LimitePago = p.LimitePago,
                    Estado = estado
                };
            });

            var lista = await Task.WhenAll(tareas);
            // Orden alfabético por equipo
            return lista.OrderBy(x => x.EquipoNombre).ToList();
        }
       

        // -- Campeonatos --
        public Task<int> InsertarCampeonatoAsync(Campeonato c) => _db.InsertAsync(c);
        public Task<int> ActualizarCampeonatoAsync(Campeonato c) => _db.UpdateAsync(c);
        public Task<List<Campeonato>> ListarCampeonatosAsync() =>
            _db.Table<Campeonato>().OrderByDescending(x => x.Activo).ThenBy(x => x.Nombre).ToListAsync();

        public Task<Campeonato?> ObtenerCampeonatoActivoAsync() =>
            _db.Table<Campeonato>().Where(x => x.Activo).FirstOrDefaultAsync();
       
        public async Task EstablecerComoActivoAsync(int campeonatoId)
        {
            var todos = await ListarCampeonatosAsync();
            foreach (var c in todos)
            {
                c.Activo = (c.CampeonatoId == campeonatoId);
            }
            await _db.UpdateAllAsync(todos);
        }
        // -- Jugadores --
        public Task<int> InsertarJugadorAsync(Jugador j) => _db.InsertAsync(j);
        public Task<int> EliminarJugadorAsync(int jugadorId) => _db.DeleteAsync<Jugador>(jugadorId);

        public Task<List<Jugador>> ListarJugadoresPorEquipoAsync(int equipoId) =>
            _db.Table<Jugador>().Where(j => j.EquipoId == equipoId).OrderBy(j => j.NombreCompleto).ToListAsync();

        public async Task<int> ContarJugadoresPorEquipoAsync(int equipoId)
        {
            var cmd = _db.Table<Jugador>().Where(j => j.EquipoId == equipoId);
            return (await cmd.ToListAsync()).Count;
        }
        // -- Helpers de estado de pago por equipo --
        public async Task<decimal> TotalPagadoPorEquipoAsync(int equipoId)
        {
            var planes = await ListarPlanesPorEquipoAsync(equipoId);
            decimal pagado = 0m;

            foreach (var p in planes)
            {
                var cuotas = await ListarCuotasPorPlanAsync(p.PlanPagoId);
                pagado += cuotas.Where(c => c.Pagada).Sum(c => c.Importe);
            }
            return pagado;
        }

        public async Task<decimal> TotalAPagarPorEquipoAsync(int equipoId)
        {
            var planes = await ListarPlanesPorEquipoAsync(equipoId);
            return planes.Sum(p => p.MontoTotal);
        }

        public async Task<bool> EquipoHabilitadoParaJugadoresAsync(int equipoId)
        {
            var total = await TotalAPagarPorEquipoAsync(equipoId);
            if (total <= 0) return false; // retorna aun sin plan
            var pagado = await TotalPagadoPorEquipoAsync(equipoId);
            return pagado >= total;
        }

        public async Task<List<Equipo>> ListarEquiposHabilitadosAsync()
        {
            var equipos = await ListarEquiposAsync();
            var habilitados = new List<Equipo>();
            foreach (var e in equipos)
            {
                if (await EquipoHabilitadoParaJugadoresAsync(e.EquipoId))
                    habilitados.Add(e);
            }
            return habilitados.OrderBy(x => x.Nombre).ToList();
        }
        //Metodo para armar el resumen de equipos registradods
        public async Task<List<ResumenEquipo>> ListarResumenEquiposAsync()
        {
            var equipos = await ListarEquiposAsync();
            var lista = new List<ResumenEquipo>();

            foreach (var e in equipos)
            {
                var cantJug = (await ListarJugadoresPorEquipoAsync(e.EquipoId)).Count;
                var habil = await EquipoHabilitadoParaJugadoresAsync(e.EquipoId);

                lista.Add(new ResumenEquipo
                {
                    EquipoId = e.EquipoId,
                    NombreEquipo = e.Nombre,
                    Delegado = e.DelegadoNombre,
                    Jugadores = cantJug,
                    Habilitado = habil
                });
            }

            return lista.OrderBy(x => x.NombreEquipo).ToList();
        }
        // -- Grupos --
        public Task<List<Grupo>> ListarGruposPorCampeonatoAsync(int campeonatoId) =>
            _db.Table<Grupo>().Where(g => g.CampeonatoId == campeonatoId).OrderBy(g => g.Nombre).ToListAsync();

        public Task<List<EquipoGrupo>> ListarEquiposDeGrupoAsync(int grupoId) =>
            _db.Table<EquipoGrupo>().Where(eg => eg.GrupoId == grupoId).ToListAsync();

        public async Task LimpiarGruposAsync(int campeonatoId)
        {
            var grupos = await ListarGruposPorCampeonatoAsync(campeonatoId);
            var ids = grupos.Select(g => g.GrupoId).ToList();
            foreach (var gid in ids)
                await _db.ExecuteAsync("DELETE FROM EquiposGrupos WHERE GrupoId = ?", gid);
            await _db.ExecuteAsync("DELETE FROM Grupos WHERE CampeonatoId = ?", campeonatoId);
        }

        public async Task CrearGruposAsync(int campeonatoId, IEnumerable<string> nombres)
        {
            foreach (var n in nombres)
                await _db.InsertAsync(new Grupo { CampeonatoId = campeonatoId, Nombre = n });
        }

        public async Task AsignarEquipoAGrupoAsync(int grupoId, int equipoId)
        {
            await _db.InsertAsync(new EquipoGrupo { GrupoId = grupoId, EquipoId = equipoId });
        }

        // ====== Equipos por campeonato ======
        public async Task<List<Equipo>> ListarEquiposPorCampeonatoAsync(int campeonatoId)
        {
            var todos = await ListarEquiposAsync();
            return todos.Where(e => e.CampeonatoId == campeonatoId).OrderBy(e => e.Nombre).ToList();
        }

        // == Asignaciones (grupos/equipos) por campeonato ====
        public async Task<List<EquipoGrupo>> ListarAsignacionesPorCampeonatoAsync(int campeonatoId)
        {
            var grupos = await ListarGruposPorCampeonatoAsync(campeonatoId);
            if (grupos.Count == 0) return new List<EquipoGrupo>();

            var ids = grupos.Select(g => g.GrupoId).ToList();
            var asignaciones = new List<EquipoGrupo>();
            foreach (var gid in ids)
                asignaciones.AddRange(await ListarEquiposDeGrupoAsync(gid));
            return asignaciones;
        }

        public async Task<bool> EquipoAsignadoEnCampeonatoAsync(int campeonatoId, int equipoId)
        {
            var asignaciones = await ListarAsignacionesPorCampeonatoAsync(campeonatoId);
            return asignaciones.Any(a => a.EquipoId == equipoId);
        }

        public Task<int> QuitarAsignacionDeEquipoAsync(int equipoId) =>
            _db.ExecuteAsync("DELETE FROM EquiposGrupos WHERE EquipoId = ?", equipoId);

        public async Task<List<Equipo>> ListarEquiposNoAsignadosAsync(int campeonatoId)
        {
            var equipos = await ListarEquiposPorCampeonatoAsync(campeonatoId);
            var asignados = await ListarAsignacionesPorCampeonatoAsync(campeonatoId);
            var setAsignados = asignados.Select(a => a.EquipoId).ToHashSet();
            return equipos.Where(e => !setAsignados.Contains(e.EquipoId)).OrderBy(e => e.Nombre).ToList();
        }
    }
}
