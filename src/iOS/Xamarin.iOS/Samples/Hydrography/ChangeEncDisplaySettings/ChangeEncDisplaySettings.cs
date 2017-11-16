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

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [Register("ChangeEncDisplaySettings")]
    public class ChangeEncDisplaySettings : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold a reference to the (static) app-wide ENC Mariner settings
        EncMarinerSettings _encMarinerSettings = EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings;

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

        public ChangeEncDisplaySettings()
        {
            Title = "ENC Display Settings";
        }

        private async void Initialize()
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
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the layer to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataSet myEncDataSet in myEncExchangeSet.DataSets)
            {
                var path = myEncDataSet.Name.Replace("\\", "/");
                // Create the cell and layer
                EncCell cell = new EncCell(Path.Combine(Path.GetDirectoryName(encPath), path));
                EncLayer myEncLayer = new EncLayer(cell);

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myEncLayer);

                // Wait for the layer to load
                await myEncLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(myEncLayer.FullExtent);
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
                    _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart;
                    break;

                case 1:
                default:
                    _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified;
                    break;
            }
        }

        private void AreaStyleChanged(object sender, EventArgs e)
        {
            switch (_areaSegment.SelectedSegment)
            {
                case 0:
                    _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain;
                    break;

                case 1:
                default:
                    _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized;
                    break;
            }
        }

        private void ColorSchemeChanged(object sender, EventArgs e)
        {
            // Apply the selected color scheme
            switch (_colorSchemeSegment.SelectedSegment)
            {
                case 0:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Day;
                    break;

                case 1:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Dusk;
                    break;

                case 2:
                default:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Night;
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
            Initialize();

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

            // Get the full path - the catalog is within a hierarchy in the downloaded data;
            // /SampleData/AddEncExchangeSet/ExchangeSet/ENC_ROOT/CATALOG.031
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