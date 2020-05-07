// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.RasterColormapRenderer
{
    [Register("RasterColormapRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Colormap renderer",
        "Layers",
        "Apply a colormap renderer to a raster.",
        "Pan and zoom to explore the effect of the colormap applied to the raster.",
        "colormap", "data", "raster", "renderer", "visualization", "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    public class RasterColormapRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // A single band raster file.
        private readonly string _rasterPath = DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");

        public RasterColormapRenderer()
        {
            Title = "Raster colormap renderer";
        }

        private async void Initialize()
        {
            // Add an imagery basemap.
            Map map = new Map(Basemap.CreateImagery());

            // Load the raster file.
            Raster rasterFile = new Raster(_rasterPath);

            // Create the layer.
            RasterLayer rasterLayer = new RasterLayer(rasterFile);

            // Create a color map where values 0-149 are red and 150-249 are yellow.
            IEnumerable<Color> colors = new int[250]
               .Select((c, i) => i < 150 ? Color.Red : Color.Yellow);

            // Create a colormap renderer.
            ColormapRenderer colormapRenderer = new ColormapRenderer(colors);

            // Set the colormap renderer on the raster layer.
            rasterLayer.Renderer = colormapRenderer;

            // Add the layer to the map.
            map.OperationalLayers.Add(rasterLayer);

            // Add map to the mapview.
            _myMapView.Map = map;

            try
            {
                // Wait for the layer to load.
                await rasterLayer.LoadAsync();

                // Set the viewpoint.
                await _myMapView.SetViewpointGeometryAsync(rasterLayer.FullExtent, 15);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                CreateErrorDialog(e.Message);
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

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

        private void CreateErrorDialog(string message)
        {
            // Create Alert.
            UIAlertController okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            // Add Action.
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert.
            PresentViewController(okAlertController, true, null);
        }
    }
}