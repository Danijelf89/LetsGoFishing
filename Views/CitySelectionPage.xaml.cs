using LetsGoFishing.Models;
using LetsGoFishing.Services;

namespace LetsGoFishing.Views;

public partial class CitySelectionPage : ContentPage
{
    private readonly WeatherService _weatherService;
    private readonly SettingsService _settingsService;
    private List<CityInfo> _allCities = [];
    private CityInfo? _selectedCity;

    public CitySelectionPage(WeatherService weatherService, SettingsService settingsService)
    {
        InitializeComponent();
        _weatherService = weatherService;
        _settingsService = settingsService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCities();
    }

    private void LoadCities()
    {
        _allCities = _weatherService.GetAvailableCities();
        CityList.ItemsSource = _allCities;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CityList.ItemsSource = _allCities;
        }
        else
        {
            CityList.ItemsSource = _allCities
                .Where(c => c.Name.ToLowerInvariant().Contains(searchText))
                .ToList();
        }
    }

    private void OnCitySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CityInfo city)
        {
            _selectedCity = city;
            SelectedCityLabel.Text = $"Izabrano: {city.Name}";
            ConfirmButton.IsEnabled = true;
        }
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main/Calendar");
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_selectedCity == null) return;

        await _settingsService.UpdateWeatherSettingsAsync(_selectedCity.Name, 6);
        await _weatherService.SetCityAsync(_selectedCity);
        await Shell.Current.GoToAsync("//Main/Calendar");
    }
}
