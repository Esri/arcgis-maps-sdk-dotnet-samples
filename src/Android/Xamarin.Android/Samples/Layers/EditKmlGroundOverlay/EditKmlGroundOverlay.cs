// Copyright 2019 Esri.
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
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.EditKmlGroundOverlay
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit KML ground overlay",
        "Layers",
        "Edit the values of a KML ground overlay.",
        "Use the slider to adjust the opacity of the ground overlay.",
        "KML", "KMZ", "Keyhole", "OGC", "imagery", "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1f3677c24b2c446e96eaf1099292e83e")]
    public class EditKmlGroundOverlay : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private SeekBar _slider;
        private TextView _valueLabel;

        // Uri of the image for the ground overlay.
        private readonly Uri _imageryUri = new Uri(DataManager.GetDataFolder("1f3677c24b2c446e96eaf1099292e83e", "1944.jpg"));

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Edit KML ground overlay";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a scene for the sceneview.
            Scene myScene = new Scene(Basemap.CreateImagery());
            _mySceneView.Scene = myScene;

            // Create a geometry for the ground overlay.
            Envelope overlayGeometry = new Envelope(-123.066227926904, 44.04736963555683, -123.0796942287304, 44.03878298600624, SpatialReferences.Wgs84);

            // Create a KML Icon for the overlay image.
            KmlIcon overlayImage = new KmlIcon(_imageryUri);

            // Create the KML ground overlay.
            KmlGroundOverlay overlay = new KmlGroundOverlay(overlayGeometry, overlayImage);

            // Set the rotation of the ground overlay.
            overlay.Rotation = -3.046024799346924;

            // Create a KML dataset with the ground overlay as the root node.
            KmlDataset dataset = new KmlDataset(overlay);

            // Create a KML layer for the scene view.
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(layer);

            // Move the viewpoint to the ground overlay.
            _mySceneView.SetViewpoint(new Viewpoint(overlay.Geometry, new Camera(overlay.Geometry.Extent.GetCenter(), 1250, 45, 60, 0)));

            // Add an event handler for the on-screen slider.
            _slider.ProgressChanged += (s, e) =>
            {
                // Change the color of the KML ground overlay image to edit the alpha-value. (Other color values are left as-is in the original image.)
                overlay.Color = System.Drawing.Color.FromArgb(e.Progress, 0, 0, 0);

                // Display the value.
                _valueLabel.Text = e.Progress.ToString();
            };
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.EditKmlGroundOverlay);

            // Update control references to point to the controls defined in the layout.
            _mySceneView = FindViewById<SceneView>(Resource.Id.SceneView);
            _slider = FindViewById<SeekBar>(Resource.Id.Slider);
            _valueLabel = FindViewById<TextView>(Resource.Id.ValueLabel);
        }
    }
}