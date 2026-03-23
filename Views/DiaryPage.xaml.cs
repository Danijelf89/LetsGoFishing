using LetsGoFishing.ViewModels;

namespace LetsGoFishing.Views;

public partial class DiaryPage : ContentPage
{
    private readonly DiaryViewModel _viewModel;

    public DiaryPage(DiaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
