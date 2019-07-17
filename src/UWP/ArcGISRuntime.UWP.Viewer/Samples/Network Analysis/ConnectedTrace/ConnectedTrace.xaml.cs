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
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.WPF.Samples.ConnectedTrace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Connected Trace",
        "Network Analysis",
        "Find all features connected to a given set of starting point(s) and barrier(s) in your network using the Connected trace type.",
        "")]
    public partial class ConnectedTrace
    {

        private readonly int[] LayerIds = new int[] { 4, 3, 5, 0 };

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private CancellationTokenSource _selectTerminalTokenSource;

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public ConnectedTrace()
        {
            InitializeComponent();

            // TODO: Delete once SS6 is used.
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.ChallengeHandler = new Esri.ArcGISRuntime.Security.ChallengeHandler(async (info) =>
            {
                return await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.GenerateCredentialAsync(new Uri(PortalUrl), Username, Password);
            });

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
                    InitialViewpoint = new Viewpoint(new Envelope(-9813547.35557238, 5129980.36635111, -9813185.0602376, 5130215.41254146, SpatialReferences.WebMercator))
                };

                foreach (int id in LayerIds)
                {
                    MyMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{id}")));
                }

                // Create and load the UtilityNetwork.
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
                await new MessageDialog(ex.Message, ex.Message.GetType().Name).ShowAsync();
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MyMapView.GeoViewTapped -= OnGeoViewTapped;
            try
            {
                e.Handled = true;

                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Identifying trace locations...";

                // This exception is used when a feature in the utility network can't be identified.
                Exception identifyException = new Exception("Could not identify location. Click on the network lines or points to add a utility element.");

                // Set whether the user is adding a starting point or a barrier.
                bool isAddingStart = IsAddingStartingLocations.IsChecked.Value;

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 10.0, false);

                // Check that a results from a layer were identified from the user input.
                if (!identifyResult.Any()) { throw identifyException; }

                // Identify the selected feature.
                IdentifyLayerResult layerResult = identifyResult?.FirstOrDefault();
                ArcGISFeature feature = layerResult?.GeoElements?.FirstOrDefault() as ArcGISFeature;

                // Check that a feature was identified from the layer.
                if (feature == null) { throw identifyException; }

                // Create element with `terminal` for junction feature or with `fractionAlong` for edge feature.
                UtilityElement element = null;

                // Select default terminal or display possible terminals for the junction feature.
                UtilityNetworkSource networkSource = _utilityNetwork.Definition.GetNetworkSource(feature.FeatureTable.TableName);

                // Check if the network source is a junction or an edge.
                if (networkSource.SourceType == UtilitySourceType.Junction)
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
                else if (networkSource.SourceType == UtilitySourceType.Edge)
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
                if (element == null) { throw identifyException; }

                // Build the utility trace parameters.
                bool createWithStartingLocation = (_parameters == null) && isAddingStart;
                if (_parameters == null)
                {
                    IEnumerable<UtilityElement> startingLocations = createWithStartingLocation ? new[] { element } : Enumerable.Empty<UtilityElement>();
                    _parameters = new UtilityTraceParameters(UtilityTraceType.Connected, startingLocations);
                }
                else if (isAddingStart)
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
                await new MessageDialog(ex.Message, "Error").ShowAsync();
            }
            finally
            {
                MyMapView.GeoViewTapped += OnGeoViewTapped;
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Start the UI for the user choosing the junction.
                TerminalPicker.Visibility = Visibility.Visible;
                MainUI.Visibility = Visibility.Collapsed;

                // Load the terminals into the UI.
                Picker.DataContext = terminals;
                Picker.SelectedIndex = 0;

                // Wait for the user to make a selection.
                _selectTerminalTokenSource = new CancellationTokenSource();
                await Task.Delay(-1, _selectTerminalTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Make the main UI visible again.
                TerminalPicker.Visibility = Visibility.Collapsed;
                MainUI.Visibility = Visibility.Visible;

                return Picker?.SelectedItem as UtilityTerminal;
            }
            throw new Exception("Terminal not selected.");
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            _selectTerminalTokenSource.Cancel();
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
                await new MessageDialog(ex.Message, "Error").ShowAsync();
            }
            finally
            {
                IsBusy.Visibility = Visibility.Collapsed;
            }
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            // Reset the UI.
            Status.Text = "Click on the network lines or points to add a utility element.";
            IsBusy.Visibility = Visibility.Collapsed;

            // Clear the utility trace parameters.
            _parameters = null;

            // Clear the map layers and graphics.
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }
    }
}