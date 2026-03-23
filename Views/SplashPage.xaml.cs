using LetsGoFishing.Services;

namespace LetsGoFishing.Views;

public partial class SplashPage : ContentPage
{
    private readonly SettingsService _settingsService;
    private bool _animationRunning = true;

    public SplashPage(SettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Start fish animation
        _ = AnimateFishAsync();

        // Load data
        LoadingText.Text = "Učitavanje podešavanja...";
        await _settingsService.LoadSettingsAsync();

        LoadingText.Text = "Priprema aplikacije...";
        await Task.Delay(500);

        _animationRunning = false;

        // Check if user has selected a city
        if (!_settingsService.Settings.HasSelectedCity)
        {
            // Navigate to city selection
            await Shell.Current.GoToAsync("//CitySelection");
        }
        else
        {
            // Navigate to main app
            await Shell.Current.GoToAsync("//Main/Calendar");
        }
    }

    private async Task AnimateFishAsync()
    {
        while (_animationRunning)
        {
            // Swim right
            await FishEmoji.TranslateToAsync(30, 10, 800, Easing.SinInOut);
            // Swim left
            await FishEmoji.TranslateToAsync(-30, -10, 800, Easing.SinInOut);
        }
    }
}
