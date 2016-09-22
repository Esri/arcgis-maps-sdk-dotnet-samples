// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntimeXamarin.Samples.AccessLoadStatus
{
    [Activity]
    public class AccessLoadStatus : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Control to show the Maps' load status
        private TextView _loadStatusTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Access load status";

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
            RunOnUiThread(() =>
            {
                // Update the load status information
                _loadStatusTextView.Text = string.Format(
                    "Maps' load status : {0}", 
                    e.Status.ToString());
            });
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create control to show the maps' loading status
            _loadStatusTextView = new TextView(this);
            layout.AddView(_loadStatusTextView);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}