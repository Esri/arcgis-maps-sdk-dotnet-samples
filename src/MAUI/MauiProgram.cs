namespace ArcGISRuntime.Samples.Maui;
using Esri.ArcGISRuntime.Maui;
using Microsoft.Maui.Controls.Compatibility.Hosting;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).UseArcGISRuntime().UseMauiCompatibility();

        return builder.Build();
	}
}
