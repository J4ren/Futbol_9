using Futbol_9.Servicios;
using Microsoft.Extensions.Logging;

namespace Futbol_9
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Inyección de dependencias
            builder.Services.AddSingleton<ServicioBaseDatos>();

            return builder.Build();
        }
    }
}
