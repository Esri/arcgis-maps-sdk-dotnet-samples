// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ControlAnnotationSublayerVisibility
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Control annotation sublayer visibility",
        category: "Layers",
        description: "Use annotation sublayers to gain finer control of annotation layer subtypes.",
        instructions: "Start the sample and take note of the visibility of the annotation. Zoom in and out to see the annotation turn on and off based on scale ranges set on the data.",
        tags: new[] { "annotation", "scale", "text", "utilities", "visualization", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("ControlAnnotationSublayerVisibility.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("b87307dcfb26411eb2e92e1627cb615b")]
    public class ControlAnnotationSublayerVisibility : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private CheckBox _openCheckBox;
        private CheckBox _closedCheckBox;
        private TextView _status;

        // Mobile map package that contains annotation layers.
        private MobileMapPackage _mobileMapPackage;

        // Sub layers of the annotation layer.
        private AnnotationSublayer _openSublayer;
        private AnnotationSublayer _closedSublayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Control annotation sublayer visibility";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Load the mobile map package.
                _mobileMapPackage = new MobileMapPackage(DataManager.GetDataFolder("b87307dcfb26411eb2e92e1627cb615b", "GasDeviceAnno.mmpk"));
                await _mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                _myMapView.Map = _mobileMapPackage.Maps.First();

                // Get the annotation layer from the MapView operational layers.
                AnnotationLayer annotationLayer = (AnnotationLayer)_myMapView.Map.OperationalLayers.Where(layer => layer is AnnotationLayer).First();

                // Load the annotation layer.
                await annotationLayer.LoadAsync();

                // Get the annotation sub layers.
                _closedSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[0];
                _openSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[1];

                // Set the label content.
                _openCheckBox.Text = $"{_openSublayer.Name} (1:{_openSublayer.MaxScale} - 1:{_openSublayer.MinScale})";
                _closedCheckBox.Text = _closedSublayer.Name;

                // Enable the check boxes.
                _openCheckBox.Enabled = true;
                _closedCheckBox.Enabled = true;

                // Add event handler for changing the text to indicate whether the "open" sublayer is visible at the current scale.
                _myMapView.ViewpointChanged += (s, e) =>
                {
                    // Check if the sublayer is visible at the current map scale.
                    if (_openSublayer.IsVisibleAtScale(_myMapView.MapScale))
                    {
                        _openCheckBox.SetTextColor(Color.Black);
                    }
                    else
                    {
                        _openCheckBox.SetTextColor(Color.Gray);
                    }

                    // Set the current map scale text.
                    _status.Text = "Current map scale: 1:" + (int)_myMapView.MapScale;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OpenCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_openSublayer != null) _openSublayer.IsVisible = _openCheckBox.Checked;
        }

        private void ClosedCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_closedSublayer != null) _closedSublayer.IsVisible = _closedCheckBox.Checked;
        }

        private void CreateLayout()
        {
            // Load the UI from the axml file.
            SetContentView(Resource.Layout.ControlAnnotationSublayerVisibility);

            // Get the UI elements from the axml resource.
            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _openCheckBox = FindViewById<CheckBox>(Resource.Id.openCheckBox);
            _closedCheckBox = FindViewById<CheckBox>(Resource.Id.closedCheckBox);
            _status = FindViewById<TextView>(Resource.Id.statusLabel);

            // Add listeners for the checkboxes.
            _openCheckBox.CheckedChange += OpenCheckBoxChanged;
            _closedCheckBox.CheckedChange += ClosedCheckBoxChanged;
        }
    }
}