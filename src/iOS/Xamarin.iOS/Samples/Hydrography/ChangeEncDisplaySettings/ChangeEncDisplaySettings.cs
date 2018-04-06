// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [Register("ChangeEncDisplaySettings")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change ENC display settings",
        "Hydrography",
        "This sample demonstrates how to control ENC environment settings. These settings apply to the display of all ENC content in your app.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public class ChangeEncDisplaySettings : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold a reference to the (static) app-wide ENC Mariner settings
        EncMarinerSettings _encMarinerSettings = EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings;

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
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT",
                "CATALOG.031");

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet( encPath );

            // Wait for the layer to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
            {
                EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

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
            // Apply the selected point symbolization
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
            // Apply the selected area symbolization
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

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // ENC environment settings apply to the entire application
            // They need to be reset after leaving the sample to avoid affecting other samples
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings.ResetToDefaults();
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
    }
}