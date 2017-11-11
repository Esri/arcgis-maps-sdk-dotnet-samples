// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.EncDisplaySettings
{
    [Register("EncDisplaySettings")]
    public class EncDisplaySettings : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold references to the segment controls
        private UISegmentedControl _colorSchemeSegment = new UISegmentedControl("Day", "Dusk", "Night")
        {
            BackgroundColor = UIColor.LightGray,
            SelectedSegment = 0
        };

        private UISegmentedControl _areaSegment = new UISegmentedControl("Plain", "Symbolized")
        {
            BackgroundColor = UIColor.LightGray,
            SelectedSegment = 0
        };

        private UISegmentedControl _pointSegment = new UISegmentedControl("Paper Chart", "Simplified")
        {
            BackgroundColor = UIColor.LightGray,
            SelectedSegment = 0
        };

        public EncDisplaySettings()
        {
            Title = "ENC Display Settings";
        }

        private async Task InitializeAsync()
        {
            // Subscribe to event notifications
            _colorSchemeSegment.ValueChanged += ColorSchemeChanged;
            _areaSegment.ValueChanged += AreaStyleChanged;
            _pointSegment.ValueChanged += PointStyleChanged;

            // Initialize the map with an oceans basemap
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = await GetEncPath();

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet _encExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the layer to load
            await _encExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataSet _encDataSet in _encExchangeSet.DataSets)
            {
                var path = _encDataSet.Name.Replace("\\", "/");
                // Create the cell and layer
                EncCell cell = new EncCell(Path.Combine(Path.GetDirectoryName(encPath), path));
                EncLayer _encLayer = new EncLayer(cell);

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(_encLayer);

                // Wait for the layer to load
                await _encLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(_encLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            _myMapView.SetViewpoint(new Viewpoint(fullExtent));
        }

        private void PointStyleChanged(object sender, EventArgs e)
        {
            switch (_pointSegment.SelectedSegment)
            {
                case 0:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart;
                    break;

                case 1:
                default:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified;
                    break;
            }
        }

        private void AreaStyleChanged(object sender, EventArgs e)
        {
            switch (_areaSegment.SelectedSegment)
            {
                case 0:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain;
                    break;

                case 1:
                default:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized;
                    break;
            }
        }

        private void ColorSchemeChanged(object sender, EventArgs e)
        {
            // Apply the selected color scheme
            switch (_colorSchemeSegment.SelectedSegment)
            {
                case 0:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Day;
                    break;

                case 1:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Dusk;
                    break;

                case 2:
                default:
                    EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Night;
                    break;
            }
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubviews(_myMapView, _colorSchemeSegment, _areaSegment, _pointSegment);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            InitializeAsync();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            var topBound = NavigationController.NavigationBar.Bounds.Height;
            _colorSchemeSegment.Frame = new CoreGraphics.CGRect(20, topBound += 40, View.Bounds.Width - 40, 30);
            _areaSegment.Frame = new CoreGraphics.CGRect(20, topBound += 40, View.Bounds.Width - 40, 30);
            _pointSegment.Frame = new CoreGraphics.CGRect(20, topBound + 40, View.Bounds.Width - 40, 30);

            base.ViewDidLayoutSubviews();
        }

        private async Task<String> GetEncPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "AddEncExchangeSet", "ExchangeSet", "ENC_ROOT", "CATALOG.031");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("9d3ddb20afe3409eae25b3cdeb82215b", "AddEncExchangeSet");
            }

            return filepath;

            #endregion offlinedata
        }
    }
}