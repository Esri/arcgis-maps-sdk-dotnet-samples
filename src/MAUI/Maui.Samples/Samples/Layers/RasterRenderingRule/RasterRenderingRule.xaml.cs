// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;

namespace ArcGIS.Samples.RasterRenderingRule
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Raster rendering rule",
        category: "Layers",
        description: "Display a raster on a map and apply different rendering rules to that raster.",
        instructions: "Run the sample and use the drop-down menu at the top to select a rendering rule.",
        tags: new[] { "raster", "rendering rules", "visualization" })]
    public partial class RasterRenderingRule : ContentPage
    {
        public RasterRenderingRule()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        // Create an empty read-only list for the various rendering rules of the image service raster
        private IReadOnlyList<RenderingRuleInfo> _myReadOnlyListRenderRuleInfos;

        // Create a Uri for the image server
        private Uri _myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer");

        // Create a list to store the names of the rendering rule info for the image service raster
        private List<string> _names = new List<string>();

        private async Task Initialize()
        {
            // Assign a new map to the MapView
            MyMapView.Map = new Map
            {
                // Set the basemap to Streets
                Basemap = new Basemap(BasemapStyle.ArcGISStreets)
            };

            // Create a new image service raster from the Uri
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(_myUri);

            try
            {
                // Load the image service raster
                await myImageServiceRaster.LoadAsync();

                // Get the ArcGIS image service info (metadata) from the image service raster
                ArcGISImageServiceInfo myArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo;

                // Get the full extent envelope of the image service raster (the Charlotte, NC area)
                Envelope myEnvelope = myArcGISImageServiceInfo.FullExtent;

                // Define a new view point from the full extent envelope
                Viewpoint myViewPoint = new Viewpoint(myEnvelope);

                // Zoom to the area of the full extent envelope of the image service raster
                await MyMapView.SetViewpointAsync(myViewPoint);

                // Get the rendering rule info (i.e. definitions of how the image should be drawn) info from the image service raster
                _myReadOnlyListRenderRuleInfos = myArcGISImageServiceInfo.RenderingRuleInfos;

                // Loop through each rendering rule info
                foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
                {
                    // Get the name of the rendering rule info
                    string myRenderingRuleName = myRenderingRuleInfo.Name;

                    // Add the name of the rendering rule info into the list of names
                    _names.Add(myRenderingRuleName);
                }

                // Call the function to display the image service raster based up on user choice of rendering rules
                await ChangeRenderingRuleAsync();
            }
            catch (Exception e)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private async Task ChangeRenderingRuleAsync()
        {
            try
            {
                // Display a picker to the user to choose among the available rendering rules for the image service raster
                string myRenderingRuleInfoName = await Application.Current.Windows[0].Page.DisplayActionSheet("Select a Rendering Rule", "Cancel", null, _names.ToArray());

                // Loop through each rendering rule info in the image service raster
                foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
                {
                    // Get the name of the rendering rule info
                    string myRenderingRuleName = myRenderingRuleInfo.Name;

                    // If the name of the rendering rule info matches what was chosen by the user, proceed
                    if (myRenderingRuleName == myRenderingRuleInfoName)
                    {
                        // Create a new rendering rule from the rendering rule info
                        RenderingRule myRenderingRule = new RenderingRule(myRenderingRuleInfo);

                        // Create a new image service raster
                        ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(_myUri)
                        {
                            // Set the image service raster's rendering rule to the rendering rule created earlier
                            RenderingRule = myRenderingRule
                        };

                        // Create a new raster layer from the image service raster
                        RasterLayer myRasterLayer = new RasterLayer(myImageServiceRaster);

                        // Add the raster layer to the operational layers of the  map view
                        MyMapView.Map.OperationalLayers.Add(myRasterLayer);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void OnChangeRenderingRuleButtonClicked(object sender, EventArgs e)
        {
            // Call the function to display the image service raster based up on user choice of rendering rules
            _ = ChangeRenderingRuleAsync();
        }
    }
}