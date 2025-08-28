using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace MediSys
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

			// Registrar servicios
			builder.Services.AddSingleton<Services.MediSysApiService>();

			// Registrar ViewModels
			builder.Services.AddTransient<ViewModels.ConsultarMedicoViewModel>();
			builder.Services.AddTransient<ViewModels.DashboardViewModel>();
            builder.Services.AddTransient<ViewModels.CrearCitaViewModel>();
            builder.Services.AddTransient<ViewModels.RegistrarMedicoViewModel>();
			// Agregar otros ViewModels que uses

			// Registrar Views
			builder.Services.AddTransient<Views.Dashboard.ConsultarMedicoPage>();
            builder.Services.AddTransient<Views.Dashboard.CrearCitaPage>();
            builder.Services.AddTransient<Views.Dashboard.RegistrarMedicoPage>();
			// Agregar otras vistas problemáticas


#if DEBUG
			builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
