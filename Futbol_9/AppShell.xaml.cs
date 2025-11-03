namespace Futbol_9
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Registrar todas las rutas para la navegación
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("campeonato", typeof(Vistas.PaginaCampeonato));
            Routing.RegisterRoute("equipos", typeof(Vistas.PaginaEquipos));
            Routing.RegisterRoute("jugadores", typeof(Vistas.PaginaJugadores));
            Routing.RegisterRoute("grupos", typeof(Vistas.PaginaGrupos));
            Routing.RegisterRoute("planes", typeof(Vistas.PaginaPlanes));
            Routing.RegisterRoute("pagos", typeof(Vistas.PaginaRegistrarPagos));
            Routing.RegisterRoute("equipos-resumen", typeof(Vistas.PaginaResumenEquipos));
            Routing.RegisterRoute("estado", typeof(Vistas.PaginaEstadoEquipos));
            Routing.RegisterRoute("asignacion", typeof(Vistas.PaginaAsignacionSorteo));
        }
    }
}
