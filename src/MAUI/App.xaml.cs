using ArcGISRuntimeMaui;

namespace ArcGISRuntime.Samples.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new CategoryListPage() { });
		Current = this;

    }

	public static App Current { get; private set; }
}
