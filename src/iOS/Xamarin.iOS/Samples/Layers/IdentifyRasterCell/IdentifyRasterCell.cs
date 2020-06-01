﻿// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.IdentifyRasterCell
{
    [Register("IdentifyRasterCell")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify raster cell",
        category: "Layers",
        description: "Get the cell value of a local raster at the tapped location and display the result in a callout.",
        instructions: "Tap or move your cursor over an area of the raster to identify a raster cell and display it's attributes in a callout.",
        tags: new[] { "NDVI", "band", "cell", "cell value", "continuous", "discrete", "identify", "pixel", "pixel value", "raster" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("b5f977c78ec74b3a8857ca86d1d9b318")]
    public class IdentifyRasterCell : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Raster layer to display raster data on the map.
        private RasterLayer _rasterLayer;

        public IdentifyRasterCell()
        {
            Title = "Identify raster cell";
        }

        private async void Initialize()
        {
            // Define a new map with Wgs84 Spatial Reference.
            var map = new Map(BasemapType.Oceans, latitude: -34.1, longitude: 18.6, levelOfDetail: 9);

            // Get the file name for the raster.
            string filepath = DataManager.GetDataFolder("b5f977c78ec74b3a8857ca86d1d9b318", "SA_EVI_8Day_03May20.tif");

            // Load the raster file.
            var raster = new Raster(filepath);

            // Initialize the raster layer.
            _rasterLayer = new RasterLayer(raster);

            // Add the raster layer to the map.
            map.OperationalLayers.Add(_rasterLayer);

            // Add map to the map view.
            _myMapView.Map = map;

            try
            {
                // Wait for the layer to load.
                await _rasterLayer.LoadAsync();

                // Set the viewpoint.
                await _myMapView.SetViewpointGeometryAsync(_rasterLayer.FullExtent);
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private async void MapTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the result for where the user tapped on the raster layer.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_rasterLayer, e.Position, 1, false, 1);

                // If no cell was identified, dismiss the callout.
                if (!identifyResult.GeoElements.Any())
                {
                    _myMapView.DismissCallout();
                    return;
                }

                // Create a StringBuilder to display information to the user.
                var stringBuilder = new StringBuilder();

                // Get the identified raster cell.
                GeoElement cell = identifyResult.GeoElements.First();

                // Loop through the attributes (key/value pairs).
                foreach (KeyValuePair<string, object> keyValuePair in cell.Attributes)
                {
                    // Add the key/value pair to the string builder.
                    stringBuilder.AppendLine($"{keyValuePair.Key}: {keyValuePair.Value}");
                }

                // Get the x and y values of the cell.
                double x = cell.Geometry.Extent.XMin;
                double y = cell.Geometry.Extent.YMin;

                // Add the X & Y coordinates where the user clicked raster cell to the string builder.
                stringBuilder.AppendLine($"X: {Math.Round(x, 4)}\nY: {Math.Round(y, 4)}");

                // Create a label using the string.
                var cellLabel = new UILabel
                {
                    Text = stringBuilder.ToString(),
                    TextAlignment = UITextAlignment.Center,
                    AdjustsFontSizeToFitWidth = false,
                    Font = UIFont.SystemFontOfSize(15),
                    TextColor = UIColor.Black,
                    Lines = 0,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    MinimumFontSize = 25
                };

                // Display the label in the map view.
                _myMapView.ShowCalloutAt(e.Location, cellLabel);
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapTapped;
        }
    }
}