namespace LetsGoFishing;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Set dark theme by default
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
