using System.Text.Json.Serialization;

namespace LetsGoFishing.Models;

/// <summary>
/// Predstavlja dan sa ribama koje se preporučuju za lov.
/// </summary>
public class FishDay
{
    public DateTime Date { get; set; }
    public List<string> Fish { get; set; } = [];
}

/// <summary>
/// Kontejner za podatke o ribolovu učitane iz JSON-a.
/// </summary>
public class FishingData
{
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, List<string>> Data { get; set; } = [];
}

/// <summary>
/// Status ribolova za određenu vrstu na određeni dan.
/// </summary>
public enum FishingStatus
{
    Allowed,      // Zeleno - dozvoljeno
    ClosedSeason, // Žuto - lovostaj
    Protected     // Crveno - zaštićena vrsta
}

/// <summary>
/// Informacije o vrsti ribe uključujući lovostaj i lovnu dužinu.
/// </summary>
public class FishSpecies
{
    public string Name { get; set; } = string.Empty;
    public string LatinName { get; set; } = string.Empty;
    public int? ClosedSeasonStartDay { get; set; }
    public int? ClosedSeasonStartMonth { get; set; }
    public int? ClosedSeasonEndDay { get; set; }
    public int? ClosedSeasonEndMonth { get; set; }
    public int MinLengthCm { get; set; }
    public string? Note { get; set; }
    public bool IsProtected { get; set; }
    public bool IsInvasive { get; set; }

    /// <summary>
    /// Proverava da li je riba u lovostaju na određeni datum.
    /// </summary>
    public bool IsInClosedSeason(DateTime date)
    {
        if (IsProtected) return true;
        if (IsInvasive) return false;

        if (!ClosedSeasonStartMonth.HasValue || !ClosedSeasonEndMonth.HasValue)
            return false;

        var startMonth = ClosedSeasonStartMonth.Value;
        var startDay = ClosedSeasonStartDay ?? 1;
        var endMonth = ClosedSeasonEndMonth.Value;
        var endDay = ClosedSeasonEndDay ?? DateTime.DaysInMonth(date.Year, endMonth);

        var year = date.Year;

        if (startMonth > endMonth)
        {
            var startDate1 = new DateTime(year, startMonth, startDay);
            var endDate1 = new DateTime(year, 12, 31);
            var startDate2 = new DateTime(year, 1, 1);
            var endDate2 = new DateTime(year, endMonth, endDay);

            return (date >= startDate1 && date <= endDate1) || (date >= startDate2 && date <= endDate2);
        }
        else
        {
            var startDate = new DateTime(year, startMonth, startDay);
            var endDate = new DateTime(year, endMonth, endDay);
            return date >= startDate && date <= endDate;
        }
    }

    /// <summary>
    /// Da li je dozvoljen ribolov ove vrste na određeni datum.
    /// </summary>
    public bool CanFish(DateTime date)
    {
        if (IsProtected) return false;
        if (IsInvasive) return true;
        return !IsInClosedSeason(date);
    }

    /// <summary>
    /// Vraća formatiran period lovostaja.
    /// </summary>
    public string GetClosedSeasonPeriod()
    {
        if (IsProtected) return "Trajno zaštićena";
        if (IsInvasive) return "Bez ograničenja";
        if (!ClosedSeasonStartMonth.HasValue) return "-";

        var startDay = ClosedSeasonStartDay ?? 1;
        var endDay = ClosedSeasonEndDay ?? 28;

        var months = new[] { "", "januar", "februar", "mart", "april", "maj", "jun",
                             "jul", "avgust", "septembar", "oktobar", "novembar", "decembar" };

        return $"{startDay}. {months[ClosedSeasonStartMonth.Value]} - {endDay}. {months[ClosedSeasonEndMonth!.Value]}";
    }
}

/// <summary>
/// Predstavlja jedan zapis u dnevniku pecanja.
/// </summary>
public class DiaryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<CaughtFish> CaughtFish { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Predstavlja ulovljenu ribu u dnevniku.
/// </summary>
public class CaughtFish
{
    public string FishName { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public double? WeightKg { get; set; }

    public string DisplayQuantity => Quantity.HasValue
        ? $"{Quantity} kom"
        : WeightKg.HasValue
            ? $"{WeightKg:F2} kg"
            : "-";
}

/// <summary>
/// Korisnička podešavanja koja se čuvaju lokalno.
/// </summary>
public class UserSettings
{
    public List<string> SelectedFish { get; set; } = [];
    public bool NotificationsEnabled { get; set; } = true;

    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan NotificationTime { get; set; } = new TimeSpan(18, 0, 0);

    public bool IsDarkTheme { get; set; } = true;
    public string SelectedCity { get; set; } = "Novi Sad";
    public int WeatherRefreshHours { get; set; } = 6;
    public bool HasSelectedCity { get; set; } = false;
}

/// <summary>
/// Predstavlja grad sa koordinatama za vremensku prognozu.
/// </summary>
public class CityInfo
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public CityInfo() { }

    public CityInfo(string name, double latitude, double longitude)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString() => Name;
}

/// <summary>
/// JSON converter za TimeSpan serijalizaciju.
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeSpan.TryParse(value, out var result) ? result : new TimeSpan(18, 0, 0);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, TimeSpan value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}

/// <summary>
/// Dnevna vremenska prognoza.
/// </summary>
public class DailyWeather
{
    public DateTime Date { get; set; }
    public double TemperatureMax { get; set; }
    public double TemperatureMin { get; set; }
    public int WeatherCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double WindSpeedMax { get; set; }
    public string Sunrise { get; set; } = string.Empty;
    public string Sunset { get; set; } = string.Empty;
}
