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
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.TraceUtilityNetwork
{
    [Register("TraceUtilityNetwork")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Trace a utility network",
        "Network Analysis",
        "Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class TraceUtilityNetwork : UIViewController
    {
        // Hold references to UI controls.
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _traceButton;
        private UISegmentedControl _startBarrierPicker;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityIndicator;
        private UISegmentedControl _tracePicker;

        // Feature service for an electric utility network in Naperville, Illinois.
        private const string FeatureServiceUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        // Viewpoint in the utility network area.
        private Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9812980.8041217551, 5128523.87694709, -9812798.4363710005, 5128627.6261982173, SpatialReferences.WebMercator));

        // Utility network objects.
        private UtilityNetwork _utilityNetwork;
        private List<UtilityElement> _startingLocations = new List<UtilityElement>();
        private List<UtilityElement> _barriers = new List<UtilityElement>();
        private UtilityTier _mediumVoltageTier;

        // Task completion source for the user selected terminal.
        private TaskCompletionSource<string> _terminalCompletionSource = null;

        // Markers for the utility elements.
        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public TraceUtilityNetwork()
        {
            Title = "Trace a utility network";
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
                UniqueValue mediumVoltageValue = new UniqueValue("N/A", "Medium Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 3), 5);
                UniqueValue lowVoltageValue = new UniqueValue("N/A", "Low Voltage", new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.DarkCyan, 3), 3);
                lineLayer.Renderer = new UniqueValueRenderer(new List<string>() { "ASSETGROUP" }, new List<UniqueValue>() { mediumVoltageValue, lowVoltageValue }, "", new SimpleLineSymbol());
                _myMapView.Map.OperationalLayers.Add(lineLayer);

                // Add the layer with electric devices.
                FeatureLayer electricDevicelayer = new FeatureLayer(new Uri($"{FeatureServiceUrl}/100"));
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
                _statusLabel.Text = "Click on the network lines or points to add a utility element.";
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
                _activityIndicator.StartAnimating();
                _statusLabel.Text = "Identifying trace locations...";

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
                    _statusLabel.Text = $"Terminal: {element.Terminal?.Name ?? "default"}";
                }
                else if (element.NetworkSource.SourceType == UtilityNetworkSourceType.Edge)
                {
                    // Compute how far tapped location is along the edge feature.
                    if (feature.Geometry is Polyline line)
                    {
                        line = GeometryEngine.RemoveZ(line) as Polyline;
                        element.FractionAlongEdge = GeometryEngine.FractionAlong(line, e.Location, -1);
                        _statusLabel.Text = $"Fraction along edge: {element.FractionAlongEdge}";
                    }
                }

                // Check whether starting location or barrier is added to update the right collection and symbology.
                Symbol symbol = null;
                if (_startBarrierPicker.SelectedSegment == 0)
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
                // Get the selected trace type.
                UtilityTraceType traceType = (UtilityTraceType)Enum.GetValues(typeof(UtilityTraceType)).GetValue(_tracePicker.SelectedSegment);

                // Update the UI.
                _activityIndicator.StartAnimating();
                _statusLabel.Text = $"Running {traceType.ToString().ToLower()} trace...";

                // Clear previous selection from the layers.
                _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ToList().ForEach(layer => layer.ClearSelection());

                // Build trace parameters.
                UtilityTraceParameters parameters = new UtilityTraceParameters(traceType, _startingLocations);
                foreach (UtilityElement barrier in _barriers)
                {
                    parameters.Barriers.Add(barrier);
                }

                // Set the trace configuration using the tier from the utility domain network.
                parameters.TraceConfiguration = _mediumVoltageTier.TraceConfiguration;

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
                _statusLabel.Text = "Trace completed.";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Trace failed...";
                if (ex is ArcGISWebException && ex.Message == null)
                {
                    new UIAlertView(ex.GetType().Name, $"HResult: {ex.HResult}", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
                else
                {
                    new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
                }
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

            // Clear collections of starting locations and barriers.
            _startingLocations.Clear();
            _barriers.Clear();

            // Clear the map of any locations, barriers and trace result.
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

            _tracePicker = new UISegmentedControl(Enum.GetNames(typeof(UtilityTraceType)));
            _tracePicker.SelectedSegment = 0;
            _tracePicker.TranslatesAutoresizingMaskIntoConstraints = false;

            _startBarrierPicker = new UISegmentedControl(new string[] { "Start", "Barrier" });
            _startBarrierPicker.SelectedSegment = 0;

            _traceButton = new UIBarButtonItem();
            _traceButton.Title = "Trace";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _toolbar.Items = new[]
            {
                new UIBarButtonItem(_startBarrierPicker),
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
            View.AddSubviews(_myMapView, _statusLabel, _tracePicker, _toolbar, _activityIndicator);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
                {
                    _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _myMapView.BottomAnchor.ConstraintEqualTo(_tracePicker.TopAnchor),

                    _tracePicker.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _tracePicker.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _tracePicker.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor),

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