namespace ArcGIS.Samples.Maui;

using Esri.ArcGISRuntime.Maui;
using CommunityToolkit.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("calcite-ui-icons-24.ttf", "calcite-ui-icons-24");
            }).UseArcGISRuntime();

        return builder.Build();
    }
}