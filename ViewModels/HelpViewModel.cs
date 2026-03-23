using CommunityToolkit.Mvvm.ComponentModel;

namespace LetsGoFishing.ViewModels;

/// <summary>
/// ViewModel za stranicu sa uputstvom.
/// </summary>
public partial class HelpViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _dataLocation = string.Empty;

    public HelpViewModel()
    {
        Title = "Uputstvo";
        DataLocation = FileSystem.AppDataDirectory;
    }
}
