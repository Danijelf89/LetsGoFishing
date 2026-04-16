using LetsGoFishing.ViewModels;

namespace LetsGoFishing.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;

    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCalendarCommand.ExecuteAsync(null);
    }

    private void OnKomLabelTapped(object sender, TappedEventArgs e)
    {
        _viewModel.UseQuantity = true;
    }

    private void OnKgLabelTapped(object sender, TappedEventArgs e)
    {
        _viewModel.UseQuantity = false;
    }
}
