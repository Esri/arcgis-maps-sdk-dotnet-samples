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

namespace ArcGISRuntime.WPF.Samples.DisplayUtilityNetworkContainer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display content of utility network container",
        "Utility network",
        "A utility network container allows a dense collection of features to be represented by a single feature, which can be used to reduce map clutter.",
        "")]
    public partial class DisplayUtilityNetworkContainer
    {
        // Overlay to hold graphics for all of the associations.
        private GraphicsOverlay _associationsOverlay;

        private GraphicsOverlay _otherGraphics;

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

                    return await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
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
            _otherGraphics = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_otherGraphics);

            // Symbols for the associations.
            Symbol attachmentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Green, 5d);
            Symbol boundingBoxSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Yellow, 5d);
            Symbol connectivitySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 5d);

            // Create a renderer for the associations.
            var attachmentValue = new UniqueValue("Attachment", string.Empty, attachmentSymbol, UtilityAssociationType.Attachment.ToString());
            var boundingBoxValue = new UniqueValue("Containment", string.Empty, boundingBoxSymbol, UtilityAssociationType.Attachment.ToString());
            var connectivityValue = new UniqueValue("Connectivity", string.Empty, connectivitySymbol, UtilityAssociationType.Connectivity.ToString());
            _associationsOverlay.Renderer = new UniqueValueRenderer(new List<string> { "AssociationType" }, new List<UniqueValue> { attachmentValue, boundingBoxValue, connectivityValue }, string.Empty, null);

            // Populate the legend in the UI.
            Dictionary<UtilityAssociationType, System.Windows.Media.ImageSource> legend;
            legend = new Dictionary<UtilityAssociationType, System.Windows.Media.ImageSource>();

            RuntimeImage attachmentSwatch = await attachmentSymbol.CreateSwatchAsync();
            legend[UtilityAssociationType.Attachment] = await attachmentSwatch?.ToImageSourceAsync();

            RuntimeImage boundingSwatch = await boundingBoxSymbol.CreateSwatchAsync();
            legend[UtilityAssociationType.Containment] = await boundingSwatch?.ToImageSourceAsync();

            RuntimeImage connectSwatch = await connectivitySymbol.CreateSwatchAsync();
            legend[UtilityAssociationType.Connectivity] = await connectSwatch?.ToImageSourceAsync();

            AssociationLegend.ItemsSource = legend;

            try
            {
                // Create a new map from the web map URL (includes ArcGIS Pro subtype group layers with only container features visible).
                MyMapView.Map = new Map(new Uri("https://sampleserver7.arcgisonline.com/portal/home/item.html?id=813eda749a9444e4a9d833a4db19e1c8"));

                // The feature service url contains a utility network used to find associations shown in this sample
                Uri featureServiceURL = new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer");

                _utilityNetwork = new UtilityNetwork(featureServiceURL);
                MyMapView.Map.UtilityNetworks.Add(_utilityNetwork);

                await _utilityNetwork.LoadAsync();

                // Set the starting viewpoint.
                await MyMapView.SetViewpointAsync(InitialViewpoint);
            }
            catch (Exception ex)
            {
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

                IEnumerable<UtilityAssociation> containmentAssociations = await _utilityNetwork.GetAssociationsAsync(containerElement, UtilityAssociationType.Containment);

                IEnumerable<UtilityElement> contentElements = containmentAssociations.Select(association => association.FromElement.ObjectId == containerElement.ObjectId ? association.FromElement : association.ToElement);

                if (!contentElements.Any()) return;

                IEnumerable<ArcGISFeature> contentFeatures = await _utilityNetwork.GetFeaturesForElementsAsync(contentElements);

                // Get the content features and give them each a symbol, and add them as a graphic to the graphics overlay.
                foreach (ArcGISFeature contentFeature in contentFeatures)
                {
                    Symbol symbol = (contentFeature.FeatureTable as ArcGISFeatureTable).LayerInfo.DrawingInfo.Renderer.GetSymbol(contentFeature);
                    _otherGraphics.Graphics.Add(new Graphic(contentFeature.Geometry, symbol));
                }

                double containerViewScale = containerElement.AssetType.ContainerViewScale;
                //var boundingBox = GeometryEngine.CombineExtents(contentFeatures.Select(f => f.Geometry.Extent));

                Geometry boundingBox;
                if(_otherGraphics.Graphics.Count == 1 && _otherGraphics.Graphics.First().Geometry is MapPoint point)
                {
                    await MyMapView.SetViewpointAsync(new Viewpoint(point, containerViewScale));
                    boundingBox = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry;
                }
                else
                {
                    boundingBox = GeometryEngine.Buffer(_associationsOverlay.Extent, 0.05);
                }
                    //\ if (graphicsOverlay.getGraphics().size() == 1 && firstGraphic instanceof Point) {
                    //mapView.setViewpointCenterAsync((Point)firstGraphic, containerViewScale).addDoneListener(()-> {

                    //    // the bounding box, which defines the container view, may be computed using the extent of the features
                    //    // it contains or centered around its geometry at the container's view scale
                    //    Geometry boundingBox = mapView.getCurrentViewpoint(Viewpoint.Type.BOUNDING_GEOMETRY).getTargetGeometry();
                    //    identifyAssociationsWithExtent(boundingBox);
                    //    new Alert(Alert.AlertType.INFORMATION, "This feature has no associations").show();

                //identifyAssociationsWithExtent(boundingBox);
                //var boundingBox = new Viewpoint(_associationsOverlay.Extent.GetCenter(), containerViewScale).TargetGeometry;

                // Add a graphic for the association.
                Graphic boundingGraphic = new Graphic(boundingBox);
                // boundingGraphic.Attributes["GlobalId"] = boundingBox;
                boundingGraphic.Attributes["AssociationType"] = UtilityAssociationType.Containment.ToString();
                _associationsOverlay.Graphics.Add(boundingGraphic);
                var allAssociations = await _utilityNetwork.GetAssociationsAsync(boundingBox.Extent);
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
                    graphic.Attributes["AssociationType"] = association.AssociationType.ToString();
                    _associationsOverlay.Graphics.Add(graphic);
                }

                await MyMapView.SetViewpointAsync(new Viewpoint(boundingBox));
            }
            catch (Exception ex)
            {
            }
        }
    }
}