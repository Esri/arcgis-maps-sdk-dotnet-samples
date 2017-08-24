// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ExportTiles
{
    [Register("ExportTiles")]
    public class ExportTiles : UIViewController
    {
        // Reference to the MapView used in the app
        private MapView _myMapView;

        public ExportTiles()
        {
            Title = "Export tiles";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {

            // Create a new map with the oceans basemap and add it to the map view
            Map myMap = new Map(Basemap.CreateStreets());
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Store the main view's width and height
            var appViewWidth = View.Bounds.Width;
            var appViewHeight = View.Bounds.Height;

            // Create a new MapView
            _myMapView = new MapView();

            // Add the MapView, UITextField, and UIButton to the page
            View.AddSubviews(_myMapView);
            View.BackgroundColor = UIColor.White;
        }
    }
}