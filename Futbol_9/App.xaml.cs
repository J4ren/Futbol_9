using Futbol_9.Servicios;

namespace Futbol_9
{
    public partial class App : Application
    {
        public App(ServicioBaseDatos db)
        {
            InitializeComponent();
            _ = Inicializar(db);

            Console.WriteLine(FileSystem.AppDataDirectory);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private static async Task Inicializar(ServicioBaseDatos db)
        {
            await db.InicializarAsync();
        }
    }

}

