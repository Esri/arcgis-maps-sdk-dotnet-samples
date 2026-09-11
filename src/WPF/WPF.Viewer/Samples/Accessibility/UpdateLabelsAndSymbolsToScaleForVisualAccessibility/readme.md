# Update labels and symbols to scale for visual accessibility

Scale feature labels and symbols according to the system text-size setting.

![Labels and symbols scaled to 225% system text size](UpdateLabelsAndSymbolsToScaleForVisualAccessibility.jpg)

## Use case

Use this pattern to improve map readability for users who increase text size in their operating system’s accessibility settings. Enable `GeoView.UseSystemTextScale` to scale feature labels automatically. To scale feature symbols, subscribe to system text-size changes and apply the reported scaling factor to the symbol sizes.

## How to use the sample

Change the text size in the operating system's accessibility settings to see the restaurant labels and symbols resize. Use **Open OS text-size settings** on Windows or Android. On iOS, open **Settings** > **Accessibility** > **Display & Text Size** > **Larger Text**.

Clear **Apply OS text size to labels** to stop labels from following the system text size, then select it again to restore system text scaling. Restaurant symbols continue to follow the system text size independently of this setting. The legend and current values show how labels and symbols are scaled.

Select a restaurant to show its name and WGS 84 coordinates in a callout. Select elsewhere to clear the callout.

## How it works

1. Create a `Map` and add a `FeatureLayer`.
2. Create a `SimpleMarkerSymbol` and apply it to the feature layer with a `SimpleRenderer`.
3. Load the feature layer, then add a `LabelDefinition` with a `TextSymbol`.
4. To scale labels, enable `GeoView.UseSystemTextScale`.
5. To scale symbols, read `UISettings.TextScaleFactor` and observe `UISettings.TextScaleFactorChanged`.
6. Use the checkbox to toggle system text scaling for labels without changing symbol scaling.
7. Identify a restaurant with `IdentifyLayerAsync` and show its name and WGS 84 coordinates in a callout.

## Relevant API

* GeoView.UseSystemTextScale

## About the data

This sample uses a [Redlands restaurants](https://www.arcgis.com/home/item.html?id=46119989eccd46a58b8f3d7aedadeb90) feature layer covering food establishments in Redlands, California. Each feature represents a single restaurant.

## Additional information

On WPF, use `GeoView.UseSystemTextScale` to control system text scaling for feature labels. Use `UISettings.TextScaleFactor` and `UISettings.TextScaleFactorChanged` to scale symbols and respond to Windows text-size changes.

On WinUI, the map view uses the inherited `Control.IsTextScaleFactorEnabled` property for labels. On MAUI, the map view uses `GeoView.UseSystemTextScale` for labels. Symbols use `Configuration.FontScale` with `IComponentCallbacks.OnConfigurationChanged` on Android and `UIFontMetrics.GetScaledValue` with `ObserveContentSizeCategoryChanged` on iOS.

## Tags

accessibility, label, readability, scale, symbol, text, visual impairment