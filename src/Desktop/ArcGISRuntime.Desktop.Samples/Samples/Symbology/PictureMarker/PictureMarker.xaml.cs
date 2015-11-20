//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.Desktop.Samples.PictureMarker
{
    public partial class PictureMarker
    {
        public PictureMarker()
        {
            InitializeComponent();

            // Handle the spatial reference changed event to add graphics with a picture marker symbol
            MyMapView.SpatialReferenceChanged += AddPictureMarkerGraphics;
        }

        private async void AddPictureMarkerGraphics(object sender, System.EventArgs e)
        {
            // Create a new picture marker symbol using an online image resource
            var campsitePictureSym = new PictureMarkerSymbol(new System.Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae"));
            await campsitePictureSym.LoadAsync();

            // TODO: Create a new picture marker symbol from a local file
            //var bluePinPictureSym = new PictureMarkerSymbol()

            // TODO: Create a new picture marker symbol from a resource in the app
            //var redPinPictureSym = new PictureMarkerSymbol()

            // Create points to display on the map
            var site1 = new MapPoint(-223560, 6552021, SpatialReferences.WebMercator);
            var site2 = new MapPoint(-226773, 6550477, SpatialReferences.WebMercator);
            var site3 = new MapPoint(-228835, 6550763, SpatialReferences.WebMercator);

            // Create new graphics using the map points and picture marker symbol
            var graphic1 = new Graphic(site1, campsitePictureSym);
            var graphic2 = new Graphic(site2, campsitePictureSym); // bluePinPictureSym); --TODO
            var graphic3 = new Graphic(site3, campsitePictureSym); // redPinPictureSym); --TODO

            // Create a new graphics overlay
            var myGraphicsOverlay = new GraphicsOverlay();

            // Add graphics to the graphics overlay
            myGraphicsOverlay.Graphics.Add(graphic1);
            myGraphicsOverlay.Graphics.Add(graphic2);
            myGraphicsOverlay.Graphics.Add(graphic3);

            // Add the new graphics overlay to the map view
            MyMapView.GraphicsOverlays.Add(myGraphicsOverlay);

            // Zoom to the extent of the new graphics
            var extent = new Envelope(new MapPoint(-228835, 6550763, SpatialReferences.WebMercator), new MapPoint(-223560, 6552021, SpatialReferences.WebMercator));
            await MyMapView.SetViewpointGeometryAsync(extent, 100.0);
        }
    }
}