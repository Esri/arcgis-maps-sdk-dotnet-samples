using Microsoft.UI.Xaml;

namespace ArcGISRuntime.WinUI.Viewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            frame.Content = new MainPage();
            Title = "ArcGIS Runtime SDK for .NET (WinUI Desktop)";
        }
    }
}
