using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LetsGoFishing.Models;
using LetsGoFishing.Services;

namespace LetsGoFishing.ViewModels;

/// <summary>
/// Item za prikaz ribe u danu kalendara.
/// </summary>
public partial class FishDayItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private FishingStatus _status;

    public bool IsAllowed => Status == FishingStatus.Allowed;
    public bool IsClosedSeason => Status == FishingStatus.ClosedSeason;
    public bool IsProtected => Status == FishingStatus.Protected;

    public Color StatusColor => Status switch
    {
        FishingStatus.Allowed => Color.FromArgb("#4CAF50"),
        FishingStatus.ClosedSeason => Color.FromArgb("#FFC107"),
        FishingStatus.Protected => Color.FromArgb("#F44336"),
        _ => Colors.Gray
    };

    public string StatusText => Status switch
    {
        FishingStatus.Allowed => "OK",
        FishingStatus.ClosedSeason => "Lovostaj",
        FishingStatus.Protected => "Zaštićena",
        _ => ""
    };
}

/// <summary>
/// ViewModel za jedan dan u kalendaru.
/// </summary>
public partial class CalendarDayViewModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private int _dayNumber;

    [ObservableProperty]
    private bool _isCurrentMonth;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _hasDiaryEntry;

    [ObservableProperty]
    private ObservableCollection<FishDayItem> _fishItems = [];

    [ObservableProperty]
    private string _weatherEmoji = string.Empty;

    [ObservableProperty]
    private string _weatherDescription = string.Empty;

    [ObservableProperty]
    private double _temperatureMax;

    [ObservableProperty]
    private double _temperatureMin;

    [ObservableProperty]
    private double _windSpeed;

    [ObservableProperty]
    private string _sunrise = string.Empty;

    [ObservableProperty]
    private string _sunset = string.Empty;

    [ObservableProperty]
    private bool _hasWeather;

    public bool HasFish => FishItems.Count > 0;
    public int AllowedCount => FishItems.Count(f => f.IsAllowed);
    public int ClosedSeasonCount => FishItems.Count(f => f.IsClosedSeason);

    public Color DayBackgroundColor
    {
        get
        {
            if (IsToday) return Color.FromArgb("#1B5E20");
            if (!IsCurrentMonth) return Color.FromArgb("#1A1A1A");
            return Color.FromArgb("#2D2D2D");
        }
    }

    public Color DayTextColor
    {
        get
        {
            if (!IsCurrentMonth) return Color.FromArgb("#666666");
            return Colors.White;
        }
    }

    public CalendarDayViewModel() { }

    public CalendarDayViewModel(DateTime date, List<FishDayItem> fishItems, DateTime currentMonth, bool hasDiaryEntry)
    {
        Date = date;
        DayNumber = date.Day;
        IsCurrentMonth = date.Month == currentMonth.Month && date.Year == currentMonth.Year;
        IsToday = date.Date == DateTime.Today;
        HasDiaryEntry = hasDiaryEntry;
        FishItems = new ObservableCollection<FishDayItem>(fishItems);
    }

    public void SetWeather(string icon, string description, double tempMax, double tempMin, double windSpeed, string sunrise, string sunset)
    {
        WeatherEmoji = GetWeatherEmoji(icon);
        WeatherDescription = description;
        TemperatureMax = tempMax;
        TemperatureMin = tempMin;
        WindSpeed = windSpeed;
        Sunrise = sunrise;
        Sunset = sunset;
        HasWeather = true;
    }

    private static string GetWeatherEmoji(string icon)
    {
        return icon switch
        {
            "sunny" => "☀️",
            "partly_cloudy" => "⛅",
            "cloudy" => "☁️",
            "foggy" => "🌫️",
            "drizzle" => "🌧️",
            "rainy" => "🌧️",
            "snowy" => "❄️",
            "stormy" => "⛈️",
            _ => "🌤️"
        };
    }
}

/// <summary>
/// Predstavlja ulovljenu ribu u editoru.
/// </summary>
public partial class CaughtFishEditorItem : ObservableObject
{
    [ObservableProperty]
    private string _fishName = string.Empty;

    [ObservableProperty]
    private int? _quantity;

    [ObservableProperty]
    private double? _weightKg;

    public string DisplayQuantity => Quantity.HasValue
        ? $"{Quantity} kom"
        : WeightKg.HasValue
            ? $"{WeightKg:F2} kg"
            : "-";

    public CaughtFishEditorItem() { }

    public CaughtFishEditorItem(CaughtFish model)
    {
        FishName = model.FishName;
        Quantity = model.Quantity;
        WeightKg = model.WeightKg;
    }

    public CaughtFish ToModel() => new()
    {
        FishName = FishName,
        Quantity = Quantity,
        WeightKg = WeightKg
    };
}

/// <summary>
/// Glavni ViewModel za kalendar stranicu.
/// </summary>
public partial class CalendarViewModel : BaseViewModel
{
    private readonly FishSpeciesService _fishSpeciesService;
    private readonly SettingsService _settingsService;
    private readonly DiaryService _diaryService;
    private readonly WeatherService _weatherService;

    private static readonly CultureInfo SerbianCulture = new("sr-Latn-RS");
    private List<string> _allFishTypes = [];

    [ObservableProperty]
    private DateTime _currentMonth = DateTime.Today;

    [ObservableProperty]
    private string _monthYearDisplay = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CalendarDayViewModel> _days = [];

    [ObservableProperty]
    private CalendarDayViewModel? _selectedDay;

    [ObservableProperty]
    private bool _isDayPopupVisible;

    [ObservableProperty]
    private ObservableCollection<FishDayItem> _filteredDialogFish = [];

    [ObservableProperty]
    private int _dialogFilterIndex = 0;

    // Context menu
    [ObservableProperty]
    private bool _isContextMenuVisible;

    [ObservableProperty]
    private CalendarDayViewModel? _contextMenuDay;

    // Weather popup
    [ObservableProperty]
    private bool _isWeatherPopupVisible;

    [ObservableProperty]
    private CalendarDayViewModel? _weatherDay;

    // Diary editor
    [ObservableProperty]
    private bool _isDiaryEditorOpen;

    [ObservableProperty]
    private DateTime _diaryDate = DateTime.Today;

    [ObservableProperty]
    private string _diaryTitle = string.Empty;

    [ObservableProperty]
    private string _diaryNotes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CaughtFishEditorItem> _diaryCaughtFish = [];

    [ObservableProperty]
    private string? _selectedFishType;

    [ObservableProperty]
    private string _fishQuantity = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _filteredFishTypes = [];

    // City display
    [ObservableProperty]
    private string _selectedCityName = string.Empty;

    [ObservableProperty]
    private bool _hasSelectedCity;

    public CalendarViewModel(
        FishSpeciesService fishSpeciesService,
        SettingsService settingsService,
        DiaryService diaryService,
        WeatherService weatherService)
    {
        _fishSpeciesService = fishSpeciesService;
        _settingsService = settingsService;
        _diaryService = diaryService;
        _weatherService = weatherService;

        Title = "Kalendar";
        UpdateMonthDisplay();
    }

    partial void OnDiaryTitleChanged(string value)
    {
        SaveDiaryEntryCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentMonthChanged(DateTime value)
    {
        UpdateMonthDisplay();
    }

    partial void OnDialogFilterIndexChanged(int value)
    {
        UpdateFilteredDialogFish();
    }

    partial void OnSelectedDayChanged(CalendarDayViewModel? value)
    {
        DialogFilterIndex = 0;
        UpdateFilteredDialogFish();
    }

    private void UpdateFilteredDialogFish()
    {
        FilteredDialogFish.Clear();

        if (SelectedDay == null) return;

        var items = DialogFilterIndex switch
        {
            1 => SelectedDay.FishItems.Where(f => f.Status == FishingStatus.Allowed),
            2 => SelectedDay.FishItems.Where(f => f.Status == FishingStatus.ClosedSeason),
            _ => SelectedDay.FishItems
        };

        foreach (var item in items)
        {
            FilteredDialogFish.Add(item);
        }
    }

    private void UpdateMonthDisplay()
    {
        var monthName = CurrentMonth.ToString("MMMM yyyy", SerbianCulture);
        MonthYearDisplay = char.ToUpper(monthName[0]) + monthName[1..];
    }

    [RelayCommand]
    private async Task LoadCalendarAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await _settingsService.LoadSettingsAsync();
            await _diaryService.LoadEntriesAsync();

            var settings = _settingsService.Settings;
            HasSelectedCity = settings.HasSelectedCity;
            SelectedCityName = settings.SelectedCity;

            if (settings.HasSelectedCity)
            {
                var savedCity = _weatherService.GetAvailableCities()
                    .FirstOrDefault(c => c.Name == settings.SelectedCity);
                if (savedCity != null)
                {
                    // Load weather only if not already loaded or city changed
                    await _weatherService.SetCityAsync(savedCity);
                }
            }

            // Load fish types for diary
            _allFishTypes = _fishSpeciesService.GetAllSpecies()
                .Where(s => !s.IsProtected)
                .Select(s => s.Name)
                .OrderBy(n => n)
                .ToList();

            FilteredFishTypes.Clear();
            foreach (var fish in _allFishTypes)
            {
                FilteredFishTypes.Add(fish);
            }

            await BuildCalendarDaysAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load calendar error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateMonthAsync(string offset)
    {
        if (int.TryParse(offset, out var offsetValue))
        {
            CurrentMonth = CurrentMonth.AddMonths(offsetValue);
            await BuildCalendarDaysAsync();
        }
    }

    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        CurrentMonth = DateTime.Today;
        await BuildCalendarDaysAsync();
    }

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        if (!HasSelectedCity) return;

        var savedCity = _weatherService.GetAvailableCities()
            .FirstOrDefault(c => c.Name == SelectedCityName);
        if (savedCity != null)
        {
            await _weatherService.SetCityAsync(savedCity, forceRefresh: true);
            await BuildCalendarDaysAsync();
        }
    }

    [RelayCommand]
    private void SelectDay(CalendarDayViewModel? day)
    {
        if (day == null || !day.IsCurrentMonth) return;

        // Show context menu
        ContextMenuDay = day;
        IsContextMenuVisible = true;
    }

    [RelayCommand]
    private void CloseContextMenu()
    {
        IsContextMenuVisible = false;
        ContextMenuDay = null;
    }

    [RelayCommand]
    private void ShowFishForDay()
    {
        if (ContextMenuDay == null || !ContextMenuDay.HasFish) return;

        SelectedDay = ContextMenuDay;
        IsContextMenuVisible = false;
        IsDayPopupVisible = true;
    }

    [RelayCommand]
    private void CloseDayPopup()
    {
        IsDayPopupVisible = false;
        SelectedDay = null;
    }

    [RelayCommand]
    private void SetDialogFilter(string filterIndex)
    {
        if (int.TryParse(filterIndex, out var index))
        {
            DialogFilterIndex = index;
        }
    }

    [RelayCommand]
    private void ShowWeatherDetails(CalendarDayViewModel? day)
    {
        if (day == null || !day.HasWeather) return;

        WeatherDay = day;
        IsWeatherPopupVisible = true;
    }

    [RelayCommand]
    private void CloseWeatherPopup()
    {
        IsWeatherPopupVisible = false;
        WeatherDay = null;
    }

    [RelayCommand]
    private void ShowWeatherFromMenu()
    {
        if (ContextMenuDay == null || !ContextMenuDay.HasWeather) return;

        IsContextMenuVisible = false;
        WeatherDay = ContextMenuDay;
        IsWeatherPopupVisible = true;
    }

    // Diary editor commands
    [RelayCommand]
    private void OpenDiaryEditor(CalendarDayViewModel? day)
    {
        var targetDay = day ?? ContextMenuDay;
        if (targetDay == null) return;

        IsContextMenuVisible = false;
        DiaryDate = targetDay.Date;
        DiaryTitle = string.Empty;
        DiaryNotes = string.Empty;
        DiaryCaughtFish.Clear();
        SelectedFishType = null;
        FishQuantity = string.Empty;
        IsDiaryEditorOpen = true;
    }

    [RelayCommand]
    private void AddFishToDiary()
    {
        if (string.IsNullOrWhiteSpace(SelectedFishType)) return;

        var caught = new CaughtFish { FishName = SelectedFishType };

        if (int.TryParse(FishQuantity, out var qty))
        {
            caught.Quantity = qty;
        }

        DiaryCaughtFish.Add(new CaughtFishEditorItem(caught));

        SelectedFishType = null;
        FishQuantity = string.Empty;
    }

    [RelayCommand]
    private void RemoveFishFromDiary(CaughtFishEditorItem? fish)
    {
        if (fish != null)
        {
            DiaryCaughtFish.Remove(fish);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveDiaryEntry))]
    private async Task SaveDiaryEntryAsync()
    {
        var entry = new DiaryEntry
        {
            Id = Guid.NewGuid(),
            Date = DiaryDate,
            Title = DiaryTitle.Trim(),
            Notes = DiaryNotes,
            CaughtFish = DiaryCaughtFish.Select(f => f.ToModel()).ToList()
        };

        await _diaryService.AddEntryAsync(entry);

        IsDiaryEditorOpen = false;
        await BuildCalendarDaysAsync();
    }

    private bool CanSaveDiaryEntry() => !string.IsNullOrWhiteSpace(DiaryTitle);

    [RelayCommand]
    private void CancelDiaryEditor()
    {
        IsDiaryEditorOpen = false;
    }

    private async Task BuildCalendarDaysAsync()
    {
        var firstOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var startDayOfWeek = (int)firstOfMonth.DayOfWeek;
        var startDate = firstOfMonth.AddDays(-startDayOfWeek);
        var currentMonth = CurrentMonth;
        var hasSelectedCity = _settingsService.Settings.HasSelectedCity;

        var calendarDays = await Task.Run(() =>
        {
            var days = new List<CalendarDayViewModel>();

            var allSpecies = _fishSpeciesService.GetAllSpecies()
                .Where(s => !s.IsProtected)
                .ToList();

            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);

                var fishItems = allSpecies.Select(species => new FishDayItem
                {
                    Name = species.Name,
                    Status = _fishSpeciesService.GetFishingStatus(species.Name, date)
                }).ToList();

                var hasDiaryEntry = _diaryService.HasEntryForDate(date);

                var dayVm = new CalendarDayViewModel(date, fishItems, currentMonth, hasDiaryEntry);

                if (hasSelectedCity)
                {
                    var weather = _weatherService.GetWeatherForDate(date);
                    if (weather != null)
                    {
                        dayVm.SetWeather(
                            weather.Icon,
                            weather.Description,
                            weather.TemperatureMax,
                            weather.TemperatureMin,
                            weather.WindSpeedMax,
                            weather.Sunrise,
                            weather.Sunset);
                    }
                }

                days.Add(dayVm);
            }

            return days;
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Days.Clear();
            foreach (var day in calendarDays)
            {
                Days.Add(day);
            }
        });
    }
}
