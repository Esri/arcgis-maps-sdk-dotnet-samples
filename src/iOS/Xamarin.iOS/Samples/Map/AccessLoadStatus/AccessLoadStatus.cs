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
using Esri.ArcGISRuntime.UI;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AccessLoadStatus
{
    [Register("AccessLoadStatus")]
    public class AccessLoadStatus : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Control to show the Maps' load status
        private UITextView _loadStatusTextView;

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
                    "Maps' load status : {0}", 
                    e.Status.ToString());
            });
        }

        private void CreateLayout()
        {
            // Create control to show the maps' loading status
            _loadStatusTextView = new UITextView()
            {
                Frame = new CoreGraphics.CGRect(
                    0, yPageOffset, View.Bounds.Width, 40)
            };

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(
                0, yPageOffset + 40, View.Bounds.Width, View.Bounds.Height - yPageOffset - 40);
          
            // Add MapView to the page
            View.AddSubviews(_myMapView, _loadStatusTextView);
        }
    }
}