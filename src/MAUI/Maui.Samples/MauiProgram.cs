﻿namespace ArcGIS.Samples.Maui;

using CommunityToolkit.Maui;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.Toolkit.Maui;
using Esri.Calcite.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if __ANDROID__
        Esri.ArcGISRuntime.UI.Controls.SceneView.MemoryLimit = 2 * 1073741824L; // 2Gb
#endif
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseArcGISRuntime()
            .UseArcGISToolkit()
            .UseCalcite();

        return builder.Build();
    }
}