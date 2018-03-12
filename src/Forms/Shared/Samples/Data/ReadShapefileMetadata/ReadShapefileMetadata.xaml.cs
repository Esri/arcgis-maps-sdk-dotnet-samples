// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System.IO;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ReadShapefileMetadata
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read shapefile metadata",
        "Data",
        "This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.",
        "The shapefile will be downloaded from an ArcGIS Online portal automatically.",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    public partial class ReadShapefileMetadata : ContentPage
    {
        public ReadShapefileMetadata()
        {
            InitializeComponent();

            Title = "Read shapefile metadata";

            // Open a shapefile stored locally and add it to the map as a feature layer
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            Map streetMap = new Map(Basemap.CreateStreets());

            // Get the path to the downloaded shapefile
            string filepath = GetShapefilePath();

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Read metadata about the shapefile and display it in the UI
            ShapefileInfo fileInfo = myShapefile.Info;
            InfoPanel.BindingContext = fileInfo;

            // Read the thumbnail image data into a byte array
            Stream imageStream = await fileInfo.Thumbnail.GetEncodedBufferAsync();
            byte[] imageData = new byte[imageStream.Length];
            imageStream.Read(imageData, 0, imageData.Length);

            // Create a new image source from the thumbnail data
            ImageSource streamImageSource = ImageSource.FromStream(() => new MemoryStream(imageData));

            // Create a new image to display the thumbnail
            var image = new Image()
            {
                Source = streamImageSource,
                Margin = new Thickness(10)
            };

            // Show the thumbnail image in a UI control
            ShapefileThumbnailImage.Source = image.Source;

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);
            await newFeatureLayer.LoadAsync();

            // Zoom the map to the extent of the shapefile
            MyMapView.SpatialReferenceChanged += async (s, e) =>
            {
                await MyMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
            };

            // Add the feature layer to the map
            streetMap.OperationalLayers.Add(newFeatureLayer);

            // Show the map in the MapView
            MyMapView.Map = streetMap;
        }

        private void ShowMetadataClicked(object sender, System.EventArgs e)
        {
            // Toggle the visibility of the metadata panel
            MetadataFrame.IsVisible = !MetadataFrame.IsVisible;
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");
        }
    }
}