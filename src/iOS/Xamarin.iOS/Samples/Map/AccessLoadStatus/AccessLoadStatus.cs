// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

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
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Control to show the Map's load status
        private UILabel _loadStatusTextView;

        // Toolbar to put behind the label
        private UIToolbar _toolbar = new UIToolbar();

        public AccessLoadStatus()
        {
            Title = "Access load status";
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

            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
            _loadStatusTextView.Frame = new CoreGraphics.CGRect(5, View.Bounds.Height - 35, View.Bounds.Width - 10, 30);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Register to handle loading status changes
            myMap.LoadStatusChanged += OnMapsLoadStatusChanged;

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnMapsLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // Make sure that the UI changes are done in the UI thread
            InvokeOnMainThread(() =>
            {
                // Update the load status information
                _loadStatusTextView.Text = string.Format(
                    "Map's load status: {0}", 
                    e.Status.ToString());
            });
        }

        private void CreateLayout()
        {
            // Create control to show the maps' loading status
            _loadStatusTextView = new UILabel(){
                TextAlignment = UITextAlignment.Center
            };
  
            // Add MapView to the page
            View.AddSubviews(_myMapView, _toolbar, _loadStatusTextView);
        }
    }
}