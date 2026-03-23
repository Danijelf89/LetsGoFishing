using LetsGoFishing.ViewModels;

namespace LetsGoFishing.Views;

public partial class LovostajPage : ContentPage
{
    private readonly LovostajViewModel _viewModel;

    public LovostajPage(LovostajViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadDataCommand.Execute(null);
    }
}
