using System.Text.Json;
using System.Text.Json.Serialization;
using LetsGoFishing.Models;

namespace LetsGoFishing.Services;

/// <summary>
/// Servis za vremensku prognozu koristeći Open-Meteo API.
/// </summary>
public class WeatherService
{
    private readonly HttpClient _httpClient;
    private List<DailyWeather> _forecast = [];
    private DateTime _lastFetch = DateTime.MinValue;
    private CityInfo _currentCity;
    private bool _isLoaded = false;
    private const string Timezone = "Europe/Belgrade";

    private static readonly List<CityInfo> _cities =
    [
        // Srbija
        new CityInfo("Novi Sad", 45.2671, 19.8335),
        new CityInfo("Beograd", 44.7866, 20.4489),
        new CityInfo("Niš", 43.3209, 21.8958),
        new CityInfo("Kragujevac", 44.0128, 20.9114),
        new CityInfo("Subotica", 46.1003, 19.6658),
        new CityInfo("Zrenjanin", 45.3816, 20.3826),
        new CityInfo("Pančevo", 44.8708, 20.6403),
        new CityInfo("Čačak", 43.8914, 20.3497),
        new CityInfo("Kraljevo", 43.7257, 20.6894),
        new CityInfo("Smederevo", 44.6628, 20.9272),
        new CityInfo("Leskovac", 42.9981, 21.9461),
        new CityInfo("Valjevo", 44.2767, 19.8914),
        new CityInfo("Kruševac", 43.5800, 21.3269),
        new CityInfo("Vranje", 42.5514, 21.9003),
        new CityInfo("Šabac", 44.7558, 19.6942),
        new CityInfo("Užice", 43.8586, 19.8489),
        new CityInfo("Sombor", 45.7736, 19.1125),
        new CityInfo("Požarevac", 44.6217, 21.1869),
        new CityInfo("Pirot", 43.1531, 22.5856),
        new CityInfo("Zaječar", 43.9042, 22.2858),
        new CityInfo("Kikinda", 45.8297, 20.4653),
        new CityInfo("Sremska Mitrovica", 44.9764, 19.6125),
        new CityInfo("Jagodina", 43.9772, 21.2611),
        new CityInfo("Vršac", 45.1167, 21.3033),
        new CityInfo("Bor", 44.0747, 22.0969),
        new CityInfo("Prokuplje", 43.2342, 21.5878),
        new CityInfo("Loznica", 44.5336, 19.2261),
        new CityInfo("Prijepolje", 43.3833, 19.6500),
        new CityInfo("Novi Pazar", 43.1367, 20.5122),
        new CityInfo("Aranđelovac", 44.3061, 20.5564),

        // Hrvatska
        new CityInfo("Zagreb", 45.8150, 15.9819),
        new CityInfo("Split", 43.5081, 16.4402),
        new CityInfo("Rijeka", 45.3271, 14.4422),
        new CityInfo("Osijek", 45.5550, 18.6955),
        new CityInfo("Zadar", 44.1194, 15.2314),
        new CityInfo("Pula", 44.8666, 13.8496),
        new CityInfo("Slavonski Brod", 45.1603, 18.0156),
        new CityInfo("Karlovac", 45.4929, 15.5553),
        new CityInfo("Varaždin", 46.3057, 16.3366),
        new CityInfo("Šibenik", 43.7350, 15.8952),
        new CityInfo("Dubrovnik", 42.6507, 18.0944)
    ];

    public WeatherService()
    {
        _httpClient = new HttpClient();
        _currentCity = _cities.First(c => c.Name == "Novi Sad");
    }

    public List<CityInfo> GetAvailableCities() => _cities.OrderBy(c => c.Name).ToList();

    public string GetCurrentCity() => _currentCity.Name;

    public async Task SetCityAsync(CityInfo city, bool forceRefresh = false)
    {
        var cityChanged = _currentCity.Name != city.Name;
        _currentCity = city;

        if (cityChanged)
        {
            _isLoaded = false;
        }

        // Load only if not loaded yet, city changed, or force refresh
        if (!_isLoaded || cityChanged || forceRefresh)
        {
            await RefreshForecastAsync();
        }
    }

    public bool IsLoaded => _isLoaded;

    public async Task RefreshForecastAsync()
    {
        _isLoaded = false;
        await GetForecastAsync();
    }

    public async Task<List<DailyWeather>> GetForecastAsync()
    {
        // If already loaded, return cached data
        if (_isLoaded && _forecast.Count > 0)
        {
            return _forecast;
        }

        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast?" +
                $"latitude={_currentCity.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                $"&longitude={_currentCity.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                $"&daily=temperature_2m_max,temperature_2m_min,weather_code,wind_speed_10m_max,sunrise,sunset" +
                $"&timezone={System.Uri.EscapeDataString(Timezone)}" +
                $"&forecast_days=16";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Daily != null)
            {
                _forecast.Clear();

                for (int i = 0; i < data.Daily.Time.Count; i++)
                {
                    var date = DateTime.Parse(data.Daily.Time[i]);
                    var weatherCode = data.Daily.WeatherCode.Count > i ? data.Daily.WeatherCode[i] : 0;

                    var sunrise = data.Daily.Sunrise.Count > i ? data.Daily.Sunrise[i] : "";
                    var sunset = data.Daily.Sunset.Count > i ? data.Daily.Sunset[i] : "";

                    _forecast.Add(new DailyWeather
                    {
                        Date = date,
                        TemperatureMax = data.Daily.TemperatureMax.Count > i ? data.Daily.TemperatureMax[i] : 0,
                        TemperatureMin = data.Daily.TemperatureMin.Count > i ? data.Daily.TemperatureMin[i] : 0,
                        WeatherCode = weatherCode,
                        Description = GetWeatherDescription(weatherCode),
                        Icon = GetWeatherIcon(weatherCode),
                        WindSpeedMax = data.Daily.WindSpeedMax.Count > i ? data.Daily.WindSpeedMax[i] : 0,
                        Sunrise = !string.IsNullOrEmpty(sunrise) ? DateTime.Parse(sunrise).ToString("HH:mm") : "",
                        Sunset = !string.IsNullOrEmpty(sunset) ? DateTime.Parse(sunset).ToString("HH:mm") : ""
                    });
                }

                _lastFetch = DateTime.Now;
                _isLoaded = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Weather fetch error: {ex.Message}");
        }

        return _forecast;
    }

    public DailyWeather? GetWeatherForDate(DateTime date)
    {
        return _forecast.FirstOrDefault(w => w.Date.Date == date.Date);
    }

    public static string GetWeatherDescription(int code)
    {
        return code switch
        {
            0 => "Vedro",
            1 or 2 or 3 => "Delimično oblačno",
            45 or 48 => "Magla",
            51 or 53 or 55 => "Rosulja",
            56 or 57 => "Ledena rosulja",
            61 or 63 or 65 => "Kiša",
            66 or 67 => "Ledena kiša",
            71 or 73 or 75 => "Sneg",
            77 => "Snežna zrna",
            80 or 81 or 82 => "Pljuskovi",
            85 or 86 => "Snežni pljuskovi",
            95 => "Grmljavina",
            96 or 99 => "Grmljavina sa gradom",
            _ => "Nepoznato"
        };
    }

    public static string GetWeatherIcon(int code)
    {
        return code switch
        {
            0 => "sunny",
            1 => "partly_cloudy",
            2 or 3 => "cloudy",
            45 or 48 => "foggy",
            51 or 53 or 55 => "drizzle",
            61 or 63 or 65 => "rainy",
            56 or 57 or 66 or 67 => "snowy",
            71 or 73 or 75 => "snowy",
            77 => "snowy",
            80 or 81 or 82 => "stormy",
            85 or 86 => "snowy",
            95 => "stormy",
            96 or 99 => "stormy",
            _ => "unknown"
        };
    }

    public static Color GetWeatherColor(int code)
    {
        return code switch
        {
            0 => Color.FromArgb("#FFD54F"),
            1 => Color.FromArgb("#FFE082"),
            2 or 3 => Color.FromArgb("#90A4AE"),
            45 or 48 => Color.FromArgb("#78909C"),
            51 or 53 or 55 => Color.FromArgb("#42A5F5"),
            61 or 63 or 65 => Color.FromArgb("#2196F3"),
            56 or 57 or 66 or 67 => Color.FromArgb("#90CAF9"),
            71 or 73 or 75 => Color.FromArgb("#B3E5FC"),
            77 => Color.FromArgb("#90CAF9"),
            80 or 81 or 82 => Color.FromArgb("#455A64"),
            85 or 86 => Color.FromArgb("#90CAF9"),
            95 => Color.FromArgb("#FFEB3B"),
            96 or 99 => Color.FromArgb("#455A64"),
            _ => Color.FromArgb("#9E9E9E")
        };
    }
}

public class OpenMeteoResponse
{
    [JsonPropertyName("daily")]
    public DailyData? Daily { get; set; }
}

public class DailyData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = [];

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMax { get; set; } = [];

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMin { get; set; } = [];

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = [];

    [JsonPropertyName("wind_speed_10m_max")]
    public List<double> WindSpeedMax { get; set; } = [];

    [JsonPropertyName("sunrise")]
    public List<string> Sunrise { get; set; } = [];

    [JsonPropertyName("sunset")]
    public List<string> Sunset { get; set; } = [];
}
