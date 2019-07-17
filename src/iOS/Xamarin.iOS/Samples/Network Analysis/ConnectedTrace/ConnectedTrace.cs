// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ConnectedTrace
{
    [Register("ConnectedTrace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Connected trace",
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
        private string _selectedTerminalName;

        private readonly int[] LayerIds = new int[] { 4, 3, 5, 0 };

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private CancellationTokenSource _selectTerminalTokenSource;

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        public ConnectedTrace()
        {
            Title = "Connected trace";
        }

        private async void Initialize()
        {
            
            // TODO: Delete once SS6 is used.
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.ChallengeHandler = new Esri.ArcGISRuntime.Security.ChallengeHandler(async (info) =>
            {
                return await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.GenerateCredentialAsync(new Uri(PortalUrl), Username, Password);
            });

            try
            {
                _activityIndicator.StartAnimating();
                _statusLabel.Text = "Loading Utility Network...";

                // Setup Map with Feature Layer(s) that contain Utility Network.
                _myMapView.Map = new Map(Basemap.CreateStreetsNightVector())
                {
                    InitialViewpoint = new Viewpoint(new Envelope(-9813547.35557238, 5129980.36635111, -9813185.0602376, 5130215.41254146, SpatialReferences.WebMercator))
                };

                foreach (int id in LayerIds)
                {
                    _myMapView.Map.OperationalLayers.Add(new FeatureLayer(new Uri($"{FeatureServiceUrl}/{id}")));
                }

                // Create and load the UtilityNetwork.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServiceUrl), MyMapView.Map);

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
            
            await WaitForTerminal(null);
            /*
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                MyMapView.GeoViewTapped += OnGeoViewTapped;
                IsBusy.Visibility = Visibility.Hidden;
            }
            */
        }

        private async Task<UtilityTerminal> WaitForTerminal(IEnumerable<UtilityTerminal> terminals)
        {
            try
            {
                // Start the UI for the user choosing the junction.
                UIAlertController prompt = UIAlertController.Create(null, "Choose terminal", UIAlertControllerStyle.ActionSheet);

                //foreach(UtilityTerminal terminal in terminals)
                var strings = new string[] { "Terminal 1", "Terminal 2" };
                foreach (string s in strings)
                {
                    //prompt.AddAction(UIAlertAction.Create(terminal.Name, UIAlertActionStyle.Default, Choose_Click));
                    prompt.AddAction(UIAlertAction.Create(s, UIAlertActionStyle.Default, Choose_Click));
                }

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.SourceView = View;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                }

                PresentViewController(prompt, true, null);

                // Wait for the user to make a selection.
                _selectTerminalTokenSource = new CancellationTokenSource();
                await Task.Delay(-1, _selectTerminalTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _statusLabel.Text = _selectedTerminalName;
                return null;
                //return terminals.Where(x => x.Name.Equals(_selectedTerminalName)).FirstOrDefault();
            }
            throw new Exception("Terminal not selected.");
        }

        private void Choose_Click(UIAlertAction action)
        {
            _selectedTerminalName = action.Title;
            _selectTerminalTokenSource.Cancel();
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
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= OnGeoViewTapped;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}