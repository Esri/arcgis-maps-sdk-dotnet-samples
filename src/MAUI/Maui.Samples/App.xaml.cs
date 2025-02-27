using ArcGIS;

namespace ArcGIS.Samples.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Current = this;
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new AppShell());
    }
}