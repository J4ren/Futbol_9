using Futbol_9.Servicios;

namespace Futbol_9
{
    public partial class App : Application
    {

        public App(ServicioBaseDatos db)
        {
            InitializeComponent();
            MainPage = new AppShell();
            _ = Inicializar(db);

            Console.WriteLine(FileSystem.AppDataDirectory);

        }

        private static async Task Inicializar(ServicioBaseDatos db)
        {
            await db.InicializarAsync();
        }
    }

}

