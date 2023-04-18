using ArcGIS;

namespace ArcGIS.Samples.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        Current = this;
    }
}