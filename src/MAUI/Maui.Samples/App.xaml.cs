using ArcGISRuntimeMaui;

namespace ArcGISMapsSDKMaui.Samples.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new CategoryListPage() { });
        Current = this;
    }
}