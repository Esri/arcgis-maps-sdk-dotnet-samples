// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.ConfigureSceneEnvironment
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Configure scene environment",
        category: "Scene",
        description: "Configure the environment settings in a local scene to change the lighting conditions and background appearance.",
        instructions: "At start-up, you will see a local scene with a set of scene environment controls. Adjusting the controls will change the scene's environment altering the presentation of the scene. Toggle the \"Stars\" and \"Atmosphere\" check boxes to enable or disable those features. Select a color from the dropdown to set a solid background color; selecting a new background color will disable the stars and atmosphere so you can see the new color. Switch between \"Sun\" and \"Virtual\" lighting, toggle \"Direct Shadows\", and adjust the hour slider to change the sun position.",
        tags: new[] { "3D", "environment", "lighting", "scene" })]
    public partial class ConfigureSceneEnvironment
    {
        // Background color options for the dropdown.
        private readonly List<(string Name, System.Drawing.Color Color)> _backgroundColorOptions = new()
        {
            ("None", System.Drawing.Color.Transparent),
            ("Black", System.Drawing.Color.Black),
            ("Red", System.Drawing.Color.Red),
            ("Orange", System.Drawing.Color.Orange),
            ("Yellow", System.Drawing.Color.Yellow),
            ("Green", System.Drawing.Color.Green),
            ("Blue", System.Drawing.Color.Blue),
            ("Purple", System.Drawing.Color.Purple),
            ("White", System.Drawing.Color.White),
        };

        // Track lighting date/time state.
        private DateTimeOffset _lightingDateTime = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero);
        private TimeSpan _lightingTimeZoneOffset = TimeSpan.Zero;
        private int _lightingHour = 12;

        public ConfigureSceneEnvironment()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Populate the background color combo box.
                foreach (var colorOption in _backgroundColorOptions)
                {
                    BackgroundColorComboBox.Items.Add(colorOption.Name);
                }

                // Create and load the scene from an ArcGIS Online web scene.
                var scene = new Scene(new Uri("https://www.arcgis.com/home/item.html?id=fcebd77958634ac3874bbc0e6b0677a4"));
                await scene.LoadAsync();

                // Set the scene on the local scene view.
                MySceneView.Scene = scene;

                // Read initial environment state from the web scene.
                var environment = scene.Environment;

                // Set sky controls to match the scene.
                AtmosphereCheckBox.IsChecked = environment.IsAtmosphereEnabled;
                StarsCheckBox.IsChecked = environment.AreStarsEnabled;

                // Set the background color control.
                var bgColor = environment.BackgroundColor;
                int colorIndex = _backgroundColorOptions.FindIndex(c => c.Color.ToArgb() == bgColor.ToArgb());
                BackgroundColorComboBox.SelectedIndex = colorIndex >= 0 ? colorIndex : 0;

                // Set lighting controls based on the scene's lighting.
                if (environment.Lighting is SunLighting sunLighting)
                {
                    SunRadioButton.IsChecked = true;
                    ShadowsCheckBox.IsChecked = sunLighting.AreDirectShadowsEnabled;

                    // Record the simulated time from the web scene.
                    _lightingDateTime = sunLighting.SimulatedDate;

                    // Record the time zone offset if one was set on the web scene.
                    if (sunLighting.DisplayTimeZone != null)
                    {
                        _lightingTimeZoneOffset = sunLighting.DisplayTimeZone.Value;
                    }

                    // Record the localized hour from the web scene lighting.
                    _lightingHour = _lightingDateTime.Add(_lightingTimeZoneOffset).Hour;
                    HourSlider.Value = _lightingHour;
                    UpdateHourLabel();

                    // Stars are available with sun lighting.
                    StarsCheckBox.IsEnabled = true;
                    HourSlider.IsEnabled = true;
                    HourLabel.Opacity = 1;
                    HourSlider.Opacity = 1;
                }
                else
                {
                    VirtualRadioButton.IsChecked = true;
                    ShadowsCheckBox.IsChecked = environment.Lighting.AreDirectShadowsEnabled;

                    // Stars and hour slider are not available with virtual lighting.
                    StarsCheckBox.IsEnabled = false;
                    HourSlider.IsEnabled = false;
                    HourLabel.Opacity = 0.4;
                    HourSlider.Opacity = 0.4;
                }

                // Wire up event handlers after initialization to avoid premature event handling.
                StarsCheckBox.Checked += StarsCheckBox_CheckedChanged;
                StarsCheckBox.Unchecked += StarsCheckBox_CheckedChanged;
                AtmosphereCheckBox.Checked += AtmosphereCheckBox_CheckedChanged;
                AtmosphereCheckBox.Unchecked += AtmosphereCheckBox_CheckedChanged;
                BackgroundColorComboBox.SelectionChanged += BackgroundColorComboBox_SelectionChanged;
                SunRadioButton.Checked += SunRadioButton_Checked;
                VirtualRadioButton.Checked += VirtualRadioButton_Checked;
                ShadowsCheckBox.Checked += ShadowsCheckBox_CheckedChanged;
                ShadowsCheckBox.Unchecked += ShadowsCheckBox_CheckedChanged;
                HourSlider.ValueChanged += HourSlider_ValueChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void StarsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MySceneView.Scene.Environment.AreStarsEnabled = StarsCheckBox.IsChecked == true;
        }

        private void AtmosphereCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MySceneView.Scene.Environment.IsAtmosphereEnabled = AtmosphereCheckBox.IsChecked == true;
        }

        private void BackgroundColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = BackgroundColorComboBox.SelectedIndex;
            if (index < 0 || index >= _backgroundColorOptions.Count) return;

            var newColor = _backgroundColorOptions[index].Color;
            MySceneView.Scene.Environment.BackgroundColor = newColor;

            // Disable atmosphere and stars so the background color is visible.
            MySceneView.Scene.Environment.IsAtmosphereEnabled = false;
            AtmosphereCheckBox.IsChecked = false;

            MySceneView.Scene.Environment.AreStarsEnabled = false;
            StarsCheckBox.IsChecked = false;
        }

        private void SunRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Create a new SunLighting object preserving current state.
            var sunLighting = new SunLighting(_lightingDateTime, ShadowsCheckBox.IsChecked == true);

            MySceneView.Scene.Environment.Lighting = sunLighting;

            // Enable stars and hour slider for sun lighting.
            StarsCheckBox.IsEnabled = true;
            HourSlider.IsEnabled = true;
            HourLabel.Opacity = 1;
            HourSlider.Opacity = 1;

            // Ensure the slider shows the correct hour.
            _lightingHour = _lightingDateTime.Add(_lightingTimeZoneOffset).Hour;
            HourSlider.Value = _lightingHour;
            UpdateHourLabel();
        }

        private void VirtualRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Create a new VirtualLighting object preserving shadow state.
            var virtualLighting = new VirtualLighting(ShadowsCheckBox.IsChecked == true);

            MySceneView.Scene.Environment.Lighting = virtualLighting;

            // Disable stars and hour slider for virtual lighting.
            StarsCheckBox.IsEnabled = false;
            HourSlider.IsEnabled = false;
            HourLabel.Opacity = 0.4;
            HourSlider.Opacity = 0.4;
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MySceneView.Scene.Environment.Lighting.AreDirectShadowsEnabled = ShadowsCheckBox.IsChecked == true;
        }

        private void HourSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int newHour = (int)HourSlider.Value;
            int hourDiff = newHour - _lightingHour;

            _lightingHour = newHour;
            UpdateHourLabel();

            // Update the time on the lighting object.
            _lightingDateTime = _lightingDateTime.Add(TimeSpan.FromHours(hourDiff));

            if (MySceneView.Scene.Environment.Lighting is SunLighting sunLighting)
            {
                sunLighting.SimulatedDate = _lightingDateTime;
            }
        }

        private void UpdateHourLabel()
        {
            HourLabel.Text = $"Hour: {_lightingHour}:00";
        }
    }
}
