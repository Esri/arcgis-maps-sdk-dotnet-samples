// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.AddBuildingSceneLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Add building scene layer",
        "Layers",
        "Add a layer to a local scene to visualize and interact with 3D building models developed using Building Information Modeling (BIM) tools.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class AddBuildingSceneLayer
    {
        private BuildingSublayer _overviewSublayer;
        private BuildingSublayer _fullModelSublayer;

        public AddBuildingSceneLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            var scene = new Scene(BasemapStyle.ArcGISTopographic, SceneViewingMode.Local);

            var elevationSource = new ArcGISTiledElevationSource(
                new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            scene.BaseSurface.ElevationSources.Add(elevationSource);

            var buildingSceneLayer = new BuildingSceneLayer(
                new Uri("https://3dcities.maps.arcgis.com/home/item.html?id=ebae5f766aac41ba9ec588f66028a6a9"));
            buildingSceneLayer.AltitudeOffset = 2;

            await buildingSceneLayer.LoadAsync();
            scene.OperationalLayers.Add(buildingSceneLayer);
            MySceneView.Scene = scene;

            var camera = new Camera(
                           new MapPoint(-13045148, 4036775, 454, SpatialReferences.WebMercator),
                           heading: 343,
                           pitch: 64,
                           roll: 0);
            MySceneView.SetViewpointCamera(camera);

            var sublayers = buildingSceneLayer.Sublayers;
            _overviewSublayer = sublayers.FirstOrDefault(s =>
                string.Equals(s.Name, "Overview", StringComparison.OrdinalIgnoreCase));
            _fullModelSublayer = sublayers.FirstOrDefault(s =>
                string.Equals(s.Name, "Full Model", StringComparison.OrdinalIgnoreCase));

            // Enable checkbox if full model sublayer exists
            if (_fullModelSublayer != null)
            {
                FullModelCheckBox.IsEnabled = true;
            }
            else
            {
                FullModelCheckBox.IsEnabled = false;
            }

            // Set initial visibility
            UpdateSublayerVisibility(false);

            // Wire up checkbox event
            FullModelCheckBox.Checked += OnCheckBoxChanged;
            FullModelCheckBox.Unchecked += OnCheckBoxChanged;
        }

        private void OnCheckBoxChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            UpdateSublayerVisibility(FullModelCheckBox.IsChecked == true);
        }

        private void UpdateSublayerVisibility(bool showFullModel)
        {
            if (_fullModelSublayer != null)
                _fullModelSublayer.IsVisible = showFullModel;

            if (_overviewSublayer != null)
                _overviewSublayer.IsVisible = !showFullModel;
        }
    }
}