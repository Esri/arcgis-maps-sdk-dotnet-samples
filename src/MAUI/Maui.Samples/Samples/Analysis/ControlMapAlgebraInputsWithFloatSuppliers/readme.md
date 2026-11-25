# Control map algebra inputs with float suppliers

Use float suppliers to dynamically update numeric inputs to a map algebra expression and automatically refresh analysis results as input values change.

![Control map algebra inputs with float suppliers sample](controlmapalgebrainputswithfloatsuppliers.jpg)

## Use case

Many spatial analysis workflows rely on numeric parameters that control how data is filtered, classified, or evaluated. As users adjust these parameters, analysis results often need to be updated repeatedly to explore different scenarios. Float suppliers allow numeric inputs to be changed dynamically, enabling map algebra expressions to refresh automatically without rebuilding the analysis each time a value changes.

## How to use the sample

When the sample opens, the map shows the result of a slope analysis which displays slopes between 0 to 10 degrees. This range shows gentle hills and flatter areas such as roads and man-made structures in dark purple. Drag the range slider handles in the control panel to adjust the minimum and maximum slope values between 0 and 85 degrees (the maximum slope value of the data). The analysis updates automatically to show only slopes within the selected range. An indicator in the control panel shows when the analysis is updating. The slope results are drawn on top of an elevation field which is rendered with a surface color ramp to provide context for the data. Green areas represent lower lying terrain, and brown to white reflects higher elevations.

## How it works

1. Create a `ContinuousField` from an elevation raster file using `ContinuousField.CreateAsync`.
2. Create a `ContinuousFieldFunction` from the continuous field using `ContinuousFieldSupplier`.
3. Derive a `Slope` function from the continuous field function.
4. Create two `FloatSupplier` instances and set their `Value` properties to the initial minimum and maximum slope values.
5. Create a `BooleanFieldFunction` for the slope range using the map algebra operators `>=`, `<=`, and `&`, with the float suppliers as inputs.
6. Apply the boolean selection to the slope function using `Mask` to retain only slopes within the selected range.
7. Create a `FieldAnalysis` for the masked slope function using the `FieldAnalysis` constructor and a `StretchRenderer`.
8. Add the field analysis to an `AnalysisOverlay` and add the overlay to the map view.
9. Update the float suppliers' `Value` properties to update the analysis result dynamically.

## Relevant API

* AnalysisOverlay
* BooleanFieldFunction
* ColorRamp
* ContinuousField
* ContinuousFieldFunction
* ContinuousFieldSupplier
* FieldAnalysis
* FloatSupplier
* StretchRenderer

## About the data

The sample uses a [1m resolution digital terrain elevation raster of the area over Méribel](https://www.arcgis.com/home/item.html?id=e8fe90cd623b48dfa683f448bef65e70), in the French Alps. The spatial reference of the data is UTM32N.

## Tags

elevation, map algebra, raster, slope, spatial analysis, terrain
