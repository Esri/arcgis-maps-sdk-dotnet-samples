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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayDrawingStatus
{
    [Register("DisplayDrawingStatus")]
    public class DisplayDrawingStatus : UIViewController
    {

        // Create and hold reference to the used MapView
        private MapView _myMapView;

        private UIToolbar _toolbar;

        // Control to show the drawing status
        UIActivityIndicatorView _activityIndicator;

        public DisplayDrawingStatus()
        {
            Title = "Display drawing status";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the tool bar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 30, View.Bounds.Width, 30);

            _activityIndicator.Frame = new CoreGraphics.CGRect(0, 0, _toolbar.Bounds.Width, _toolbar.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapType.Topographic, 34.056, -117.196, 4);

            // Create uri to the used feature service
            var serviceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize a new feature layer
            ServiceFeatureTable myFeatureTable = new ServiceFeatureTable(serviceUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(myFeatureTable);

            // Make sure that the feature layer gets loaded
            await myFeatureLayer.LoadAsync();

            // Add the feature layer to the Map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Hook up the DrawStatusChanged event
            _myMapView.DrawStatusChanged += OnMapViewDrawStatusChanged;

            // Animate the activity spinner
            _activityIndicator.StartAnimating();
        }

        private void OnMapViewDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            // Make sure that the UI changes are done in the UI thread
            BeginInvokeOnMainThread(() =>
            {
                // Show the activity indicator if the map is drawing
                if (e.Status == DrawStatus.InProgress)
                    _activityIndicator.Hidden = false;
                else
                    _activityIndicator.Hidden = true;
            });
        }

        private void CreateLayout()
        {
            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();
            
            // Create a toolbar on the bottom of the display 
            _toolbar = new UIToolbar();
           
            // Create an activity indicator
            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
           
            // Create a UIBarButtonItem to show the activity indicator
            UIBarButtonItem indicatorButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            indicatorButton.CustomView = _activityIndicator;

            // Add the indicatorButton to an array of UIBarButtonItems
            var barButtonItems = new UIBarButtonItem[] { indicatorButton };

            // Add the UIBarButtonItems to the toolbar
            _toolbar.SetItems(barButtonItems, true);

            // Add the MapView to the Subview
            View.AddSubviews(_myMapView, _toolbar);
        }
    }
}