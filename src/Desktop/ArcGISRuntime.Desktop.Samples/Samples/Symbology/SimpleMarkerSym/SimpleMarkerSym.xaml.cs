// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Media;

namespace ArcGISRuntime.Desktop.Samples.SimpleMarkerSym
{
    public partial class SimpleMarkerSym
    {
        public SimpleMarkerSym()
        {
            InitializeComponent();
            AddPointGraphic();
        }

        private void AddPointGraphic()
        {
            // Create a new point to display on the map (Esri Redlands)
            var esriRedlandsPoint = new MapPoint(-117.195646, 34.056397, SpatialReferences.Wgs84);

            // Create a new red circle marker symbol
            var circleSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 14);

            // Create a new graphic using the map point and the symbol
            var pointGraphic = new Graphic(esriRedlandsPoint, circleSymbol);

            // Add the graphic to the map
            MyGraphicsOverlay.Graphics.Add(pointGraphic);

            // Center the map view extent on the point graphic
            var mapCenterPoint = new Viewpoint(esriRedlandsPoint, 24000);
            MyMapView.SetViewpoint(mapCenterPoint);
        }
    }
}