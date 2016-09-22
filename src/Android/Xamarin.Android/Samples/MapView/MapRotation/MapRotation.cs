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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntimeXamarin.Samples.MapRotation
{
    [Activity]
    public class MapRotation : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        private TextView _loadStatusTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Initialize();

            Title = "Map rotation";
        }

        void Initialize()
        {
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a slider (SeekBar) control for selecting an angle
            var angleSlider = new SeekBar(this);

            // Set a maximum slider value of 180 and a current value of 90 (minimum is always 0)
            angleSlider.Max = 180;
            angleSlider.Progress = 0;

            // When the slider value (Progress) changes, rotate the map   
            angleSlider.ProgressChanged += (object s, SeekBar.ProgressChangedEventArgs e) => 
            {
                if (e.FromUser)
                {
                    // Set rotation asynchronously (no need to await the result)
                    _myMapView.SetViewpointRotationAsync(e.Progress);

                    // Display the MapView's rotation.
                    _loadStatusTextView.Text = string.Format("{0:0}°", _myMapView.MapRotation);
                }
            };

            // Create a new Map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Create a new map view control to display the map
            _myMapView = new MapView();
            _myMapView.Map = myMap;

            // Set the current map rotation to match the default slider value
            // (no need to await the asynchronous call)
            _myMapView.SetViewpointRotationAsync(angleSlider.Progress);

            // Add a label to display the MapView's rotation.
            _loadStatusTextView = new TextView(this);
            layout.AddView(_loadStatusTextView);

            // Add the slider and map view to the layout
            layout.AddView(angleSlider);
            layout.AddView(_myMapView);

            // Apply the layout to the app
            SetContentView(layout);
        }
    }
}