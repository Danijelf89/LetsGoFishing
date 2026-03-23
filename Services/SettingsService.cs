using System.Text.Json;
using LetsGoFishing.Models;

namespace LetsGoFishing.Services;

/// <summary>
/// Upravlja perzistencijom korisničkih podešavanja.
/// </summary>
public class SettingsService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _settingsFilePath;
    private UserSettings _settings = new();

    public UserSettings Settings => _settings;

    public SettingsService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<UserSettings>(json, _jsonOptions) ?? new UserSettings();
            }
            else
            {
                _settings = new UserSettings();
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings load error: {ex.Message}");
            _settings = new UserSettings();
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings save error: {ex.Message}");
        }
    }

    public async Task UpdateSelectedFishAsync(IEnumerable<string> selectedFish)
    {
        _settings.SelectedFish = selectedFish.ToList();
        await SaveSettingsAsync();
    }

    public async Task UpdateNotificationSettingsAsync(bool enabled, TimeSpan? time = null)
    {
        _settings.NotificationsEnabled = enabled;
        if (time.HasValue)
        {
            _settings.NotificationTime = time.Value;
        }
        await SaveSettingsAsync();
    }

    public async Task UpdateThemeAsync(bool isDarkTheme)
    {
        _settings.IsDarkTheme = isDarkTheme;
        await SaveSettingsAsync();
    }

    public async Task UpdateWeatherSettingsAsync(string city, int refreshHours)
    {
        _settings.SelectedCity = city;
        _settings.WeatherRefreshHours = refreshHours;
        _settings.HasSelectedCity = true;
        await SaveSettingsAsync();
    }

    public async Task UpdateCityAsync(string city)
    {
        _settings.SelectedCity = city;
        _settings.HasSelectedCity = true;
        await SaveSettingsAsync();
    }
}
