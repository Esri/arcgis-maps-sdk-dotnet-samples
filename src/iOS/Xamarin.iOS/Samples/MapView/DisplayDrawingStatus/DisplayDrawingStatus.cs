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
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayDrawingStatus
{
    [Register("DisplayDrawingStatus")]
    public class DisplayDrawingStatus : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView;

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

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapType.Topographic, 34.056, -117.196, 4);

            // Create uri to the used feature service
            var serviceUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

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

        private void OnMapViewDrawStatusChanged(object sender, DrawStatus e)
        {
            // Make sure that the UI changes are done in the UI thread
            BeginInvokeOnMainThread(() =>
            {
                // Show the activity indicator if the map is drawing
                if (e == DrawStatus.InProgress)
                    _activityIndicator.Hidden = false;
                else
                    _activityIndicator.Hidden = true;
            });
        }

        private void CreateLayout()
        {
            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();
            _myMapView.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, View.Bounds.Height - 30);

            // Create a toolbar on the bottom of the display 
            UIToolbar toolbar = new UIToolbar();
            toolbar.Frame = new CoreGraphics.CGRect(0, _myMapView.Bounds.Height, View.Bounds.Width, 30);

            // Create an activity indicator
            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            _activityIndicator.Frame = new CoreGraphics.CGRect(0, 0, toolbar.Bounds.Width, toolbar.Bounds.Height);

            // Create a UIBarButtonItem to show the activity indicator
            UIBarButtonItem indicatorButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            indicatorButton.CustomView = _activityIndicator;

            // Add the indicatorButton to an array of UIBarButtonItems
            var barButtonItems = new UIBarButtonItem[] { indicatorButton };

            // Add the UIBarButtonItems to the toolbar
            toolbar.SetItems(barButtonItems, true);

            // Add the MapView to the Subview
            View.AddSubviews(_myMapView, toolbar);
        }
    }
}