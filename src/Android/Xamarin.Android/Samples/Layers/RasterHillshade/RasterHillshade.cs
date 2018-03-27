// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.RasterHillshade
{
    [Activity(Label = "RasterHillshade")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("134d60f50e184e8fa56365f44e5ce3fb")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster hillshade renderer",
        "Layers",
        "This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).",
        "", "Featured")]
    public class RasterHillshade : Activity
    {
        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double ZFactor = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PixelSizePower = 1.0;
        private const double PixelSizeFactor = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PixelBitDepth = 8;

        // Map view control to show the hillshade
        private MapView _myMapView;

        // Store a reference to the layer
        private RasterLayer _rasterLayer;

        // Store a dictionary of slope types
        private Dictionary<string, SlopeType> _slopeTypeValues = new Dictionary<string, SlopeType>();

        // Store a selected slope type
        private SlopeType _slopeType = SlopeType.PercentRise;

        // TextView controls to show the selected azimuth and altitude values
        private TextView _azimuthTextView;
        private TextView _altitudeTextView;

        // Button to launch the slope type choices menu
        private Button _slopeTypeButton;

        // Button to apply the renderer.
        private Button _applyHillshadeButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Raster hillshade";

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a stack layout for the entire page
            LinearLayout mainLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create a button to show the available slope types for the user to choose from
            _slopeTypeButton = new Button(this)
            {
                Text = "Slope type: " + _slopeType.ToString()
            };

            // Show a popup menu of available slope types when the button is clicked
            _slopeTypeButton.Click += (s, e) => 
            {
                // Get the button that raised the event
                Button slopeChoiceButton = s as Button;

                // Create menu to show slope options
                PopupMenu slopeTypeMenu = new PopupMenu(this, slopeChoiceButton);
                slopeTypeMenu.MenuItemClick += (sndr,evt)=> 
                {
                    // Get the name of the selected slope type
                    string selectedSlope = evt.Item.TitleCondensedFormatted.ToString();

                    // Find and store the corresponding slope type enum
                    foreach (SlopeType slope in Enum.GetValues(typeof(SlopeType)))
                    {
                        if (slope.ToString() == selectedSlope)
                        {
                            _slopeType = slope;
                            _slopeTypeButton.Text = "Slope type: " + selectedSlope;
                        }
                    }
                };

                // Create menu options
                foreach (SlopeType slope in Enum.GetValues(typeof(SlopeType)))
                {
                    slopeTypeMenu.Menu.Add(slope.ToString());
                }

                // Show menu in the view
                slopeTypeMenu.Show();
            };

            // Create a slider (SeekBar) control for selecting an azimuth angle
            SeekBar azimuthSlider = new SeekBar(this)
            {
                // Set the slider width and height
                LayoutParameters = new ViewGroup.LayoutParams(350, 35),

                // Set a maximum slider value of 360 (minimum is 0)
                Max = 360
            };

            // When the slider changes, show the new value in the label
            azimuthSlider.ProgressChanged += (s, e) => 
            {
                _azimuthTextView.Text = e.Progress.ToString();
            };

            // Create a slider (SeekBar) control for selecting an altitude angle
            SeekBar altitudeSlider = new SeekBar(this)
            {
                // Set the slider width and height
                LayoutParameters = new ViewGroup.LayoutParams(350, 35),

                // Set a maximum slider value of 90 (minimum is 0)
                Max = 90
            };

            // When the slider changes, show the new value in the label
            altitudeSlider.ProgressChanged += (s, e) => 
            {
                _altitudeTextView.Text = e.Progress.ToString();
            };

            // Create labels (TextViews) to show the selected altitude and azimuth values
            _altitudeTextView = new TextView(this);
            _azimuthTextView = new TextView(this);

            // Create a horizontal layout for the altitude slider and text
            LinearLayout altitudeControls = new LinearLayout(this);
            altitudeControls.SetGravity(GravityFlags.Center);

            // Add the altitude selection controls
            altitudeControls.AddView(new TextView(this) { Text = "Altitude:" });
            altitudeControls.AddView(altitudeSlider);
            altitudeControls.AddView(_altitudeTextView);

            // Create a horizontal layout for the azimuth slider and text
            LinearLayout azimuthControls = new LinearLayout(this);
            azimuthControls.SetGravity(GravityFlags.Center);

            // Add the azimuth selection controls
            azimuthControls.AddView(new TextView(this) { Text = "Azimuth:" });
            azimuthControls.AddView(azimuthSlider);
            azimuthControls.AddView(_azimuthTextView);

            // Create a button to create and apply a hillshade renderer to the raster layer
            _applyHillshadeButton = new Button(this)
            {
                Text = "Apply hillshade"
            };

            // Handle the click event to apply the hillshade renderer
            _applyHillshadeButton.Click += ApplyHillshadeButton_Click;

            // Add the slope type button to the layout
            mainLayout.AddView(_slopeTypeButton);

            // Add the slider controls to the layout
            mainLayout.AddView(altitudeControls);
            mainLayout.AddView(azimuthControls);

            // Set the default values for the azimuth and altitude
            altitudeSlider.Progress = 45;
            azimuthSlider.Progress = 270;

            // Add the apply hillshade renderer button
            mainLayout.AddView(_applyHillshadeButton);

            // Create the map view
            _myMapView = new MapView(this);

            // Add the map view to the layout
            mainLayout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(mainLayout);
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap
            Map map = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset
            string filepath = GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Create a viewpoint with the raster's full extent
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map
            map.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map
            map.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view
            _myMapView.Map = map;

            // Add slope type values to the dictionary and picker
            foreach (var slope in Enum.GetValues(typeof(SlopeType)))
            {
                _slopeTypeValues.Add(slope.ToString(), (SlopeType)slope);
            }
        }

        private void ApplyHillshadeButton_Click(object sender, EventArgs e)
        {
            // Get the current azimuth and altitude parameter values
            int altitude = 0;
            int azimuth = 0;
            int.TryParse(_altitudeTextView.Text, out altitude);
            int.TryParse(_azimuthTextView.Text, out azimuth);

            // Create a hillshade renderer that uses the values selected by the user
            HillshadeRenderer hillshadeRenderer = new HillshadeRenderer(altitude, azimuth, ZFactor, _slopeType, PixelSizeFactor, PixelSizePower, PixelBitDepth);

            // Apply the new renderer to the raster layer
            _rasterLayer.Renderer = hillshadeRenderer;
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("134d60f50e184e8fa56365f44e5ce3fb", "srtm-hillshade", "srtm.tiff");
        }
    }
}