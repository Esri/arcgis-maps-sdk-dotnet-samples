# Class break renderer

Render features in a layer using a distinct symbol for each class break defined in the renderer.

![Image of unique value renderer](RenderClassBreak.jpg)

## Use case

A class break renderer allows you to symbolize features in a layer based on one or more defined ranges. This is typically done by using symbols for each class break, fill styles, or images to represent features that fall within a range. A class break renderer could be used to symbolize states with population in certain range with same symbol.

## How to use the sample

The map with the symbolized feature layer will be shown automatically when the sample loads.

## How it works

Using the `ClassBreakRenderer`, separate symbols can be used to display features that fall within a specific range of value for a given field. In this case, the field is Pop2000 of the USA. While multiple fields can be used, this sample only uses one.

1. A `SimpleFillSymbol` is defined for each type of feature.
2. `SimpleFillSymbol` can be applied to polygon features, which is the type of feature contained by this `ServiceFeatureTable`.
3. Separate `ClassBreak` objects are created which define the ranges in the renderer field and the symbol used to render matching features.
4. A default symbol is created to render all features that do not match any of the `classbreaks` objects defined.

## Relevant API

* FeatureLayer
* ServiceFeatureTable
* SimpleFillSymbol
* SimpleLineSymbol
* ClassBreak
* ClassBreakRenderer

## About the data

The map shows U.S. states symbolized by pop2000. Symbols are defined for population ranges *500000 - 2000000*, *2000000 - 6000000*, and *6000000 - 30000000* states. All other features are symbolized with the default symbol.

## Tags

draw, renderer, symbol, symbology, classes, class breaks 