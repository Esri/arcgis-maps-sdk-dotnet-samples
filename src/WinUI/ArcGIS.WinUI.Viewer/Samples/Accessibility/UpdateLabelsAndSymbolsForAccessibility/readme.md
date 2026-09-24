# Update labels and symbols to scale for visual accessibility

Scale feature labels and symbols according to the system text-size setting.

![Labels and symbols scaled to system text size](UpdateLabelsAndSymbolsForAccessibility.jpg)

## Use case

Use this pattern to improve map readability for users who increase text size in their operating system’s accessibility settings. Enable `Control.IsTextScaleFactorEnabled` to scale feature labels automatically. To scale feature symbols, subscribe to system text-size changes and apply the reported scaling factor to the symbol sizes.

## How to use the sample

Change the text size in Windows accessibility settings to see the restaurant labels and symbols resize. Use **Open OS text-size settings** to open the relevant settings page.

Clear **Apply OS text size to labels** to stop labels from following the system text size, then select it again to restore system text scaling. Restaurant symbols continue to follow the system text size independently of this setting. The legend and current values show how labels and symbols are scaled.

Select a restaurant to show its name and WGS 84 coordinates in a callout. Select elsewhere to clear the callout.

## How it works

1. Create a `Map` and add a `FeatureLayer`.
2. Load the `restaurant` symbol from `Esri2DPointSymbolsStyle` and apply it with a `SimpleRenderer`.
3. Load the feature layer, then add a `LabelDefinition` with a `TextSymbol`.
4. To scale labels, enable `Control.IsTextScaleFactorEnabled`.
5. To scale symbols, read `UISettings.TextScaleFactor` and observe `UISettings.TextScaleFactorChanged`. Scale `MultilayerPointSymbol.Size` by the system text-scale factor.
6. Use the checkbox to toggle system text scaling for labels without changing symbol scaling.
7. Identify a restaurant with `IdentifyLayerAsync` and show its name and WGS 84 coordinates in a callout.

## Relevant API

* Control.IsTextScaleFactorEnabled

## About the data

This sample uses a [Redlands restaurants](https://www.arcgis.com/home/item.html?id=46119989eccd46a58b8f3d7aedadeb90) feature layer covering food establishments in Redlands, California. Each feature represents a single restaurant.

The restaurant symbol comes from [Esri's 2D point symbol web style](https://www.arcgis.com/home/item.html?id=220936cc6ed342c9937abd8f180e7d1e).

## Additional information

On WinUI, use the inherited `Control.IsTextScaleFactorEnabled` property for labels. Use `UISettings.TextScaleFactor` and `UISettings.TextScaleFactorChanged` to scale symbols and respond to Windows text-size changes.

## Tags

accessibility, label, readability, scale, symbol, text, visual impairment