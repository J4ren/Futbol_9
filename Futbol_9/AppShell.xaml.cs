namespace Futbol_9
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("equipos", typeof(Vistas.PaginaEquipos));
        }
    }
}
