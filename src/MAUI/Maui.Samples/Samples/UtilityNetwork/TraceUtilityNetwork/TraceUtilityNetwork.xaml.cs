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
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UtilityNetworks;
using System.Diagnostics;

namespace ArcGIS.Samples.TraceUtilityNetwork
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Trace utility network",
        category: "Utility network",
        description: "Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.",
        instructions: "Tap on one or more features while 'Add starting locations' or 'Add barriers' is selected. When a junction feature is identified, you may be prompted to select a terminal. When an edge feature is identified, the distance from the tapped location to the beginning of the edge feature will be computed. Select the type of trace using the drop down menu. Tap 'Trace' to initiate a trace on the network. Tap 'Reset' to clear the trace parameters and start over.",
        tags: new[] { "condition barriers", "downstream trace", "network analysis", "subnetwork trace", "trace configuration", "traversability", "upstream trace", "utility network", "validate consistency" })]
    public partial class TraceUtilityNetwork : ContentPage
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
        private UtilityTraceType _selectedTraceType = UtilityTraceType.Connected;

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

                    return await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                BusyIndicator.IsVisible = true;
                Status.Text = "Loading Utility Network...";

                // Create a map.
                MyMapView.Map = new Map(BasemapStyle.ArcGISDarkGray)
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
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
            finally
            {
                MainUI.IsEnabled = true;
                BusyIndicator.IsVisible = false;
            }
        }

        private void IsAddingStartingLocations_Clicked(object sender, EventArgs e)
        {
            IsAddingStartingLocations.IsEnabled = false;
            IsAddingBarriers.IsEnabled = true;
        }

        private void IsAddingBarriers_Clicked(object sender, EventArgs e)
        {
            IsAddingStartingLocations.IsEnabled = true;
            IsAddingBarriers.IsEnabled = false;
        }

        private async void OnGeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                BusyIndicator.IsVisible = true;
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
                        element.Terminal = await WaitForTerminal(terminals);
                    }
                    Status.Text = $"Terminal: {element.Terminal?.Name ?? "default"}";
                }
                else if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Edge)
                {
                    // Compute how far tapped location is along the edge feature.
                    if (feature.Geometry is Polyline line)
                    {
                        line = GeometryEngine.RemoveZ(line) as Polyline;
                        double fraction = GeometryEngine.FractionAlong(line, e.Location, -1);
                        if (double.IsNaN(fraction)) { return; }
                        element.FractionAlongEdge = fraction;
                        Status.Text = $"Fraction along edge: {element.FractionAlongEdge}";
                    }
                }

                // Check whether starting location or barrier is added to update the right collection and symbology.
                Symbol symbol = null;
                if (IsAddingStartingLocations.IsEnabled != true)
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
            catch (OperationCanceledException)
            {
                Status.Text = "Identify canceled.";
                return;
            }
            catch (Exception ex)
            {
                Status.Text = "Identifying locations failed.";
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
            finally
            {
                if (Status.Text.Equals("Identifying trace locations...")) { Status.Text = "Could not identify location."; }
                BusyIndicator.IsVisible = false;
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            // Load the terminals into a DisplayActionSheet and await the user's selection.
            var terminalArray = terminals.Select(x => x.Name).ToArray();
            string choice = await Application.Current.MainPage.DisplayActionSheet("Choose junction.", "Cancel", null, terminalArray);
            if (terminalArray.Contains(choice))
            {
                return terminals.Single(x => x.Name == choice);
            }
            else
            {
                throw new OperationCanceledException();
            }
        }

        private void OnReset(object sender, EventArgs e)
        {
            // Reset the UI.
            Status.Text = "Click on the network lines or points to add a utility element.";
            BusyIndicator.IsVisible = false;

            // Clear collections of starting locations and barriers.
            _startingLocations.Clear();
            _barriers.Clear();

            // Clear the map of any locations, barriers and trace result.
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            MyMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                // Get the selected trace type.
                UtilityTraceType traceType = _selectedTraceType;

                // Update the UI.
                MainUI.IsEnabled = false;
                BusyIndicator.IsVisible = true;
                Status.Text = $"Running {traceType} trace...";

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
                    await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, "Trace failed.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
                }
            }
            finally
            {
                MainUI.IsEnabled = true;
                BusyIndicator.IsVisible = false;
            }
        }

        private async void TraceTypeButtonPressed(object sender, EventArgs e)
        {
            try
            {
                // Prompt the user to select a type of trace.
                var traceTypes = new string[] { "Connected", "Subnetwork", "Upstream", "Downstream" };
                string choice = await Application.Current.MainPage.DisplayActionSheet("Choose type of trace", "Cancel", null, traceTypes);

                // Set the selected trace type.
                _selectedTraceType = (UtilityTraceType)Enum.Parse(typeof(UtilityTraceType), choice);
                TracePickerButton.Text = $"Trace type: {_selectedTraceType}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}