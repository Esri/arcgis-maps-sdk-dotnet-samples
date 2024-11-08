// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayUtilityNetworkContainer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display content of utility network container",
        category: "Utility network",
        description: "A utility network container allows a dense collection of features to be represented by a single feature, which can be used to reduce map clutter.",
        instructions: "Tap on a container feature to show all features inside the container. The container is shown as a polygon graphic with the content features contained within. The viewpoint and scale of the map are also changed to the container's extent. Connectivity and attachment associations inside the container are shown as red and blue dotted lines respectively.",
        tags: new[] { "associations", "connectivity association", "containment association", "structural attachment associations", "utility network" })]
    public partial class DisplayUtilityNetworkContainer
    {
        // Overlay to hold graphics for all of the associations.
        private GraphicsOverlay _associationsOverlay;

        // This viewpoint shows several associations clearly in the utility network.
        private readonly Viewpoint InitialViewpoint = new Viewpoint(41.801504, -88.163718, 4e3);

        // Utility network that will be created from the feature server.
        private UtilityNetwork _utilityNetwork;

        public DisplayUtilityNetworkContainer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // As of ArcGIS Enterprise 10.8.1, using utility network functionality requires a licensed user. The following login for the sample server is licensed to perform utility network operations.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "viewer01";
                    string sampleServer7Pass = "I68VGU^nMurF";

                    return await AccessTokenCredential.CreateAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            // Create a graphics overlay for associations.
            _associationsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_associationsOverlay);

            // Symbols for the associations.
            Symbol attachmentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Blue, 5d);
            Symbol boundingBoxSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Yellow, 5d);
            Symbol connectivitySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 5d);

            // Create a renderer for the associations.
            var attachmentValue = new UniqueValue("Attachment", string.Empty, attachmentSymbol, UtilityAssociationType.Attachment.ToString());
            var boundingBoxValue = new UniqueValue("Containment", string.Empty, boundingBoxSymbol, UtilityAssociationType.Containment.ToString());
            var connectivityValue = new UniqueValue("Connectivity", string.Empty, connectivitySymbol, UtilityAssociationType.Connectivity.ToString());
            _associationsOverlay.Renderer = new UniqueValueRenderer(new List<string> { "AssociationType" }, new List<UniqueValue> { attachmentValue, boundingBoxValue, connectivityValue }, string.Empty, null);

            // Populate the legend in the UI.
            var symbologyKey = new Dictionary<UtilityAssociationType, System.Windows.Media.ImageSource>();

            RuntimeImage attachmentSwatch = await attachmentSymbol.CreateSwatchAsync();
            symbologyKey[UtilityAssociationType.Attachment] = await attachmentSwatch?.ToImageSourceAsync();

            RuntimeImage boundingSwatch = await boundingBoxSymbol.CreateSwatchAsync();
            symbologyKey[UtilityAssociationType.Containment] = await boundingSwatch?.ToImageSourceAsync();

            RuntimeImage connectSwatch = await connectivitySymbol.CreateSwatchAsync();
            symbologyKey[UtilityAssociationType.Connectivity] = await connectSwatch?.ToImageSourceAsync();

            AssociationLegend.ItemsSource = symbologyKey;

            try
            {
                // Create a new map from the web map URL (includes ArcGIS Pro subtype group layers with only container features visible).
                MyMapView.Map = new Map(new Uri("https://sampleserver7.arcgisonline.com/portal/home/item.html?id=813eda749a9444e4a9d833a4db19e1c8"));

                // The feature service url contains a utility network used to find associations shown in this sample.
                Uri featureServiceURL = new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer");

                _utilityNetwork = new UtilityNetwork(featureServiceURL);
                MyMapView.Map.UtilityNetworks.Add(_utilityNetwork);

                await _utilityNetwork.LoadAsync();

                // Set the starting viewpoint.
                await MyMapView.SetViewpointAsync(InitialViewpoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Message.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            _ = IdentifyContainer(e.Position);
        }

        private async Task IdentifyContainer(System.Windows.Point position)
        {
            try
            {
                // Identify the feature to be used from the sublayer.
                IEnumerable<IdentifyLayerResult> identifyResult = await MyMapView.IdentifyLayersAsync(position, 10.0, false);
                ArcGISFeature feature = identifyResult?.FirstOrDefault()?.SublayerResults.FirstOrDefault()?.GeoElements?.FirstOrDefault() as ArcGISFeature;
                if (feature == null) { return; }

                // Create element from the identified feature.
                UtilityElement containerElement = _utilityNetwork.CreateElement(feature);

                // Get all of the containment associations for the selected container element.
                IEnumerable<UtilityAssociation> containmentAssociations = await _utilityNetwork.GetAssociationsAsync(containerElement, UtilityAssociationType.Containment);

                // Get all of the other elements associated with the selected element.
                IEnumerable<UtilityElement> contentElements = containmentAssociations.Select(association => association.FromElement.ObjectId == containerElement.ObjectId ? association.ToElement : association.FromElement);

                // Verify that there are elements in the associations.
                if (!contentElements.Any()) return;

                // Get features for these elements.
                IEnumerable<ArcGISFeature> contentFeatures = await _utilityNetwork.GetFeaturesForElementsAsync(contentElements);

                // Clear the graphics overlay.
                _associationsOverlay.Graphics.Clear();

                // Enable the close button.
                CloseButton.Visibility = Visibility.Visible;

                // Get the content features and give them each a symbol, and add them as a graphic to the graphics overlay.
                foreach (ArcGISFeature contentFeature in contentFeatures)
                {
                    // Get the symbol for each element from the feature table.
                    Symbol symbol = (contentFeature.FeatureTable as ArcGISFeatureTable).LayerInfo.DrawingInfo.Renderer.GetSymbol(contentFeature);
                    _associationsOverlay.Graphics.Add(new Graphic(contentFeature.Geometry, symbol));
                }

                // Create a bounding box for the container.
                Geometry boundingBox;

                // If there is only single element, create a bounding box using the container view scale.
                if (contentFeatures.Count() == 1 && contentFeatures.First().Geometry is MapPoint point)
                {
                    double containerViewScale = containerElement.AssetType.ContainerViewScale;
                    boundingBox = new Envelope(point, containerViewScale, containerViewScale);
                }
                else
                {
                    // Create a bounding box using the combined extents of the elements from associations.
                    Envelope combinedExtents = GeometryEngine.CombineExtents(contentFeatures.Select(f => f.Geometry));
                    boundingBox = GeometryEngine.Buffer(combinedExtents, 0.05);
                }

                // Add a graphic for the bounding box.
                Graphic boundingGraphic = new Graphic(boundingBox);
                boundingGraphic.Attributes["AssociationType"] = UtilityAssociationType.Containment.ToString();
                _associationsOverlay.Graphics.Add(boundingGraphic);

                // Add graphics for every association.
                IEnumerable<UtilityAssociation> allAssociations = await _utilityNetwork.GetAssociationsAsync(boundingBox.Extent);
                foreach (UtilityAssociation association in allAssociations)
                {
                    // Check if the graphics overlay already contains the association.
                    if (_associationsOverlay.Graphics.Any(g => g.Attributes.ContainsKey("GlobalId") && (Guid)g.Attributes["GlobalId"] == association.GlobalId) || association.Geometry == null)
                    {
                        continue;
                    }

                    // Add a graphic for the association.
                    Graphic graphic = new Graphic(association.Geometry);
                    graphic.Attributes["GlobalId"] = association.GlobalId;

                    // This association type will be used by the unique value renderer to determine the symbology of the graphic.
                    graphic.Attributes["AssociationType"] = association.AssociationType.ToString();
                    _associationsOverlay.Graphics.Add(graphic);
                }

                // Set the viewpoint to show the bounding box.
                await MyMapView.SetViewpointGeometryAsync(boundingBox, 25);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Message.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseButton.Visibility = Visibility.Hidden;

            // Clear the graphics overlay.
            _associationsOverlay.Graphics.Clear();

            // Return to the initial viewpoint.
            MyMapView.SetViewpointAsync(InitialViewpoint);
        }
    }
}