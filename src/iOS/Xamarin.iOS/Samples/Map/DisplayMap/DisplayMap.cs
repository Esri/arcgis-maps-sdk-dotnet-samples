// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayMap
{
    [Register("DisplayMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a map",
        "Map",
        "This sample demonstrates how to display a map with a basemap.",
        "")]
    public class DisplayMap : UIViewController
    {
        // Create and hold a reference to the map view.
        private readonly MapView _myMapView = new MapView();

        public DisplayMap()
        {
            Title = "Display a map";
        }

        private void Initialize()
        {
            // Show an imagery basemap
            _myMapView.Map = new Map(Basemap.CreateImagery());
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Reposition controls.
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

            base.ViewDidLayoutSubviews();
        }
    }
}