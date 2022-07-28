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
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.SetInitialMapLocation
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Set initial map location",
        category: "Map",
        description: "Display a basemap centered at an initial location and scale.",
        instructions: "When the map loads, note the specific location and scale of the initial map view.",
        tags: new[] { "LOD", "basemap", "center", "envelope", "extent", "initial", "lat", "latitude", "level of detail", "location", "long", "longitude", "scale", "zoom level" })]
    public class SetInitialMapLocation : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Set initial map location";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(BasemapStyle.ArcGISImagery);
            myMap.InitialViewpoint = new Viewpoint(-33.867886, -63.985, 15);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}