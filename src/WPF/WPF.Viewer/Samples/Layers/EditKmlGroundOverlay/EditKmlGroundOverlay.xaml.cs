// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;

namespace ArcGIS.WPF.Samples.EditKmlGroundOverlay
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit KML ground overlay",
        category: "Layers",
        description: "Edit the values of a KML ground overlay.",
        instructions: "Use the slider to adjust the opacity of the ground overlay.",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "imagery" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("1f3677c24b2c446e96eaf1099292e83e")]
    public partial class EditKmlGroundOverlay
    {
        private readonly Uri _imageryUri = new Uri(DataManager.GetDataFolder("1f3677c24b2c446e96eaf1099292e83e", "1944.jpg"));

        public EditKmlGroundOverlay()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a scene for the sceneview.
            Scene myScene = new Scene(BasemapStyle.ArcGISImageryStandard);
            MySceneView.Scene = myScene;

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
            MySceneView.Scene.OperationalLayers.Add(layer);

            // Move the viewpoint to the ground overlay.
            MySceneView.SetViewpoint(new Viewpoint(overlay.Geometry, new Camera(overlay.Geometry.Extent.GetCenter(), 1250, 45, 60, 0)));

            // Add an event handler for the on-screen slider.
            OpacitySlider.ValueChanged += (s, e) =>
            {
                // Change the color of the KML ground overlay image to edit the alpha-value. (Other color values are left as-is in the original image.)
                overlay.Color = System.Drawing.Color.FromArgb((int)(e.NewValue), 0, 0, 0);

                // Make the value an integer (For the UI).
                OpacitySlider.Value = (int)OpacitySlider.Value;
            };
        }
    }
}