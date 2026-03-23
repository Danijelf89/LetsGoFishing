using System.Text.Json;
using LetsGoFishing.Models;

namespace LetsGoFishing.Services;

/// <summary>
/// Servis za upravljanje dnevnikom pecanja sa lokalnim skladištenjem.
/// </summary>
public class DiaryService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _diaryFilePath;
    private List<DiaryEntry> _entries = [];

    public DiaryService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Use AppDataDirectory which persists across app restarts
        var appDataDir = FileSystem.AppDataDirectory;
        _diaryFilePath = Path.Combine(appDataDir, "diary.json");

        System.Diagnostics.Debug.WriteLine($"Diary file path: {_diaryFilePath}");
    }

    public async Task LoadEntriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading diary from: {_diaryFilePath}");

            if (File.Exists(_diaryFilePath))
            {
                var json = await File.ReadAllTextAsync(_diaryFilePath);
                System.Diagnostics.Debug.WriteLine($"Diary JSON loaded: {json.Length} chars");
                _entries = JsonSerializer.Deserialize<List<DiaryEntry>>(json, _jsonOptions) ?? [];
                System.Diagnostics.Debug.WriteLine($"Diary entries loaded: {_entries.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Diary file does not exist, starting fresh");
                _entries = [];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Diary load error: {ex.Message}\n{ex.StackTrace}");
            _entries = [];
        }
    }

    private async Task SaveEntriesAsync()
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_diaryFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Diagnostics.Debug.WriteLine($"Created directory: {directory}");
            }

            var json = JsonSerializer.Serialize(_entries, _jsonOptions);
            await File.WriteAllTextAsync(_diaryFilePath, json);
            System.Diagnostics.Debug.WriteLine($"Diary saved: {_entries.Count} entries, {json.Length} chars");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Diary save error: {ex.Message}\n{ex.StackTrace}");
        }
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
