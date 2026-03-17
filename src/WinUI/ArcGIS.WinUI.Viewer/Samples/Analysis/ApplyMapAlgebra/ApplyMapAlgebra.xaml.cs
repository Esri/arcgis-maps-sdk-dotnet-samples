// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Analysis;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.ApplyMapAlgebra
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply map algebra",
        category: "Analysis",
        description: "Apply map algebra to an elevation raster to floor, mask, and categorize the elevation values into discrete integer-based categories.",
        instructions: "When the sample opens, it displays the source elevation raster. Select the **Categorize** button to generate a raster with three distinct ice age related geomorphological categories (raised shore line areas in blue, ice free high ground in brown and areas covered by ice in teal). After processing completes, use the radio buttons to switch between the map algebra results raster and the original elevation raster.",
        tags: new[] { "elevation", "map algebra", "raster", "spatial analysis", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("aa97788593e34a32bcaae33947fdc271")]
    public partial class ApplyMapAlgebra
    {
        private RasterLayer _elevationRasterLayer;
        private RasterLayer _geomorphicRasterLayer;
        private ContinuousField _elevationField;
        private DiscreteField _geomorphicCategoryField;

        public ApplyMapAlgebra()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map with a dark hillshade basemap and set the initial viewpoint to the Isle of Arran, Scotland.
            MyMapView.Map = new Map(BasemapStyle.ArcGISHillshadeDark)
            {
                InitialViewpoint = new Viewpoint(55.584612, -5.234218, 300000)
            };

            try
            {
                // Get the path to the locally stored elevation raster file.
                string rasterPath = DataManager.GetDataFolder("aa97788593e34a32bcaae33947fdc271", "arran.tif");

                // Create a continuous field from the elevation raster file.
                _elevationField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0);

                // Display the source elevation raster on the map.
                DisplayElevationRaster(rasterPath);

                // Enable the categorize button once the data is loaded.
                CategorizeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
        }

        private void DisplayElevationRaster(string rasterPath)
        {
            // Create a raster layer from the elevation raster file.
            _elevationRasterLayer = new RasterLayer(new Raster(rasterPath));

            // Create a stretch renderer to visualize the elevation raster layer using the surface preset color ramp.
            _elevationRasterLayer.Renderer = new StretchRenderer(
                new MinMaxStretchParameters(new[] { 0.0 }, new[] { 874.0 }),
                gammas: new[] { 1.0 },
                estimateStatistics: false,
                colorRamp: ColorRamp.Create(PresetColorRampType.Surface, 256));

            // Set opacity to allow the basemap hillshade to show through.
            _elevationRasterLayer.Opacity = 0.5;

            // Add the elevation raster layer to the map.
            MyMapView.Map.OperationalLayers.Add(_elevationRasterLayer);
        }

        private async Task CreateGeomorphicCategoryField()
        {
            // Create a continuous field function from the elevation field.
            var continuousFieldFunction = ContinuousFieldFunction.Create(_elevationField);

            // Mask out values below sea level to categorize only land.
            var elevationFieldFunction = continuousFieldFunction.Mask(
                continuousFieldFunction.IsGreaterThanOrEqualTo(0));

            // Round elevation values down to the lower 10 m interval, then convert to a discrete field function.
            var tenMeterBinField = ((elevationFieldFunction / 10).Floor() * 10).ToDiscreteFieldFunction();

            // Create boolean fields for each geomorphic category based on the nearest 10 m interval field.
            var isRaisedShoreline = tenMeterBinField.IsGreaterThanOrEqualTo(0)
                .LogicalAnd(tenMeterBinField.IsLessThan(10));

            // Operator overloads (>=, <, &) can be used in place of IsGreaterThanOrEqualTo, IsLessThan, and LogicalAnd.
            var isIceCovered = (tenMeterBinField >= 10) & (tenMeterBinField < 600);

            var isIceFreeHighGround = tenMeterBinField.IsGreaterThanOrEqualTo(600);

            // Assign geomorphic categories based on the boolean fields:
            // raised shoreline = 1, ice covered = 2, ice-free high ground = 3.
            _geomorphicCategoryField = await tenMeterBinField
                .ReplaceIf(selection: isRaisedShoreline, value: 1)
                .ReplaceIf(selection: isIceCovered, value: 2)
                .ReplaceIf(selection: isIceFreeHighGround, value: 3)
                .EvaluateAsync();
        }

        private async void OnCategorizeClicked(object sender, RoutedEventArgs e)
        {
            CategorizeButton.IsEnabled = false;
            CategorizeButton.Content = "Map algebra computing...";

            try
            {
                // Build and evaluate the map algebra expression to generate the geomorphic category field.
                await CreateGeomorphicCategoryField();
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error creating geomorphic category field").ShowAsync();
                CategorizeButton.IsEnabled = true;
                CategorizeButton.Content = "Categorize";
                return;
            }

            try
            {
                // Export the discrete field to a temporary folder.
                string tempDir = Path.Combine(Path.GetTempPath(), "geomorphic_temp_files");
                Directory.CreateDirectory(tempDir);

                // Clean up any previously exported files.
                const string filenamesPrefix = "geomorphicCategorization";
                foreach (string file in Directory.GetFiles(tempDir, $"{filenamesPrefix}*"))
                    File.Delete(file);

                // Export the discrete field result to a raster file.
                var exportedFiles = await _geomorphicCategoryField.ExportToFilesAsync(tempDir, filenamesPrefix);

                if (exportedFiles.Count == 0 || !File.Exists(exportedFiles[0]))
                {
                    await new MessageDialog2("Exported geomorphic categorization file does not exist.", "Export Error").ShowAsync();
                    CategorizeButton.IsEnabled = true;
                    CategorizeButton.Content = "Categorize";
                    return;
                }

                // Display the geomorphic categories on the map.
                DisplayGeomorphicCategories(exportedFiles[0]);

                // Show the layer toggle controls and hide the categorize button.
                CategorizePanel.Visibility = Visibility.Collapsed;
                LayerTogglePanel.Visibility = Visibility.Visible;
                GeomorphicRadioButton.IsChecked = true;
                UpdateVisibleRasterLayer();
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error exporting geomorphic categorization").ShowAsync();
                CategorizeButton.IsEnabled = true;
                CategorizeButton.Content = "Categorize";
            }
        }

        private void DisplayGeomorphicCategories(string rasterFilePath)
        {
            // Create a raster layer from the exported geomorphic categorization file.
            _geomorphicRasterLayer = new RasterLayer(new Raster(rasterFilePath));

            // Create an array of colors for the geomorphic categories.
            // The array index maps directly to the pixel value in the raster.
            var colors = new Color[]
            {
                Color.Transparent,              // 0 = no category (NoData pixels are hidden)
                Color.FromArgb(25, 118, 210),   // 1 = raised shoreline - blue
                Color.FromArgb(128, 203, 196),  // 2 = ice covered - teal
                Color.FromArgb(121, 85, 72),    // 3 = ice-free high ground - brown
            };

            // Create a colormap renderer to assign colors to pixel values and apply it to the raster layer.
            _geomorphicRasterLayer.Renderer = new ColormapRenderer(colors);

            // Set opacity to allow the basemap hillshade to show through.
            _geomorphicRasterLayer.Opacity = 0.5;

            // Add the geomorphic categorization raster layer to the map.
            MyMapView.Map.OperationalLayers.Add(_geomorphicRasterLayer);
        }

        private void OnRasterLayerSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisibleRasterLayer();
        }

        private void UpdateVisibleRasterLayer()
        {
            if (_elevationRasterLayer == null) return;
            bool showGeomorphic = GeomorphicRadioButton.IsChecked == true;
            _elevationRasterLayer.IsVisible = !showGeomorphic;
            if (_geomorphicRasterLayer != null)
                _geomorphicRasterLayer.IsVisible = showGeomorphic;
        }
    }
}