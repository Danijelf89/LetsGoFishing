using LetsGoFishing.Views;

namespace LetsGoFishing
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("ImagePreviewPage", typeof(ImagePreviewPage));
        }
    }
}
