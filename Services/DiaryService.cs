using System.Text.Json;
using LetsGoFishing.Models;

namespace LetsGoFishing.Services;

/// <summary>
/// Servis za upravljanje dnevnikom pecanja sa lokalnim skladištenjem.
/// Koristi Preferences API za pouzdano čuvanje podataka koje preživljava update aplikacije.
/// </summary>
public class DiaryService
{
    private const string DiaryPreferencesKey = "diary_entries_data";
    private readonly JsonSerializerOptions _jsonOptions;
    private List<DiaryEntry> _entries = [];

    public DiaryService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public Task LoadEntriesAsync()
    {
        try
        {
            var json = Preferences.Get(DiaryPreferencesKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine($"Diary JSON loaded from Preferences: {json.Length} chars");
                _entries = JsonSerializer.Deserialize<List<DiaryEntry>>(json, _jsonOptions) ?? [];
                System.Diagnostics.Debug.WriteLine($"Diary entries loaded: {_entries.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No diary data in Preferences, starting fresh");
                _entries = [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Diary load error: {ex.Message}\n{ex.StackTrace}");
            _entries = [];
        }

        return Task.CompletedTask;
    }

    private Task SaveEntriesAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_entries, _jsonOptions);
            Preferences.Set(DiaryPreferencesKey, json);
            System.Diagnostics.Debug.WriteLine($"Diary saved to Preferences: {_entries.Count} entries, {json.Length} chars");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Diary save error: {ex.Message}\n{ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<DiaryEntry> GetAllEntries()
    {
        return _entries.OrderByDescending(e => e.Date).ToList().AsReadOnly();
    }

    public DiaryEntry? GetEntryForDate(DateTime date)
    {
        return _entries.FirstOrDefault(e => e.Date.Date == date.Date);
    }

    public IReadOnlyList<DiaryEntry> GetEntriesForYear(int year)
    {
        return _entries
            .Where(e => e.Date.Year == year)
            .OrderByDescending(e => e.Date)
            .ToList()
            .AsReadOnly();
    }

    public async Task AddEntryAsync(DiaryEntry entry)
    {
        entry.CreatedAt = DateTime.Now;
        _entries.Add(entry);
        await SaveEntriesAsync();
    }

    public async Task UpdateEntryAsync(DiaryEntry entry)
    {
        var index = _entries.FindIndex(e => e.Id == entry.Id);
        if (index >= 0)
        {
            entry.ModifiedAt = DateTime.Now;
            _entries[index] = entry;
            await SaveEntriesAsync();
        }
    }

    public async Task DeleteEntryAsync(Guid entryId)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
        {
            _entries.Remove(entry);
            await SaveEntriesAsync();
        }
    }

    public int GetFishingDaysCount(int year)
    {
        return _entries
            .Where(e => e.Date.Year == year)
            .Select(e => e.Date.Date)
            .Distinct()
            .Count();
    }

    public bool HasEntryForDate(DateTime date)
    {
        return _entries.Any(e => e.Date.Date == date.Date);
    }
}
