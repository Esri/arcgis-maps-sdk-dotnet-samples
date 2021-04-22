// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.MapRotation
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map rotation",
        category: "MapView",
        description: "Rotate a map.",
        instructions: "Use the slider to rotate the map.",
        tags: new[] { "rotate", "rotation", "viewpoint" })]
    public class MapRotation : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        private TextView _mapRotationLabel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            CreateLayout();
            Initialize();

            Title = "Map rotation";
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap
            Basemap myBasemap = new Basemap(BasemapStyle.ArcGISStreets);

            _myMapView.Map = new Map(myBasemap);
        }

        private void CreateLayout()
        {
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a slider (SeekBar) control for selecting an angle
            SeekBar angleSlider = new SeekBar(this)
            {
                // Set a maximum slider value (minimum is always 0)
                Max = 360,
                Progress = 0
            };

            // When the slider value (Progress) changes, rotate the map
            angleSlider.ProgressChanged += (s, e) =>
            {
                if (e.FromUser)
                {
                    // Set rotation asynchronously (no need to await the result)
                    _myMapView.SetViewpointRotationAsync(e.Progress);

                    // Display the MapView's rotation.
                    _mapRotationLabel.Text = $"{angleSlider.Progress:0}°";
                }
            };

            // Create a layout to show the slider and label
            LinearLayout sliderLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            sliderLayout.SetPadding(10, 10, 10, 10);
            angleSlider.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            _mapRotationLabel = new TextView(this);
            _mapRotationLabel.SetMaxWidth(150);
            _mapRotationLabel.SetMinWidth(150);
            sliderLayout.AddView(angleSlider);
            sliderLayout.AddView(_mapRotationLabel);

            // Display the MapView's initial rotation value.
            _mapRotationLabel.Text = $"{angleSlider.Progress:0}°";

            // Add the controls to the view
            layout.AddView(sliderLayout);
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Apply the layout to the app
            SetContentView(layout);
        }
    }
}