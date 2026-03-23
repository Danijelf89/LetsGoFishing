using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LetsGoFishing.Models;
using LetsGoFishing.Services;

namespace LetsGoFishing.ViewModels;

/// <summary>
/// ViewModel za stranicu podešavanja.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;
    private readonly WeatherService _weatherService;

    [ObservableProperty]
    private ObservableCollection<CityInfo> _availableCities = [];

    [ObservableProperty]
    private CityInfo? _selectedCity;

    [ObservableProperty]
    private bool _isDarkTheme = true;

    public SettingsViewModel(SettingsService settingsService, WeatherService weatherService)
    {
        _settingsService = settingsService;
        _weatherService = weatherService;
        Title = "Podešavanja";
    }

    partial void OnSelectedCityChanged(CityInfo? value)
    {
        if (value != null)
        {
            _ = SaveWeatherSettingsAsync();
        }
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _ = SaveThemeAsync();
        ApplyTheme();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await _settingsService.LoadSettingsAsync();
            var settings = _settingsService.Settings;

            // Load available cities
            AvailableCities.Clear();
            foreach (var city in _weatherService.GetAvailableCities())
            {
                AvailableCities.Add(city);
            }

            // Set current values
            SelectedCity = AvailableCities.FirstOrDefault(c => c.Name == settings.SelectedCity);
            IsDarkTheme = settings.IsDarkTheme;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveWeatherSettingsAsync()
    {
        if (SelectedCity == null) return;

        await _settingsService.UpdateCityAsync(SelectedCity.Name);
        await _weatherService.SetCityAsync(SelectedCity, forceRefresh: true);
    }

    private async Task SaveThemeAsync()
    {
        await _settingsService.UpdateThemeAsync(IsDarkTheme);
    }

    private void ApplyTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
