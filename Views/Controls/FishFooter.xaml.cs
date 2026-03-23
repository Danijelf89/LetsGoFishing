namespace LetsGoFishing.Views.Controls;

public partial class FishFooter : ContentView
{
    private bool _animationRunning;

    public FishFooter()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _animationRunning = true;
        _ = AnimateFishAsync();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _animationRunning = false;
    }

    private async Task AnimateFishAsync()
    {
        var width = 400.0; // Default width
        try
        {
            width = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        }
        catch { }

        // Set initial positions
        Fish1.TranslationX = -50;
        Fish2.TranslationX = width + 50;
        Fish3.TranslationX = -30;
        Fish4.TranslationX = width + 100;

        while (_animationRunning)
        {
            try
            {
                // Fish 1 - swims right, slow
                _ = Fish1.TranslateToAsync(width + 50, 0, 8000, Easing.Linear)
                    .ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => { if (_animationRunning) Fish1.TranslationX = -50; }));

                // Fish 2 - swims left, medium speed
                _ = Fish2.TranslateToAsync(-50, 0, 6000, Easing.Linear)
                    .ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => { if (_animationRunning) Fish2.TranslationX = width + 50; }));

                // Fish 3 - swims right, fast
                _ = Fish3.TranslateToAsync(width + 30, 0, 5000, Easing.Linear)
                    .ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => { if (_animationRunning) Fish3.TranslationX = -30; }));

                // Fish 4 - big fish, very slow
                _ = Fish4.TranslateToAsync(-100, 0, 12000, Easing.Linear)
                    .ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => { if (_animationRunning) Fish4.TranslationX = width + 100; }));

                await Task.Delay(1000);
            }
            catch
            {
                break;
            }
        }
    }
}
