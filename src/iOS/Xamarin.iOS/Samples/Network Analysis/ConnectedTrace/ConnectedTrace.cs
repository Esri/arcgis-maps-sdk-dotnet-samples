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
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ConnectedTrace
{
    [Register("ConnectedTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find connected features in utility networks",
        "Network Analysis",
        "Find all features connected to a given set of starting point(s) and barrier(s) in your network using the Connected trace type.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class ConnectedTrace : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _traceButton;
        private UISegmentedControl _segmentedControl;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityIndicator;

        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private TaskCompletionSource<string> _terminalCompletionSource = null;

        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9813547.35557238, 5129980.36635111, -9813185.0602376, 5130215.41254146, SpatialReferences.WebMercator));

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public ConnectedTrace()
        {
            Title = "Find connected features in utility networks";
        }

        private async void Initialize()
        {
            try
            {
                _activityIndicator.StartAnimating();
                _statusLabel.Text = "Loading Utility Network...";

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

                _statusLabel.Text = "Click on the network lines or points to add a utility element.";

                // Create symbols for starting points and barriers.
                _startingPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Green, 20d);
                _barrierPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 20d);

                // Create a graphics overlay.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(graphicsOverlay);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Loading Utility Network failed...";
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
            }
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                e.Handled = true;

                _activityIndicator.StartAnimating();
                _statusLabel.Text = "Identifying trace locations...";

                // Set whether the user is adding a starting point or a barrier.
                bool isAddingStart = _segmentedControl.SelectedSegment == 0;

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
                        _statusLabel.Text = $"terminal: `{terminal?.Name ?? "default"}`";
                    }
                    else
                    {
                        element = _utilityNetwork.CreateElement(feature, terminals.FirstOrDefault());
                        _statusLabel.Text = $"terminal: `{element.Terminal?.Name ?? "default"}`";
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

                        _statusLabel.Text = $"fractionAlongEdge: `{element.FractionAlongEdge}`";
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
                _statusLabel.Text = "Identifying locations failed...";
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                if (_statusLabel.Text.Equals("Identifying trace locations...")) { _statusLabel.Text = "Could not identify location."; }
                _activityIndicator.StopAnimating();
            }
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Prevent user from tapping map while selecting terminal.
                _myMapView.GeoViewTapped -= OnGeoViewTapped;

                // Start the UI for the user choosing the junction.
                UIAlertController prompt = UIAlertController.Create(null, "Choose terminal", UIAlertControllerStyle.ActionSheet);

                foreach (UtilityTerminal terminal in terminals)
                {
                    prompt.AddAction(UIAlertAction.Create(terminal.Name, UIAlertActionStyle.Default, Choose_Click));
                }

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.SourceView = _toolbar;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                    ppc.DidDismiss += (s, e) => _terminalCompletionSource.TrySetCanceled();
                }

                PresentViewController(prompt, true, null);

                // Wait for the user to select a terminal.
                _terminalCompletionSource = new TaskCompletionSource<string>();
                string selectedName = await _terminalCompletionSource.Task;
                return terminals.Where(x => x.Name.Equals(selectedName)).FirstOrDefault();
            }
            finally
            {
                // Enable the main UI again.
                _myMapView.GeoViewTapped += OnGeoViewTapped;
            }
        }

        private void Choose_Click(UIAlertAction action)
        {
            _terminalCompletionSource.TrySetResult(action.Title);
        }

        private async void OnTrace(object sender, EventArgs e)
        {
            try
            {
                _activityIndicator.StartAnimating();
                _statusLabel.Text = "Running connected trace...";

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
                _statusLabel.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Trace failed...";
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                _activityIndicator.StopAnimating();
            }
        }

        private void OnReset(object sender, EventArgs e)
        {
            // Reset the UI.
            _statusLabel.Text = "Click on the network lines or points to add a utility element.";
            _activityIndicator.StopAnimating();

            // Clear the utility trace parameters.
            _parameters = null;

            // Clear the map layers and graphics.
            _myMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
            _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();
            View.BackgroundColor = UIColor.White;

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _statusLabel = new UILabel
            {
                Text = "Instructions",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _segmentedControl = new UISegmentedControl(new string[] { "Start", "Barrier" });
            _segmentedControl.SelectedSegment = 0;

            _traceButton = new UIBarButtonItem();
            _traceButton.Title = "Trace";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _toolbar.Items = new[]
            {
                new UIBarButtonItem(_segmentedControl),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _traceButton,
                _resetButton
            };

            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HidesWhenStopped = true,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f)
            };

            // Add the views.
            View.AddSubviews(_myMapView, _statusLabel, _toolbar, _activityIndicator);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
                {
                    _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor),
                    _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                    _statusLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                    _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _statusLabel.HeightAnchor.ConstraintEqualTo(40),
                    _activityIndicator.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor),
                    _activityIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _activityIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _activityIndicator.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
                }
            );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += OnGeoViewTapped;
            _traceButton.Clicked += OnTrace;
            _resetButton.Clicked += OnReset;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= OnGeoViewTapped;
            _traceButton.Clicked -= OnTrace;
            _resetButton.Clicked -= OnReset;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}