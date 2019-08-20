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

namespace ArcGISRuntime.Samples.FindFeaturesUtilityNetwork
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find connected features in utility networks",
        "Network Analysis",
        "Find all features connected to a given set of starting point(s) and barrier(s) in your network using the Connected trace type.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("FindFeaturesUtilityNetwork.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class FindFeaturesUtilityNetwork : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;

        private RadioButton _addStartButton;
        private RadioButton _addBarrierButton;
        private Button _traceButton;
        private Button _resetButton;
        private TextView _status;
        private ProgressBar _progressBar;

        private bool isAddingStart = true;

        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private TaskCompletionSource<int> _terminalCompletionSource = null;

        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9813547.35557238, 5129980.36635111, -9813185.0602376, 5130215.41254146, SpatialReferences.WebMercator));

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find connected features in utility networks";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;
                _status.Text = "Loading Utility Network...";

                // Setup Map with Feature Layer(s) that contain Utility Network.
                _myMapView.Map = new Map(Basemap.CreateStreetsNightVector())
                {
                    InitialViewpoint = _startingViewpoint
                };

                // Add the layer with electric distribution lines.
                FeatureLayer lineLayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/115"));
                lineLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 3));
                _myMapView.Map.OperationalLayers.Add(lineLayer);

                // Add the layer with electric devices.
                FeatureLayer electricDevicelayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/100"));
                _myMapView.Map.OperationalLayers.Add(electricDevicelayer);

                // Create and load the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), _myMapView.Map);

                _status.Text = "Click on the network lines or points to add a utility element.";

                // Create symbols for starting points and barriers.
                _startingPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Green, 20d);
                _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 20d);

                // Create a graphics overlay.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(graphicsOverlay);
            }
            catch (Exception ex)
            {
                _status.Text = "Loading Utility Network failed...";
                CreateDialog(ex.Message);
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
                        _status.Text = $"Terminal: {terminal?.Name ?? "default"}";
                    }
                    else
                    {
                        element = _utilityNetwork.CreateElement(feature, terminals.FirstOrDefault());
                        _status.Text = $"Terminal: {element.Terminal?.Name ?? "default"}";
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

                        _status.Text = $"Fraction along edge: {element.FractionAlongEdge}";
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
                _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Add(traceLocationGraphic);
            }
            catch (Exception ex)
            {
                _status.Text = "Identifying locations failed...";
                CreateDialog(ex.Message);
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

        private async void Trace_Click(object sender, EventArgs e)
        {
            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;
                _status.Text = "Running connected trace...";

                // Verify that the parameters contain a starting location.
                if (_parameters == null || !_parameters.StartingLocations.Any()) { throw new Exception("No starting locations set."); }

                //  Get the trace result from the utility network.
                IEnumerable<UtilityTraceResult> traceResult = await _utilityNetwork.TraceAsync(_parameters);
                UtilityElementTraceResult elementTraceResult = traceResult?.FirstOrDefault() as UtilityElementTraceResult;

                if (elementTraceResult?.Elements?.Count > 0)
                {
                    // Clear previous selection from the layer.
                    _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                    // Group the utility elements by their network source.
                    IEnumerable<IGrouping<string, UtilityElement>> groupedElementsResult = from element in elementTraceResult.Elements
                                                                                           group element by element.NetworkSource.Name into groupedElements
                                                                                           select groupedElements;

                    foreach (IGrouping<string, UtilityElement> elementGroup in groupedElementsResult)
                    {
                        // Get the layer for the utility element.
                        FeatureLayer layer = (FeatureLayer)_myMapView.Map.OperationalLayers.FirstOrDefault(l => l is FeatureLayer && ((FeatureLayer)l).FeatureTable.TableName == elementGroup.Key);
                        if (layer == null)
                            continue;

                        // Convert elements to features to highlight result.
                        IEnumerable<Feature> features = await _utilityNetwork.GetFeaturesForElementsAsync(elementGroup);
                        layer.SelectFeatures(features);
                    }
                }
                _status.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                _status.Text = "Trace failed...";
                CreateDialog(ex.Message);
            }
            finally
            {
                _progressBar.Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            // Reset the UI.
            _status.Text = "Click on the network lines or points to add a utility element.";
            _progressBar.Visibility = Android.Views.ViewStates.Invisible;

            // Clear the utility trace parameters.
            _parameters = null;

            // Clear the map layers and graphics.
            _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            SetContentView(Resource.Layout.FindFeaturesUtilityNetwork);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);

            _addStartButton = FindViewById<RadioButton>(Resource.Id.addStart);
            _addBarrierButton = FindViewById<RadioButton>(Resource.Id.addBarrier);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _status = FindViewById<TextView>(Resource.Id.statusLabel);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            _myMapView.GeoViewTapped += GeoViewTapped;
            _traceButton.Click += Trace_Click;
            _resetButton.Click += Reset_Click;

            _addStartButton.Click += (s, e) => { isAddingStart = true; };
            _addBarrierButton.Click += (s, e) => { isAddingStart = false; };
        }

        private void CreateDialog(string message)
        {
            // Create a dialog to show message to user.
            AlertDialog alert = new AlertDialog.Builder(this).Create();
            alert.SetMessage(message);
            alert.Show();
        }
    }
}