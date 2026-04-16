namespace LetsGoFishing.Views;

[QueryProperty(nameof(ImagePath), "ImagePath")]
public partial class ImagePreviewPage : ContentPage
{
    public string ImagePath
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                PreviewImage.Source = ImageSource.FromFile(value);
            }
        }
    }

    public ImagePreviewPage()
    {
        InitializeComponent();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
