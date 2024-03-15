using mar_sumaken_app.Services;
using Microsoft.Extensions.Logging;
using mar_sumaken_app.Views;

namespace mar_sumaken_app
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
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FontAwesome6Regular");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FontAwesome6Solid");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<LoadingPage>();

            return builder.Build();
        }
    }
}
