using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LetsGoFishing.Models;
using LetsGoFishing.Services;

namespace LetsGoFishing.ViewModels;

/// <summary>
/// Item za prikaz zapisa u dnevniku.
/// </summary>
public partial class DiaryEntryItem : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _dateDisplay = string.Empty;

    [ObservableProperty]
    private string _fishSummary = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private ObservableCollection<CaughtFish> _caughtFish = [];

    // Location
    [ObservableProperty]
    private double? _latitude;

    [ObservableProperty]
    private double? _longitude;

    [ObservableProperty]
    private string? _locationName;

    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;

    public static DiaryEntryItem FromModel(DiaryEntry entry)
    {
        var fishSummary = entry.CaughtFish.Count > 0
            ? $"{entry.CaughtFish.Count} vrsta"
            : "Bez ulova";

        return new DiaryEntryItem
        {
            Id = entry.Id,
            Title = entry.Title,
            DateDisplay = entry.Date.ToString("d. MMMM yyyy."),
            FishSummary = fishSummary,
            Notes = entry.Notes,
            Date = entry.Date,
            CaughtFish = new ObservableCollection<CaughtFish>(entry.CaughtFish),
            Latitude = entry.Latitude,
            Longitude = entry.Longitude,
            LocationName = entry.LocationName
        };
    }
}

/// <summary>
/// ViewModel za stranicu dnevnika.
/// </summary>
public partial class DiaryViewModel : BaseViewModel
{
    private readonly DiaryService _diaryService;
    private readonly FishSpeciesService _fishSpeciesService;

    [ObservableProperty]
    private ObservableCollection<DiaryEntryItem> _entries = [];

    [ObservableProperty]
    private int _selectedYear = DateTime.Today.Year;

    [ObservableProperty]
    private int _fishingDaysCount;

    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private DateTime _editorDate = DateTime.Today;

    [ObservableProperty]
    private string _editorNotes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CaughtFish> _editorCaughtFish = [];

    [ObservableProperty]
    private ObservableCollection<string> _filteredFishTypes = [];

    [ObservableProperty]
    private string? _selectedFishType;

    [ObservableProperty]
    private string _fishQuantity = string.Empty;

    [ObservableProperty]
    private string _fishWeight = string.Empty;

    [ObservableProperty]
    private bool _useQuantity = true;

    // Location
    [ObservableProperty]
    private string _editorLocationName = string.Empty;

    [ObservableProperty]
    private double? _editorLatitude;

    [ObservableProperty]
    private double? _editorLongitude;

    [ObservableProperty]
    private bool _editorHasLocation;

    [ObservableProperty]
    private bool _isGettingLocation;

    private Guid? _editingEntryId;
    private List<string> _allFishTypes = [];

    public DiaryViewModel(DiaryService diaryService, FishSpeciesService fishSpeciesService)
    {
        _diaryService = diaryService;
        _fishSpeciesService = fishSpeciesService;
        Title = "Dnevnik";
    }

    partial void OnEditorTitleChanged(string value)
    {
        SaveEntryCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await _diaryService.LoadEntriesAsync();

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

            RefreshEntries();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ChangeYear(string offset)
    {
        if (int.TryParse(offset, out var offsetValue))
        {
            SelectedYear += offsetValue;
            RefreshEntries();
        }
    }

    private void RefreshEntries()
    {
        var entries = _diaryService.GetEntriesForYear(SelectedYear);

        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(DiaryEntryItem.FromModel(entry));
        }

        FishingDaysCount = _diaryService.GetFishingDaysCount(SelectedYear);
    }

    [RelayCommand]
    private void OpenNewEntry()
    {
        _editingEntryId = null;
        IsEditMode = false;
        EditorTitle = string.Empty;
        EditorDate = DateTime.Today;
        EditorNotes = string.Empty;
        EditorCaughtFish.Clear();
        SelectedFishType = null;
        FishQuantity = string.Empty;
        FishWeight = string.Empty;
        UseQuantity = true;
        EditorLocationName = string.Empty;
        EditorLatitude = null;
        EditorLongitude = null;
        EditorHasLocation = false;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void EditEntry(DiaryEntryItem? entry)
    {
        if (entry == null) return;

        _editingEntryId = entry.Id;
        IsEditMode = true;
        EditorTitle = entry.Title;
        EditorDate = entry.Date;
        EditorNotes = entry.Notes;
        EditorCaughtFish.Clear();
        foreach (var fish in entry.CaughtFish)
        {
            EditorCaughtFish.Add(new CaughtFish
            {
                FishName = fish.FishName,
                Quantity = fish.Quantity,
                WeightKg = fish.WeightKg
            });
        }
        SelectedFishType = null;
        FishQuantity = string.Empty;
        FishWeight = string.Empty;
        UseQuantity = true;
        EditorLatitude = entry.Latitude;
        EditorLongitude = entry.Longitude;
        EditorLocationName = entry.LocationName ?? string.Empty;
        EditorHasLocation = entry.HasLocation;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(DiaryEntryItem? entry)
    {
        if (entry == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Potvrda",
            "Da li želite da obrišete ovaj zapis?",
            "Da", "Ne");

        if (confirm)
        {
            await _diaryService.DeleteEntryAsync(entry.Id);
            RefreshEntries();
        }
    }

    [RelayCommand]
    private void AddFishToList()
    {
        if (string.IsNullOrWhiteSpace(SelectedFishType)) return;

        var caught = new CaughtFish { FishName = SelectedFishType };

        if (UseQuantity && int.TryParse(FishQuantity, out var qty))
        {
            caught.Quantity = qty;
        }
        else if (!UseQuantity && double.TryParse(FishWeight.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var weight))
        {
            caught.WeightKg = weight;
        }

        EditorCaughtFish.Add(caught);

        SelectedFishType = null;
        FishQuantity = string.Empty;
        FishWeight = string.Empty;
    }

    [RelayCommand]
    private void RemoveFishFromList(CaughtFish? fish)
    {
        if (fish != null)
        {
            EditorCaughtFish.Remove(fish);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveEntry))]
    private async Task SaveEntryAsync()
    {
        var entry = new DiaryEntry
        {
            Id = _editingEntryId ?? Guid.NewGuid(),
            Date = EditorDate,
            Title = EditorTitle.Trim(),
            Notes = EditorNotes,
            CaughtFish = EditorCaughtFish.ToList(),
            Latitude = EditorLatitude,
            Longitude = EditorLongitude,
            LocationName = string.IsNullOrWhiteSpace(EditorLocationName) ? null : EditorLocationName
        };

        if (IsEditMode && _editingEntryId.HasValue)
        {
            await _diaryService.UpdateEntryAsync(entry);
        }
        else
        {
            await _diaryService.AddEntryAsync(entry);
        }

        IsEditorOpen = false;
        RefreshEntries();
    }

    private bool CanSaveEntry() => !string.IsNullOrWhiteSpace(EditorTitle);

    [RelayCommand]
    private void CancelEditor()
    {
        IsEditorOpen = false;
    }

    [RelayCommand]
    private async Task GetCurrentLocationAsync()
    {
        if (IsGettingLocation) return;

        try
        {
            IsGettingLocation = true;

            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Lokacija", "Potrebna je dozvola za pristup lokaciji.", "OK");
                    return;
                }
            }

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(15)
            });

            if (location != null)
            {
                EditorLatitude = location.Latitude;
                EditorLongitude = location.Longitude;
                EditorHasLocation = true;
                EditorLocationName = $"{location.Latitude:F4}, {location.Longitude:F4}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            await Shell.Current.DisplayAlert("Greška", "Nije moguće dobiti lokaciju.", "OK");
        }
        finally
        {
            IsGettingLocation = false;
        }
    }

    [RelayCommand]
    private void ClearEditorLocation()
    {
        EditorLatitude = null;
        EditorLongitude = null;
        EditorLocationName = string.Empty;
        EditorHasLocation = false;
    }

    [RelayCommand]
    private async Task OpenLocationInMapsAsync(DiaryEntryItem? entry)
    {
        if (entry == null || !entry.HasLocation) return;

        try
        {
            var location = new Location(entry.Latitude!.Value, entry.Longitude!.Value);
            var options = new MapLaunchOptions
            {
                Name = entry.LocationName ?? "Pecanje"
            };
            await Map.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map error: {ex.Message}");
            await Shell.Current.DisplayAlert("Greška", "Nije moguće otvoriti mapu.", "OK");
        }
    }
}
