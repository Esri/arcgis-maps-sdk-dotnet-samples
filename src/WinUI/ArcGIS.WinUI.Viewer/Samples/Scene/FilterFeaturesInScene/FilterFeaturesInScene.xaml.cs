// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polygon = Esri.ArcGISRuntime.Geometry.Polygon;

namespace ArcGIS.WinUI.Samples.FilterFeaturesInScene
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter features in scene",
        category: "Scene",
        description: "Filter 3D scene features out of a given geometry with a polygon filter.",
        instructions: "The sample initializes showing the \"Navigation\" 3D Basemap. Click the \"Filter 3D buildings in extent\" button, to set a `SceneLayerPolygonFilter` and filter out the Esri 3D buildings within the extent of a detailed buildings scene layer. Notice how the Esri 3D buildings within and intersecting the extent of the detailed buildings layer are hidden. Click the \"Show detailed buildings\" button to load a scene layer that contains more detailed buildings. Click the \"Reset scene\" button to hide the detailed buildings scene layer and clear the 3D buildings filter.",
        tags: new[] { "3D", "buildings", "disjoint", "exclude", "extent", "filter", "hide", "polygon" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class FilterFeaturesInScene
    {
        // ArcGIS Online services.
        private const string Basemap3D = "https://www.arcgis.com/home/item.html?id=00a5f468dda941d7bf0b51c144aae3f0";
        private const string ElevationSource = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";
        private const string DetailedBuildings = "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/SanFrancisco_Bldgs/SceneServer";

        private ArcGISSceneLayer _detailedBuildingsSceneLayer;
        private ArcGISSceneLayer _3dBuildingsSceneLayer;

        private SceneLayerPolygonFilter _sceneLayerPolygonFilter;
        private Graphic _cityExtentGraphic;
        private Polygon _sceneLayerExtentPolygon;

        public FilterFeaturesInScene()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            MySceneView.Scene = new Scene();

            // Create a surface with an elevation source for the scene.
            var surface = new Surface();
            surface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationSource)));
            MySceneView.Scene.BaseSurface = surface;

            // Create and set the basemap using a 3D basemap portal item.
            var basemap = new Basemap(new Uri(Basemap3D));
            MySceneView.Scene.Basemap = basemap;

            // Wait for basemap to load to find the 3D buildings layer.
            await basemap.LoadAsync();

            // Loop through all layers in the basemap and look for the buildings layer.
            foreach (var baseLayer in basemap.BaseLayers)
            {
                // Search for a layer that contains 'building' in case the name is ever updated.
                if (baseLayer.Name.Contains("building", StringComparison.OrdinalIgnoreCase))
                {
                    _3dBuildingsSceneLayer = baseLayer as ArcGISSceneLayer;
                    if (_3dBuildingsSceneLayer != null)
                    {
                        break;
                    }
                }
            }

            if (_3dBuildingsSceneLayer == null)
            {
                await new ContentDialog()
                {
                    Title = "Error",
                    Content = "Buildings layer not found in base layers. Please check that your basemap contains an ArcGIS Scene Layer with 'building' in the name.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            // Create a scene layer for the detailed buildings.
            _detailedBuildingsSceneLayer = new ArcGISSceneLayer(new Uri(DetailedBuildings));

            // Initially hide the detailed buildings so they don't clip into the 3D basemap buildings.
            _detailedBuildingsSceneLayer.IsVisible = false;

            // Add the detailed buildings layer to the scene's operational layers.
            MySceneView.Scene.OperationalLayers.Add(_detailedBuildingsSceneLayer);

            // When the detailed building scene layer finishes loading, get its extent for the polygon filter.
            await _detailedBuildingsSceneLayer.LoadAsync();

            // Construct a red polygon that shows the extent of the detailed buildings scene layer.
            Envelope cityExtent = _detailedBuildingsSceneLayer.FullExtent;
            var builder = new PolygonBuilder(MySceneView.SpatialReference);
            builder.AddPoint(cityExtent.XMin, cityExtent.YMin);
            builder.AddPoint(cityExtent.XMax, cityExtent.YMin);
            builder.AddPoint(cityExtent.XMax, cityExtent.YMax);
            builder.AddPoint(cityExtent.XMin, cityExtent.YMax);

            _sceneLayerExtentPolygon = builder.ToGeometry();

            // Create the SceneLayerPolygonFilter to later apply to the 3D buildings layer.
            _sceneLayerPolygonFilter = new SceneLayerPolygonFilter(new List<Polygon>() { _sceneLayerExtentPolygon }, SceneLayerPolygonFilterSpatialRelationship.Disjoint);

            // Create the extent graphic so we can add it later with the detailed buildings scene layer.
            var simpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 5.0f);
            var simpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Transparent, simpleLineSymbol);
            _cityExtentGraphic = new Graphic(_sceneLayerExtentPolygon, simpleFillSymbol);

            // Initially hide the graphic, since the filter has not been applied yet.
            _cityExtentGraphic.IsVisible = false;

            MySceneView.GraphicsOverlays.Add(new GraphicsOverlay());
            MySceneView.GraphicsOverlays.First().Graphics.Add(_cityExtentGraphic);

            // Set up initial button state.
            MyButton.Tag = "FilterScene";
            MyButton.Content = "Filter 3D buildings in extent";

            await MySceneView.SetViewpointCameraAsync(new Camera(new MapPoint(-122.421008, 37.702425, 207), 60, 70, 0));
        }

        // Determine which step of the sample user is on.
        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MyButton.Tag)
            {
                case "FilterScene":
                    FilterScene();
                    break;

                case "ShowDetailedBuildings":
                    ShowDetailedBuildings();
                    break;

                case "ResetScene":
                    ResetScene();
                    break;
            }
        }

        // Hide buildings within the detailed building extent so they don't clip.
        private void FilterScene()
        {
            // Update button tag and label to reflect next step.
            MyButton.Tag = "ShowDetailedBuildings";
            MyButton.Content = "Show detailed buildings";

            // Apply the polygon filter to the buildings layer.
            _3dBuildingsSceneLayer.PolygonFilter = _sceneLayerPolygonFilter;

            // Show the extent graphic to visualize the polygon filter.
            _cityExtentGraphic.IsVisible = true;
        }

        // Show the detailed buildings scene layer.
        private void ShowDetailedBuildings()
        {
            // Update button tag and label to reflect next step.
            MyButton.Tag = "ResetScene";
            MyButton.Content = "Reset scene";

            // Show the detailed buildings scene layer.
            _detailedBuildingsSceneLayer.IsVisible = true;
        }

        // Reset the scene to its original state.
        private void ResetScene()
        {
            // Update button tag and label to reflect next step.
            MyButton.Tag = "FilterScene";
            MyButton.Content = "Filter 3D buildings in extent";

            // Hide the detailed buildings layer from the scene.
            _detailedBuildingsSceneLayer.IsVisible = false;

            // Remove the polygon filter from the buildings layer.
            _3dBuildingsSceneLayer.PolygonFilter = null;

            // Hide the red extent boundary graphic.
            _cityExtentGraphic.IsVisible = false;
        }
    }
}