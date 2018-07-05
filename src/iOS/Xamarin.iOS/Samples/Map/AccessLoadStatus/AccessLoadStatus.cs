// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AccessLoadStatus
{
    [Register("AccessLoadStatus")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Access load status",
        "Map",
        "This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.",
        "")]
    public class AccessLoadStatus : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UILabel _loadStatusTextView = new UILabel
        {
            TextAlignment = UITextAlignment.Center
        };

        public AccessLoadStatus()
        {
            Title = "Access load status";
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
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _loadStatusTextView.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Register to handle loading status changes.
            myMap.LoadStatusChanged += OnMapsLoadStatusChanged;

            // Show the map.
            _myMapView.Map = myMap;
        }

        private void OnMapsLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // Make sure that the UI changes are done in the UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the load status information.
                _loadStatusTextView.Text = $"Map's load status: {e.Status}";
            });
        }

        private void CreateLayout()
        {
            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar, _loadStatusTextView);
        }
    }
}