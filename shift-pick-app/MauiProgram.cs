using shift_pick_app.Services;
using Microsoft.Extensions.Logging;
using shift_pick_app.ViewModels;
using shift_pick_app.Views;

namespace shift_pick_app
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
                    fonts.AddFont("MPLUSRounded1c-Regular.ttf", "MPLUSRounded1cRegular");
                    fonts.AddFont("MPLUSRounded1c-Bold.ttf", "MPLUSRounded1cBold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<NavigationService>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<PasswordReset>();
            builder.Services.AddTransient<PasswordResetViewModel>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<HandySettingPage>();
            builder.Services.AddTransient<HandySettingPageViewModel>();

            return builder.Build();
        }
    }
}
