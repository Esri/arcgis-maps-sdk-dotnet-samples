// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.FindFeaturesUtilityNetwork
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find connected features in utility networks",
        "Network Analysis",
        "Find all features connected to a given set of starting point(s) and barrier(s) in your network using the Connected trace type.",
        "")]
    public partial class FindFeaturesUtilityNetwork
    {
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private TaskCompletionSource<UtilityTerminal> _terminalCompletionSource = null;

        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9813547.35557238, 5129980.36635111, -9813185.0602376, 5130215.41254146, SpatialReferences.WebMercator));

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public FindFeaturesUtilityNetwork()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Loading Utility Network...";

                // Setup Map with Feature Layer(s) that contain Utility Network.
                MyMapView.Map = new Map(Basemap.CreateStreetsNightVector())
                {
                    InitialViewpoint = _startingViewpoint
                };

                // Add the layer with electric distribution lines.
                FeatureLayer lineLayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/115"));
                lineLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 3));
                MyMapView.Map.OperationalLayers.Add(lineLayer);

                // Add the layer with electric devices.
                FeatureLayer electricDevicelayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/100"));
                MyMapView.Map.OperationalLayers.Add(electricDevicelayer);

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), MyMapView.Map);

                Status.Text = "Click on the network lines or points to add a utility element.";

                // Create symbols for starting points and barriers.
                _startingPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Green, 20d);
                _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 20d);

                // Create a graphics overlay.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(graphicsOverlay);
            }
            catch (Exception ex)
            {
                Status.Text = "Loading Utility Network failed...";
                MessageBox.Show(ex.Message, ex.Message.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Hidden;
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                e.Handled = true;

                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Identifying trace locations...";

                // Set whether the user is adding a starting point or a barrier.
                bool isAddingStart = IsAddingStartingLocations.IsChecked.Value;

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 10.0, false);

                // Check that a results from a layer were identified from the user input.
                if (!identifyResult.Any()) { return; }

                // Identify the selected feature.
                IdentifyLayerResult layerResult = identifyResult?.FirstOrDefault();
                ArcGISFeature feature = layerResult?.GeoElements?.FirstOrDefault() as ArcGISFeature;

                // Check that a feature was identified from the layer.
                if (feature == null) { return; }

                // Create element with `terminal` for junction feature or with `fractionAlong` for edge feature.
                UtilityElement element = null;

                // Select default terminal or display possible terminals for the junction feature.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(feature.FeatureTable.TableName);

                // Check if the network source is a junction or an edge.
                if (networkSource.SourceType == UtilityNetworkSourceType.Junction)
                {
                    // Get the UtilityAssetGroup from the feature.
                    string assetGroupFieldName = ((ArcGISFeatureTable)feature.FeatureTable).SubtypeField ?? "ASSETGROUP";
                    int assetGroupCode = Convert.ToInt32(feature.Attributes[assetGroupFieldName]);
                    UtilityAssetGroup assetGroup = networkSource?.AssetGroups?.FirstOrDefault(g => g.Code == assetGroupCode);

                    // Get the UtilityAssetType from the feature.
                    int assetTypeCode = Convert.ToInt32(feature.Attributes["ASSETTYPE"]);
                    UtilityAssetType assetType = assetGroup?.AssetTypes?.FirstOrDefault(t => t.Code == assetTypeCode);

                    // Get the list of terminals for the feature.
                    IEnumerable<UtilityTerminal> terminals = assetType?.TerminalConfiguration?.Terminals;

                    // If there is more than one terminal, prompt the user to select a terminal.
                    if (terminals.Count() > 1)
                    {
                        // Ask the user to choose the terminal.
                        UtilityTerminal terminal = await WaitForTerminal(terminals);

                        // Create a UtilityElement with the terminal.
                        element = _utilityNetwork.CreateElement(feature, terminal);
                        Status.Text = $"terminal: `{terminal?.Name ?? "default"}`";
                    }
                    else
                    {
                        element = _utilityNetwork.CreateElement(feature, terminals.FirstOrDefault());
                        Status.Text = $"terminal: `{element.Terminal?.Name ?? "default"}`";
                    }
                }
                else if (networkSource.SourceType == UtilityNetworkSourceType.Edge)
                {
                    element = _utilityNetwork.CreateElement(feature);

                    // Compute how far tapped location is along the edge feature.
                    if (feature.Geometry is Polyline line)
                    {
                        line = GeometryEngine.RemoveZ(line) as Polyline;

                        // Set how far the element is along the edge.
                        element.FractionAlongEdge = GeometryEngine.FractionAlong(line, e.Location, -1);

                        Status.Text = $"fractionAlongEdge: `{element.FractionAlongEdge}`";
                    }
                }

                // Check that the element can be added to the parameters.
                if (element == null) { return; }

                // Build the utility trace parameters.
                if (_parameters == null)
                {
                    IEnumerable<UtilityElement> startingLocations = Enumerable.Empty<UtilityElement>();
                    _parameters = new UtilityTraceParameters(UtilityTraceType.Connected, startingLocations);
                }
                if (isAddingStart)
                {
                    _parameters.StartingLocations.Add(element);
                }
                else
                {
                    _parameters.Barriers.Add(element);
                }

                // Add a graphic for the new utility element.
                Graphic traceLocationGraphic = new Graphic(feature.Geometry as MapPoint ?? e.Location, isAddingStart ? _startingPointSymbol : _barrierPointSymbol);
                MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Add(traceLocationGraphic);
            }
            catch (Exception ex)
            {
                Status.Text = "Identifying locations failed...";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (Status.Text.Equals("Identifying trace locations...")) { Status.Text = "Could not identify location."; }
                IsBusy.Visibility = Visibility.Hidden;
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Switch the UI for the user choosing the junction.
                TerminalPicker.Visibility = Visibility.Visible;
                MainUI.Visibility = Visibility.Collapsed;
                MyMapView.GeoViewTapped -= OnGeoViewTapped;

                // Load the terminals into the UI.
                Picker.DataContext = terminals;
                Picker.SelectedIndex = 0;

                // Wait for the user to select a terminal.
                _terminalCompletionSource = new TaskCompletionSource<UtilityTerminal>();
                return await _terminalCompletionSource.Task;
            }
            finally
            {
                // Enable the main UI again.
                TerminalPicker.Visibility = Visibility.Collapsed;
                MainUI.Visibility = Visibility.Visible;
                MyMapView.GeoViewTapped += OnGeoViewTapped;
            }
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(Picker.SelectedItem as UtilityTerminal);
        }

        private async void OnTrace(object sender, RoutedEventArgs e)
        {
            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Running connected trace...";

                // Verify that the parameters contain a starting location.
                if (_parameters == null || !_parameters.StartingLocations.Any()) { throw new Exception("No starting locations set."); }

                //  Get the trace result from the utility network.
                IEnumerable<UtilityTraceResult> traceResult = await _utilityNetwork.TraceAsync(_parameters);
                UtilityElementTraceResult elementTraceResult = traceResult?.FirstOrDefault() as UtilityElementTraceResult;

                if (elementTraceResult?.Elements?.Count > 0)
                {
                    // Clear previous selection from the layer.
                    MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                    // Group the utility elements by their network source.
                    IEnumerable<IGrouping<string, UtilityElement>> groupedElementsResult = from element in elementTraceResult.Elements
                                                                                           group element by element.NetworkSource.Name into groupedElements
                                                                                           select groupedElements;

                    foreach (IGrouping<string, UtilityElement> elementGroup in groupedElementsResult)
                    {
                        // Get the layer for the utility element.
                        FeatureLayer layer = (FeatureLayer)MyMapView.Map.OperationalLayers.FirstOrDefault(l => l is FeatureLayer && ((FeatureLayer)l).FeatureTable.TableName == elementGroup.Key);
                        if (layer == null)
                            continue;

                        // Convert elements to features to highlight result.
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elementGroup);
                        layer.SelectFeatures(features);
                    }
                }
                Status.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                Status.Text = "Trace failed...";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy.Visibility = Visibility.Hidden;
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            // Reset the UI.
            Status.Text = "Click on the network lines or points to add a utility element.";
            IsBusy.Visibility = Visibility.Hidden;

            // Clear the utility trace parameters.
            _parameters = null;

            // Clear the map layers and graphics.
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }
    }
}