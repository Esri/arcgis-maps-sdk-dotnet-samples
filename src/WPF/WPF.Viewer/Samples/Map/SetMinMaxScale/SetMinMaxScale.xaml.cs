// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.WPF.Samples.SetMinMaxScale
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Set min & max scale",
        category: "Map",
        description: "Restrict zooming between specific scale ranges.",
        instructions: "Zoom in and out of the map. The zoom extents of the map are limited between the given minimum and maximum scales.",
        tags: new[] { "area of interest", "level of detail", "maximum", "minimum", "scale", "viewpoint" })]
    public partial class SetMinMaxScale
    {
        public SetMinMaxScale()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with Streets basemap
            Map myMap = new Map(BasemapStyle.ArcGISStreets)
            {
                // Set the scale at which this layer can be viewed
                // MinScale defines how far 'out' you can zoom where
                // MaxScale defines how far 'in' you can zoom.
                MinScale = 8000,
                MaxScale = 2000
            };

            // Create central point where map is centered
            MapPoint centralPoint = new MapPoint(-355453, 7548720, SpatialReferences.WebMercator);

            // Create starting viewpoint
            Viewpoint startingViewpoint = new Viewpoint(
                centralPoint,
                3000);
            // Set starting viewpoint
            myMap.InitialViewpoint = startingViewpoint;

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}