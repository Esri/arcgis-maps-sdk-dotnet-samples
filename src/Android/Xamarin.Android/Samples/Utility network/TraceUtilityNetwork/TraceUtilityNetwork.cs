// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
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
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.TraceUtilityNetwork
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Trace utility network",
        category: "Utility network",
        description: "Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.",
        instructions: "Tap on one or more features while 'Add starting locations' or 'Add barriers' is selected. When a junction feature is identified, you may be prompted to select a terminal. When an edge feature is identified, the distance from the tapped location to the beginning of the edge feature will be computed. Select the type of trace using the drop down menu. Tap 'Trace' to initiate a trace on the network. Tap 'Reset' to clear the trace parameters and start over.",
        tags: new[] { "condition barriers", "downstream trace", "network analysis", "subnetwork trace", "trace configuration", "traversability", "upstream trace", "utility network", "validate consistency", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("TraceUtilityNetwork.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class TraceUtilityNetwork : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private RadioButton _addStartButton;
        private RadioButton _addBarrierButton;
        private Button _traceButton;
        private Button _resetButton;
        private Button _traceTypeButton;
        private TextView _status;
        private ProgressBar _progressBar;

        private bool isAddingStart = true;

        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/server/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        // Viewpoint in the utility network area.
        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9812980.8041217551, 5128523.87694709, -9812798.4363710005, 5128627.6261982173, SpatialReferences.WebMercator));

        // Utility network objects.
        private UtilityNetwork _utilityNetwork;
        private List<UtilityElement> _startingLocations = new List<UtilityElement>();
        private List<UtilityElement> _barriers = new List<UtilityElement>();
        private UtilityTier _mediumVoltageTier;
        private UtilityTraceType _utilityTraceType = UtilityTraceType.Connected;

        // Task completion source for the user selected terminal.
        private TaskCompletionSource<int> _terminalCompletionSource = null;

        // Markers for the utility elements.
        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Trace a utility network";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
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
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;
                _status.Text = "Loading Utility Network...";

                // Create a map.
                _myMapView.Map = new Map(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")))
                {
                    InitialViewpoint = _startingViewpoint
                };

                // Add the layer with electric distribution lines.
                FeatureLayer lineLayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/3"));
                UniqueValue mediumVoltageValue = new UniqueValue("N/A", "Medium Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 3), 5);
                UniqueValue lowVoltageValue = new UniqueValue("N/A", "Low Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.DarkCyan, 3), 3);
                lineLayer.Renderer = new UniqueValueRenderer(new List<string>() { "ASSETGROUP" }, new List<UniqueValue>() { mediumVoltageValue, lowVoltageValue }, "", new SimpleLineSymbol());
                _myMapView.Map.OperationalLayers.Add(lineLayer);

                // Add the layer with electric devices.
                FeatureLayer electricDevicelayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/0"));
                _myMapView.Map.OperationalLayers.Add(electricDevicelayer);

                // Set the selection color for features in the map view.
                _myMapView.SelectionProperties = new SelectionProperties(System.Drawing.Color.Yellow);

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), _myMapView.Map);

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
                _myMapView.GraphicsOverlays.Add(graphicsOverlay);

                // Set the instruction text.
                _status.Text = "Tap on the network lines or points to add a utility element.";
            }
            catch (Exception ex)
            {
                _status.Text = "Loading Utility Network failed...";
                CreateDialog(ex.Message, title: ex.GetType().Name);
            }
            finally
            {
                _progressBar.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;
                _status.Text = "Identifying trace locations...";

                // Identify the feature to be used.
                IEnumerable<IdentifyLayerResult> identifyResult = await _myMapView.IdentifyLayersAsync(e.Position, 10.0, false);
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
                    _status.Text = $"Terminal: {element.Terminal?.Name ?? "default"}";
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
                        _status.Text = $"Fraction along edge: {element.FractionAlongEdge}";
                    }
                }

                // Check whether starting location or barrier is added to update the right collection and symbology.
                Symbol symbol = null;
                if (isAddingStart)
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
                _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Add(traceLocationGraphic);
            }
            catch (Exception ex)
            {
                _status.Text = "Identifying locations failed.";
                CreateDialog(ex.Message, ex.GetType().Name);
            }
            finally
            {
                if (_status.Text.Equals("Identifying trace locations...")) { _status.Text = "Could not identify location."; }
                _progressBar.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            // Create an array of the terminal names for the UI.
            string[] terminalNames = terminals.ToList().Select(t => t.Name).ToArray();

            // Create UI for terminal selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select terminal for junction");
            builder.SetItems(terminalNames, Choose_Click);
            builder.SetCancelable(false);
            builder.Show();

            // Return the selected terminal.
            _terminalCompletionSource = new TaskCompletionSource<int>();
            string selectedName = terminalNames[await _terminalCompletionSource.Task];
            return terminals.Where(x => x.Name.Equals(selectedName)).FirstOrDefault();
        }

        private void Choose_Click(object sender, DialogClickEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(e.Which);
        }

        private void ChooseTraceType(object sender, EventArgs e)
        {
            // Create UI for trace type selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select trace type");
            builder.SetItems(new string[] { "Connected", "Subnetwork", "Upstream", "Downstream" }, ChangeTraceType);
            builder.SetCancelable(true);
            builder.Show();
        }

        private void ChangeTraceType(object sender, DialogClickEventArgs e)
        {
            _utilityTraceType = (UtilityTraceType)Enum.GetValues(typeof(UtilityTraceType)).GetValue(e.Which);
            _traceTypeButton.Text = $"Trace type: {_utilityTraceType}";
        }

        private async void Trace_Click(object sender, EventArgs e)
        {
            try
            {
                // Update the UI.
                _progressBar.Visibility = Android.Views.ViewStates.Visible;
                _status.Text = $"Running {_utilityTraceType.ToString().ToLower()} trace...";

                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Build trace parameters.
                UtilityTraceParameters parameters = new UtilityTraceParameters(_utilityTraceType, _startingLocations);
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
                    foreach (FeatureLayer layer in _myMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                    {
                        IEnumerable<UtilityElement> elements = elementTraceResult.Elements.Where(element => element.NetworkSource.Name == layer.FeatureTable.TableName);
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elements);
                        layer.SelectFeatures(features);
                    }
                }
                _status.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                _status.Text = "Trace failed...";
                if (ex is ArcGISWebException && ex.Message == null)
                {
                    CreateDialog("Trace failed.", ex.GetType().Name);
                }
                else
                {
                    CreateDialog(ex.Message, ex.GetType().Name);
                }
            }
            finally
            {
                _progressBar.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            // Reset the UI.
            _status.Text = "Tap on the network lines or points to add a utility element.";
            _progressBar.Visibility = Android.Views.ViewStates.Invisible;

            // Clear the utility trace parameters.
            _startingLocations.Clear();
            _barriers.Clear();

            // Clear the map layers and graphics.
            _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            SetContentView(Resource.Layout.TraceUtilityNetwork);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);

            _addStartButton = FindViewById<RadioButton>(Resource.Id.addStart);
            _addBarrierButton = FindViewById<RadioButton>(Resource.Id.addBarrier);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _traceTypeButton = FindViewById<Button>(Resource.Id.traceTypeButton);
            _status = FindViewById<TextView>(Resource.Id.statusLabel);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            _myMapView.GeoViewTapped += GeoViewTapped;
            _traceButton.Click += Trace_Click;
            _resetButton.Click += Reset_Click;

            _traceTypeButton.Click += ChooseTraceType;

            _addStartButton.Click += (s, e) => { isAddingStart = true; };
            _addBarrierButton.Click += (s, e) => { isAddingStart = false; };
        }

        private void CreateDialog(string message, string title = null)
        {
            // Create a dialog to show message to user.
            AlertDialog alert = new AlertDialog.Builder(this).Create();
            if (title != null) alert.SetTitle(title);
            alert.SetMessage(message);
            alert.Show();
        }
    }
}