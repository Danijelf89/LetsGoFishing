using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LetsGoFishing.Models;
using LetsGoFishing.Services;

namespace LetsGoFishing.ViewModels;

/// <summary>
/// Item za prikaz vrste ribe u tabeli lovostaja.
/// </summary>
public partial class FishSpeciesItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _latinName = string.Empty;

    [ObservableProperty]
    private string _closedSeasonPeriod = string.Empty;

    [ObservableProperty]
    private string _minLength = string.Empty;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private bool _isInClosedSeason;

    [ObservableProperty]
    private bool _isProtected;

    [ObservableProperty]
    private bool _isInvasive;

    public Color BackgroundColor
    {
        get
        {
            if (IsInClosedSeason) return Color.FromArgb("#33FFC107");
            return Colors.Transparent;
        }
    }
}

/// <summary>
/// ViewModel za stranicu lovostaja.
/// </summary>
public partial class LovostajViewModel : BaseViewModel
{
    private readonly FishSpeciesService _fishSpeciesService;
    private List<FishSpeciesItem> _allSpecies = [];

    [ObservableProperty]
    private ObservableCollection<FishSpeciesItem> _filteredSpecies = [];

    [ObservableProperty]
    private ObservableCollection<FishSpeciesItem> _protectedSpecies = [];

    [ObservableProperty]
    private ObservableCollection<FishSpeciesItem> _invasiveSpecies = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyClosedSeason;

    public LovostajViewModel(FishSpeciesService fishSpeciesService)
    {
        _fishSpeciesService = fishSpeciesService;
        Title = "Lovostaj";
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSpecies();
    }

    partial void OnShowOnlyClosedSeasonChanged(bool value)
    {
        FilterSpecies();
    }

    [RelayCommand]
    private void LoadData()
    {
        var today = DateTime.Today;

        // Dobij sve lovljive vrste (ne zaštićene i ne invazivne)
        // Sortiraj tako da ribe u lovostaju budu na vrhu liste
        _allSpecies = _fishSpeciesService.GetAllSpecies()
            .Where(s => !s.IsProtected && !s.IsInvasive)
            .Select(s => new FishSpeciesItem
            {
                Name = s.Name,
                LatinName = s.LatinName,
                ClosedSeasonPeriod = s.GetClosedSeasonPeriod(),
                MinLength = s.MinLengthCm > 0 ? $"{s.MinLengthCm} cm" : "-",
                Note = s.Note ?? string.Empty,
                IsInClosedSeason = s.IsInClosedSeason(today)
            })
            .OrderByDescending(s => s.IsInClosedSeason)
            .ThenBy(s => s.Name)
            .ToList();

        // Zaštićene vrste
        var protectedList = _fishSpeciesService.GetProtectedSpecies()
            .Select(s => new FishSpeciesItem
            {
                Name = s.Name,
                LatinName = s.LatinName,
                IsProtected = true
            })
            .OrderBy(s => s.Name)
            .ToList();

        ProtectedSpecies.Clear();
        foreach (var item in protectedList)
        {
            ProtectedSpecies.Add(item);
        }

        // Invazivne vrste
        var invasiveList = _fishSpeciesService.GetInvasiveSpecies()
            .Select(s => new FishSpeciesItem
            {
                Name = s.Name,
                LatinName = s.LatinName,
                Note = s.Note ?? string.Empty,
                IsInvasive = true
            })
            .OrderBy(s => s.Name)
            .ToList();

        InvasiveSpecies.Clear();
        foreach (var item in invasiveList)
        {
            InvasiveSpecies.Add(item);
        }

        FilterSpecies();
    }

    private void FilterSpecies()
    {
        var filtered = _allSpecies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(s =>
                s.Name.ToLowerInvariant().Contains(search) ||
                s.LatinName.ToLowerInvariant().Contains(search));
        }

        if (ShowOnlyClosedSeason)
        {
            filtered = filtered.Where(s => s.IsInClosedSeason);
        }

        // Sort by IsInClosedSeason first (closed season at top), then by name
        filtered = filtered
            .OrderByDescending(s => s.IsInClosedSeason)
            .ThenBy(s => s.Name);

        FilteredSpecies.Clear();
        foreach (var item in filtered)
        {
            FilteredSpecies.Add(item);
        }
    }
}
