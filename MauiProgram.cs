using LetsGoFishing.Services;
using LetsGoFishing.ViewModels;
using LetsGoFishing.Views;
using Microsoft.Extensions.Logging;

namespace LetsGoFishing;

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

        // Register services
        builder.Services.AddSingleton<FishSpeciesService>();
        builder.Services.AddSingleton<WeatherService>();
        builder.Services.AddSingleton<DiaryService>();
        builder.Services.AddSingleton<SettingsService>();

        // Register ViewModels
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<LovostajViewModel>();
        builder.Services.AddTransient<DiaryViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<HelpViewModel>();

        // Register Views
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<CitySelectionPage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<LovostajPage>();
        builder.Services.AddTransient<DiaryPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<HelpPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
