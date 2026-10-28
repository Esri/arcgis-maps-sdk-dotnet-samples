// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.FilterMosaicDatasetRasterByTimeExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter mosaic dataset raster by time extent",
        category: "MapView",
        description: "Visualize changes in rasters over time by filtering a mosaic dataset raster for a selected timestamp.",
        instructions: "When the sample starts, the mosaic dataset raster is displayed in a map view and the available acquisition timestamps are loaded. Use the previous and next controls to step through the distinct timestamps in chronological order. Use the first and last controls to jump to the earliest and latest timestamps. The currently selected timestamp is shown in UTC, and the map view updates to display imagery for that time. Use the slider to adjust the opacity of the raster layer to compare it to the imagery basemap.",
        tags: new[] { "aerial imagery", "date", "mosaic dataset", "raster", "time", "time extent", "time-aware", "time-enabled", "timestamp" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("5d1c805850054a098667cdb217d76476")]
    public partial class FilterMosaicDatasetRasterByTimeExtent
    {
        private RasterLayer _rasterLayer;
        private IReadOnlyList<DateTimeOffset> _timestamps = Array.Empty<DateTimeOffset>();
        private int _selectedIndex;

        public FilterMosaicDatasetRasterByTimeExtent()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Display an imagery basemap for comparison with the historic rasters.
                MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

                // Load the mosaic dataset from the downloaded mobile geodatabase.
                string databasePath = DataManager.GetDataFolder(
                    "5d1c805850054a098667cdb217d76476", "el_toro.geodatabase");
                string rasterName = MosaicDatasetRaster.GetNames(databasePath).First();
                MosaicDatasetRaster raster = new MosaicDatasetRaster(databasePath, rasterName);
                await raster.LoadAsync();

                // Create and load a raster layer, then add it to the map.
                _rasterLayer = new RasterLayer(raster);
                await _rasterLayer.LoadAsync();
                MyMapView.Map.OperationalLayers.Add(_rasterLayer);

                // The distinct timestamps are sorted chronologically. Stepping through them
                // avoids time ranges with no imagery when acquisitions are irregular.
                _timestamps = (await raster.GetDistinctTimestampsAsync(CancellationToken.None)).ToList();

                // Frame the mosaic dataset with the same padding for every timestamp.
                await MyMapView.SetViewpointGeometryAsync(_rasterLayer.FullExtent, 100);

                if (_timestamps.Count > 0)
                {
                    SelectTimestamp(0);
                }
                else
                {
                    TimestampText.Text = "No timestamps available";
                }

                OpacitySlider.IsEnabled = true;
            }
            catch (Exception ex)
            {
                TimestampText.Text = "Unable to load raster";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void SelectTimestamp(int index)
        {
            _selectedIndex = index;

            // An instant uses the same start and end time to show only that acquisition.
            MyMapView.TimeExtent = new TimeExtent(_timestamps[index]);
            TimestampText.Text = _timestamps[index].UtcDateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            FirstButton.IsEnabled = PreviousButton.IsEnabled = index > 0;
            NextButton.IsEnabled = LastButton.IsEnabled = index < _timestamps.Count - 1;
        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTimestamp(0);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTimestamp(_selectedIndex - 1);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTimestamp(_selectedIndex + 1);
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTimestamp(_timestamps.Count - 1);
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_rasterLayer != null)
            {
                double opacity = Math.Round(e.NewValue, 2);
                _rasterLayer.Opacity = opacity;
                OpacityValue.Text = opacity.ToString("P0", CultureInfo.InvariantCulture);
            }
        }
    }
}
