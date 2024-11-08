// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.TraceUtilityNetwork
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Trace utility network",
        category: "Utility network",
        description: "Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.",
        instructions: "Tap on one or more features while 'Add starting locations' or 'Add barriers' is selected. When a junction feature is identified, you may be prompted to select a terminal. When an edge feature is identified, the distance from the tapped location to the beginning of the edge feature will be computed. Select the type of trace using the drop down menu. Click 'Trace' to initiate a trace on the network. Click 'Reset' to clear the trace parameters and start over.",
        tags: new[] { "condition barriers", "downstream trace", "network analysis", "subnetwork trace", "toolkit", "trace configuration", "traversability", "upstream trace", "utility network", "validate consistency" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class TraceUtilityNetwork
    {
        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        private const int LineLayerId = 3;
        private const int DeviceLayerId = 0;

        // Viewpoint in the utility network area.
        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9812980.8041217551, 5128523.87694709, -9812798.4363710005, 5128627.6261982173, SpatialReferences.WebMercator));

        // Utility network objects.
        private UtilityNetwork _utilityNetwork;
        private List<UtilityElement> _startingLocations = new List<UtilityElement>();
        private List<UtilityElement> _barriers = new List<UtilityElement>();
        private UtilityTier _mediumVoltageTier;

        // Task completion source for the user selected terminal.
        private TaskCompletionSource<UtilityTerminal> _terminalCompletionSource = null;

        // Markers for the utility elements.
        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public TraceUtilityNetwork()
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

            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Loading Utility Network...";

                // Create a map.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreetsNight)
                {
                    InitialViewpoint = _startingViewpoint
                };

                // Create and load a service geodatabase that matches utility network.
                ServiceGeodatabase serviceGeodatabase = await ServiceGeodatabase.CreateAsync(new Uri(FeatureServiceUrl));

                // Add the layer with electric distribution lines.
                FeatureLayer lineLayer = new FeatureLayer(serviceGeodatabase.GetTable(LineLayerId));
                UniqueValue mediumVoltageValue = new UniqueValue("N/A", "Medium Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 3), 5);
                UniqueValue lowVoltageValue = new UniqueValue("N/A", "Low Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.DarkCyan, 3), 3);
                lineLayer.Renderer = new UniqueValueRenderer(new List<string>() { "ASSETGROUP" }, new List<UniqueValue>() { mediumVoltageValue, lowVoltageValue }, "", new SimpleLineSymbol());
                MyMapView.Map.OperationalLayers.Add(lineLayer);

                // Add the layer with electric devices.
                FeatureLayer electricDevicelayer = new FeatureLayer(serviceGeodatabase.GetTable(DeviceLayerId));
                MyMapView.Map.OperationalLayers.Add(electricDevicelayer);

                // Set the selection color for features in the map view.
                MyMapView.SelectionProperties = new SelectionProperties(System.Drawing.Color.Yellow);

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), MyMapView.Map);

                // Update the trace configuration UI.
                TraceTypes.ItemsSource = new List<UtilityTraceType>() { UtilityTraceType.Connected, UtilityTraceType.Subnetwork, UtilityTraceType.Upstream, UtilityTraceType.Downstream };
                TraceTypes.SelectedIndex = 0;

                // Get the utility tier used for traces in this network. For this data set, the "Medium Voltage Radial" tier from the "ElectricDistribution" domain network is used.
                UtilityDomainNetwork domainNetwork = _utilityNetwork.Definition.GetDomainNetwork("ElectricDistribution");
                _mediumVoltageTier = domainNetwork.GetTier("Medium Voltage Radial");

                // More complex datasets may require using utility trace configurations from different tiers. The following LINQ expression gets all tiers present in the utility network.
                //IEnumerable<UtilityTier> tiers = _utilityNetwork.Definition.DomainNetworks.Select(domain => domain.Tiers).SelectMany(tier => tier);

                // Create symbols for starting locations and barriers.
                _startingPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.LightGreen, 25d);
                _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.OrangeRed, 25d);

                // Create a graphics overlay.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(graphicsOverlay);

                // Set the instruction text.
                Status.Text = "Click on the network lines or points to add a utility element.";
            }
            catch (Exception ex)
            {
                Status.Text = "Loading Utility Network failed...";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                MainUI.IsEnabled = true;
                IsBusy.Visibility = Visibility.Hidden;
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = "Identifying trace locations...";

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await MyMapView.IdentifyLayersAsync(e.Position, 10.0, false);
                ArcGISFeature feature = identifyResult?.FirstOrDefault()?.GeoElements?.FirstOrDefault() as ArcGISFeature;
                if (feature == null) { return; }

                // Create element from the identified feature.
                UtilityElement element = _utilityNetwork.CreateElement(feature);

                if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Junction)
                {
                    // Select terminal for junction feature.
                    IEnumerable<UtilityTerminal> terminals = element.AssetType.TerminalConfiguration?.Terminals;
                    if (terminals?.Count() > 1)
                    {
                        element.Terminal = await GetTerminalAsync(terminals);
                    }
                    Status.Text = $"Terminal: {element.Terminal?.Name ?? "default"}";
                }
                else if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Edge)
                {
                    // Compute how far tapped location is along the edge feature.
                    if (feature.Geometry is Polyline line)
                    {
                        line = line.RemoveZ() as Polyline;
                        double fraction = line.FractionAlong(e.Location, -1);
                        if (double.IsNaN(fraction)) { return; }
                        element.FractionAlongEdge = fraction;
                        Status.Text = $"Fraction along edge: {element.FractionAlongEdge}";
                    }
                }

                // Check whether starting location or barrier is added to update the right collection and symbology.
                Symbol symbol = null;
                if (IsAddingStartingLocations.IsChecked.Value == true)
                {
                    _startingLocations.Add(element);
                    symbol = _startingPointSymbol;
                }
                else
                {
                    _barriers.Add(element);
                    symbol = _barrierPointSymbol;
                }

                // Add a graphic for the new utility element.
                Graphic traceLocationGraphic = new Graphic(feature.Geometry as MapPoint ?? e.Location, symbol);
                MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Add(traceLocationGraphic);
            }
            catch (Exception ex)
            {
                Status.Text = "Identifying locations failed.";
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (Status.Text.Equals("Identifying trace locations...")) { Status.Text = "Could not identify location."; }
                IsBusy.Visibility = Visibility.Hidden;
            }
        }

        private async Task<UtilityTerminal> GetTerminalAsync(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                MyMapView.GeoViewTapped -= OnGeoViewTapped;
                TerminalPicker.Visibility = Visibility.Visible;
                MainUI.Visibility = Visibility.Collapsed;
                Picker.ItemsSource = terminals;
                Picker.SelectedIndex = 1;

                // Waits for user to select a terminal.
                _terminalCompletionSource = new TaskCompletionSource<UtilityTerminal>();
                return await _terminalCompletionSource.Task;
            }
            finally
            {
                TerminalPicker.Visibility = Visibility.Collapsed;
                MainUI.Visibility = Visibility.Visible;
                MyMapView.GeoViewTapped += OnGeoViewTapped;
            }
        }

        private void OnTerminalSelected(object sender, RoutedEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(Picker.SelectedItem as UtilityTerminal);
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            // Reset the UI.
            Status.Text = "Click on the network lines or points to add a utility element.";
            IsBusy.Visibility = Visibility.Hidden;
            TraceTypes.SelectedIndex = 0;

            // Clear collections of starting locations and barriers.
            _startingLocations.Clear();
            _barriers.Clear();

            // Clear the map of any locations, barriers and trace result.
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }

        private async void OnTrace(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected trace type.
                UtilityTraceType traceType = (UtilityTraceType)TraceTypes.SelectedItem;

                // Update the UI.
                MainUI.IsEnabled = false;
                IsBusy.Visibility = Visibility.Visible;
                Status.Text = $"Running {traceType.ToString().ToLower()} trace...";

                // Clear previous selection from the layers.
                MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Build trace parameters.
                UtilityTraceParameters parameters = new UtilityTraceParameters(traceType, _startingLocations);
                foreach (UtilityElement barrier in _barriers)
                {
                    parameters.Barriers.Add(barrier);
                }

                // Set the trace configuration using the tier from the utility domain network.
                parameters.TraceConfiguration = _mediumVoltageTier.GetDefaultTraceConfiguration();

                //  Get the trace result from the utility network.
                IEnumerable<UtilityTraceResult> traceResult = await _utilityNetwork.TraceAsync(parameters);
                UtilityElementTraceResult elementTraceResult = traceResult?.FirstOrDefault() as UtilityElementTraceResult;

                // Check if there are any elements in the result.
                if (elementTraceResult?.Elements?.Count > 0)
                {
                    foreach (FeatureLayer layer in MyMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                    {
                        IEnumerable<UtilityElement> elements = elementTraceResult.Elements.Where(element => element.NetworkSource.Name == layer.FeatureTable.TableName);
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elements);
                        layer.SelectFeatures(features);
                    }
                }
                Status.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                Status.Text = "Trace failed...";
                if (ex is ArcGISWebException && ex.Message == null)
                {
                    MessageBox.Show("Trace failed.", ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                MainUI.IsEnabled = true;
                IsBusy.Visibility = Visibility.Hidden;
            }
        }
    }
}