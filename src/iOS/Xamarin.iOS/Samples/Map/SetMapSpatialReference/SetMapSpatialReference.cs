// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.SetMapSpatialReference
{
    [Register("SetMapSpatialReference")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Set map spatial reference",
        "Map",
        "This sample demonstrates how you can set the spatial reference on a Map and all the operational layers would project accordingly.",
        "")]
    public class SetMapSpatialReference : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public SetMapSpatialReference()
        {
            Title = "Set map spatial reference";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition the view.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create new Map using the World Bonne spatial reference (54024).
            Map myMap = new Map(SpatialReference.Create(54024));

            // Adding a map image layer which can reproject itself to the map's spatial reference.
            // Note: Some layer such as tiled layer cannot reproject and will fail to draw if their spatial 
            // reference is not the same as the map's spatial reference.
            ArcGISMapImageLayer operationalLayer = new ArcGISMapImageLayer(new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));

            // Add operational layer to the Map.
            myMap.OperationalLayers.Add(operationalLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Add MapView to the view.
            View.AddSubviews(_myMapView);
        }
    }
}