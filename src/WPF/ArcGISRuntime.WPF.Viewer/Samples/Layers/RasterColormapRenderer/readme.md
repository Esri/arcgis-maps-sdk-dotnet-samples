# Colormap renderer

Demonstrates how to use a colormap renderer on raster layer.
Colormap renderers can be used to replace values on a raster layer with a color based on the original brightness value.

![](RasterColorMapRenderer.jpg)

## How it works
To apply a `ColormapRenderer` to a `RasterLayer`:

- Create a `Raster` from a raster file.
- Create a `RasterLayer` from the raster.
- Create an `IEnumerable<Color>` and use it as a colormap: colors at the beginning of the list replace the darkest values in the raster and colors at the end of the list replaced the brightest values of the raster.
- Create a colormap renderer with the colormap and apply it to the raster layer with `rasterLayer.Renderer = colormapRenderer`.

## Features
- `Map`
- `Basemap`
- `ColormapRenderer`
- `MapView`
- `Raster`
- `RasterLayer`

## Offline Data
Read more about how to set up the sample's offline data [here](http://links.esri.com/ArcGISRuntimeQtSamples).

Link | Local Location
---------|-------|
|[ShastaBW.tif raster](https://www.arcgis.com/home/item.html?id=cc68728b5904403ba637e1f1cd2995ae)| `<userhome>`/ArcGIS/Runtime/Data/raster/ShastaBW.tif |
