// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.RouteAroundBarriers
{
    [Register("RouteAroundBarriers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Route around barriers",
        category: "Network analysis",
        description: "Find a route that reaches all stops without crossing any barriers.",
        instructions: "Tap 'Add stop' to add stops to the route. Tap 'Add barrier' to add areas that can't be crossed by the route. Tap 'Route' to find the route and display it. Select 'Allow stops to be re-ordered' to find the best sequence. Select 'Preserve first stop' if there is a known start point, and 'Preserve last stop' if there is a known final destination.",
        tags: new[] { "barriers", "best sequence", "directions", "maneuver", "network analysis", "routing", "sequence", "stop order", "stops" })]
    public class RouteAroundBarriers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UISegmentedControl _stopsOrBarriersPicker;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _settingsButton;
        private UIBarButtonItem _routeButton;
        private UIBarButtonItem _directionsButton;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityIndicator;
        private UITableViewController _directionsController;
        private RouteSettingsViewController _routeSettingsController;
        private DirectionsViewModel _directionsViewModel;

        // Track the current state of the sample.
        private SampleState _currentSampleState;

        // Graphics overlays to maintain the stops, barriers, and route result.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _stopsOverlay;
        private GraphicsOverlay _barriersOverlay;

        // The route task manages routing work.
        private RouteTask _routeTask;

        // The route parameters defines how the route will be calculated.
        private RouteParameters _routeParameters;

        // Symbols for displaying the barriers and the route line.
        private Symbol _routeSymbol;
        private Symbol _barrierSymbol;

        // URL to the network analysis service.
        private const string RouteServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route";

        // Hold a list of directions.
        private readonly List<DirectionManeuver> _directions = new List<DirectionManeuver>();

        public RouteAroundBarriers()
        {
            Title = "Route around barriers";
        }

        private async void Initialize()
        {
            try
            {
                // Update interface state.
                UpdateInterfaceState(SampleState.NotReady);

                // Create the map with a basemap.
                Map sampleMap = new Map(Basemap.CreateTopographicVector());
                sampleMap.InitialViewpoint = new Viewpoint(32.7157, -117.1611, 1e5);
                _myMapView.Map = sampleMap;

                // Create the graphics overlays. These will manage rendering of route, direction, stop, and barrier graphics.
                _routeOverlay = new GraphicsOverlay();
                _stopsOverlay = new GraphicsOverlay();
                _barriersOverlay = new GraphicsOverlay();

                // Add graphics overlays to the map view.
                _myMapView.GraphicsOverlays.Add(_routeOverlay);
                _myMapView.GraphicsOverlays.Add(_stopsOverlay);
                _myMapView.GraphicsOverlays.Add(_barriersOverlay);

                // Create and initialize the route task.
                _routeTask = await RouteTask.CreateAsync(new Uri(RouteServiceUrl));

                // Get the route parameters from the route task.
                _routeParameters = await _routeTask.CreateDefaultParametersAsync();

                // Prepare symbols.
                _routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
                _barrierSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Cross, Color.Red, null);

                // Configure directions display.
                _directionsViewModel = new DirectionsViewModel(_directions);

                // Create the table view controller for displaying directions.
                _directionsController = new UITableViewController(UITableViewStyle.Plain);

                // The table view content is managed by the view model.
                _directionsController.TableView.Source = _directionsViewModel;

                // Create the view controller for configuring the route.
                _routeSettingsController = new RouteSettingsViewController();

                // Enable the UI.
                UpdateInterfaceState(SampleState.Ready);
            }
            catch (Exception e)
            {
                UpdateInterfaceState(SampleState.NotReady);
                System.Diagnostics.Debug.WriteLine(e);
                ShowMessage("Couldn't load sample", "Couldn't start the sample. See the debug output for detail.");
            }
        }

        private async void HandleMapTap(MapPoint mapLocation)
        {
            // Normalize geometry - important for geometries that will be sent to a server for processing.
            mapLocation = (MapPoint)GeometryEngine.NormalizeCentralMeridian(mapLocation);

            switch (_currentSampleState)
            {
                case SampleState.AddingBarriers:
                    // Buffer the tapped point to create a larger barrier.
                    Geometry bufferedGeometry = GeometryEngine.BufferGeodetic(mapLocation, 500, LinearUnits.Meters);

                    // Create the graphic to show the barrier.
                    Graphic barrierGraphic = new Graphic(bufferedGeometry, _barrierSymbol);

                    // Add the graphic to the overlay - this will cause it to appear on the map.
                    _barriersOverlay.Graphics.Add(barrierGraphic);
                    break;

                case SampleState.AddingStops:
                    // Get the name of this stop.
                    string stopName = $"{_stopsOverlay.Graphics.Count + 1}";

                    // Create the marker to show underneath the stop number.
                    PictureMarkerSymbol pushpinMarker = await GetPictureMarker();

                    // Create the text symbol for showing the stop.
                    TextSymbol stopSymbol = new TextSymbol(stopName, Color.White, 15, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    stopSymbol.OffsetY = 15;

                    CompositeSymbol combinedSymbol = new CompositeSymbol(new MarkerSymbol[] { pushpinMarker, stopSymbol });

                    // Create the graphic to show the stop.
                    Graphic stopGraphic = new Graphic(mapLocation, combinedSymbol);

                    // Add the graphic to the overlay - this will cause it to appear on the map.
                    _stopsOverlay.Graphics.Add(stopGraphic);
                    break;
            }
        }

        private void ConfigureThenRoute()
        {
            // Guard against error conditions.
            if (_routeParameters == null)
            {
                ShowMessage("Not ready yet", "Sample isn't ready yet; define route parameters first.");
                UpdateInterfaceState(SampleState.Ready);
                return;
            }

            if (_stopsOverlay.Graphics.Count < 2)
            {
                ShowMessage("Not enough stops", "Add at least two stops before solving a route.");
                UpdateInterfaceState(SampleState.Ready);
                return;
            }

            // Clear any existing route from the map.
            _routeOverlay.Graphics.Clear();

            // Configure the route result to include directions and stops.
            _routeParameters.ReturnStops = true;
            _routeParameters.ReturnDirections = true;

            // Create a list to hold stops that should be on the route.
            List<Stop> routeStops = new List<Stop>();

            // Create stops from the graphics.
            foreach (Graphic stopGraphic in _stopsOverlay.Graphics)
            {
                // Note: this assumes that only points were added to the stops overlay.
                MapPoint stopPoint = (MapPoint)stopGraphic.Geometry;

                // Create the stop from the graphic's geometry.
                Stop routeStop = new Stop(stopPoint);

                // Set the name of the stop to its position in the list.
                routeStop.Name = $"{_stopsOverlay.Graphics.IndexOf(stopGraphic) + 1}";

                // Add the stop to the list of stops.
                routeStops.Add(routeStop);
            }

            // Configure the route parameters with the stops.
            _routeParameters.ClearStops();
            _routeParameters.SetStops(routeStops);

            // Create a list to hold barriers that should be routed around.
            List<PolygonBarrier> routeBarriers = new List<PolygonBarrier>();

            // Create barriers from the graphics.
            foreach (Graphic barrierGraphic in _barriersOverlay.Graphics)
            {
                // Get the polygon from the graphic.
                Polygon barrierPolygon = (Polygon)barrierGraphic.Geometry;

                // Create a barrier from the polygon.
                PolygonBarrier routeBarrier = new PolygonBarrier(barrierPolygon);

                // Add the barrier to the list of barriers.
                routeBarriers.Add(routeBarrier);
            }

            // Configure the route parameters with the barriers.
            _routeParameters.ClearPolygonBarriers();
            _routeParameters.SetPolygonBarriers(routeBarriers);

            // If the user allows stops to be re-ordered, the service will find the optimal order.
            _routeParameters.FindBestSequence = _routeSettingsController.AllowReorderStops;

            // If the user has allowed re-ordering, but has a definite start point, tell the service to preserve the first stop.
            _routeParameters.PreserveFirstStop = _routeSettingsController.PreserveFirstStop;

            // If the user has allowed re-ordering, but has a definite end point, tell the service to preserve the last stop.
            _routeParameters.PreserveLastStop = _routeSettingsController.PreserveLastStop;

            // Calculate and show the route.
            CalculateAndShowRoute();
        }

        private async void CalculateAndShowRoute()
        {
            try
            {
                // Calculate the route.
                RouteResult calculatedRoute = await _routeTask.SolveRouteAsync(_routeParameters);

                // Get the first returned result.
                Route firstResult = calculatedRoute.Routes.First();

                // Get the route geometry - this is the line that shows the route.
                Polyline calculatedRouteGeometry = firstResult.RouteGeometry;

                // Create the route graphic from the geometry and the symbol.
                Graphic routeGraphic = new Graphic(calculatedRouteGeometry, _routeSymbol);

                // Clear any existing routes, then add this one to the map.
                _routeOverlay.Graphics.Clear();
                _routeOverlay.Graphics.Add(routeGraphic);

                // Add the directions to the textbox.
                PrepareDirectionsList(firstResult.DirectionManeuvers);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e);
                ShowMessage("Routing error", $"Couldn't calculate route. See debug output for details. Message: {e.Message}");
            }
            finally
            {
                // Update the interface now that routing is finished.
                UpdateInterfaceState(SampleState.Ready);
            }
        }

        private void PrepareDirectionsList(IReadOnlyList<DirectionManeuver> directions)
        {
            // Clear existing directions.
            _directions.Clear();

            foreach (DirectionManeuver step in directions)
            {
                _directions.Add(step);
            }

            _directionsController.TableView.ReloadData();
        }

        private void HandleRouteClicked(object sender, EventArgs e)
        {
            UpdateInterfaceState(SampleState.Routing);
            ConfigureThenRoute();
        }

        private void HandleResetClicked(object sender, EventArgs e)
        {
            UpdateInterfaceState(SampleState.NotReady);
            _stopsOverlay.Graphics.Clear();
            _barriersOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();
            _directions.Clear();
            _directionsController.TableView.ReloadData();
            UpdateInterfaceState(SampleState.Ready);
        }

        private void HandleOpenSettings(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            var controller = new UINavigationController(_routeSettingsController);
            // Show a close button in the top right.
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => controller.DismissViewController(true, null));
            controller.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            // Show the table view in a popover.
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 250);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem)sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(controller, true, null);
        }

        private void HandleDirectionsClicked(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            var controller = new UINavigationController(_directionsController);
            controller.Title = "Directions";

            // Show a close button in the top right.
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => controller.DismissViewController(true, null));
            controller.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);

            // Show the table view in a popover.
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(500, 500);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem)sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(controller, true, null);
        }

        private void HandleStopsBarrierModeChange(object sender, EventArgs e)
        {
            switch (_stopsOrBarriersPicker.SelectedSegment)
            {
                case 0:
                    UpdateInterfaceState(SampleState.AddingStops);
                    break;

                case 1:
                    UpdateInterfaceState(SampleState.AddingBarriers);
                    break;
            }
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e) => HandleMapTap(e.Location);

        private async Task<PictureMarkerSymbol> GetPictureMarker()
        {
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 50;
            pinSymbol.Height = 50;
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;

            return pinSymbol;
        }

        private void UpdateInterfaceState(SampleState newState)
        {
            // Manage the UI state for the sample.
            _currentSampleState = newState;
            switch (_currentSampleState)
            {
                case SampleState.NotReady:
                    _stopsOrBarriersPicker.Enabled = false;
                    _resetButton.Enabled = false;
                    _settingsButton.Enabled = false;
                    _directionsButton.Enabled = false;
                    _routeButton.Enabled = false;
                    _statusLabel.Text = "Preparing sample...";
                    _activityIndicator.StartAnimating();
                    break;

                case SampleState.AddingBarriers:
                    _statusLabel.Text = "Tap the map to add a barrier.";
                    break;

                case SampleState.AddingStops:
                    _statusLabel.Text = "Tap the map to add a stop.";
                    break;

                case SampleState.Ready:
                    _stopsOrBarriersPicker.Enabled = true;
                    _resetButton.Enabled = true;
                    _settingsButton.Enabled = true;
                    _directionsButton.Enabled = true;
                    _routeButton.Enabled = true;
                    _statusLabel.Text = "Tap 'stops' or 'barriers', then tap the map to add.";
                    _stopsOrBarriersPicker.SelectedSegment = -1;
                    _activityIndicator.StopAnimating();
                    break;

                case SampleState.Routing:
                    _activityIndicator.StartAnimating();
                    _statusLabel.Text = "Calculating route...";
                    break;
            }
        }

        // Enum represents various UI states.
        private enum SampleState
        {
            NotReady,
            Ready,
            AddingStops,
            AddingBarriers,
            Routing
        }

        private void ShowMessage(string title, string detail)
        {
            new UIAlertView(title, detail, (IUIAlertViewDelegate)null, "OK", null).Show();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _stopsOrBarriersPicker = new UISegmentedControl("Stops", "Barriers");

            _resetButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh);

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

            _settingsButton = new UIBarButtonItem(UIImage.FromBundle("Settings"), UIBarButtonItemStyle.Plain, null);

            _routeButton = new UIBarButtonItem("Route", UIBarButtonItemStyle.Plain, null);

            _directionsButton = new UIBarButtonItem(UIImage.FromBundle("DirectionsList"), UIBarButtonItemStyle.Plain, null);

            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HidesWhenStopped = true,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f)
            };

            _toolbar.Items = new[]
            {
                new UIBarButtonItem(_stopsOrBarriersPicker),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton,
                _settingsButton,
                _routeButton,
                _directionsButton
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

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _settingsButton.Clicked += HandleOpenSettings;
            _resetButton.Clicked += HandleResetClicked;
            _stopsOrBarriersPicker.ValueChanged += HandleStopsBarrierModeChange;
            _routeButton.Clicked += HandleRouteClicked;
            _directionsButton.Clicked += HandleDirectionsClicked;
            _myMapView.GeoViewTapped += MapView_Tapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events.
            _settingsButton.Clicked -= HandleOpenSettings;
            _resetButton.Clicked -= HandleResetClicked;
            _stopsOrBarriersPicker.ValueChanged -= HandleStopsBarrierModeChange;
            _routeButton.Clicked -= HandleRouteClicked;
            _directionsButton.Clicked -= HandleDirectionsClicked;
            _myMapView.GeoViewTapped -= MapView_Tapped;
        }
    }

    internal class DirectionsViewModel : UITableViewSource
    {
        public List<DirectionManeuver> Directions;
        private const string CellIdentifier = "LayerTableCell";

        public DirectionsViewModel(List<DirectionManeuver> directions)
        {
            Directions = directions;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Gets a cell for the specified section and row within that section.
            var cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);

            cell.TextLabel.Text = Directions[indexPath.Row].DirectionText;
            cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            cell.TextLabel.Lines = 0;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Directions.Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            // Two sections - layers in the map and layers not in the map.
            return 1;
        }
    }

    public class RouteSettingsViewController : UIViewController
    {
        public bool AllowReorderStops;
        public bool PreserveFirstStop;
        public bool PreserveLastStop;

        public RouteSettingsViewController()
        {
            Title = "Route options";
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void LoadView()
        {
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(scrollView);

            scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            UIStackView formContainer = new UIStackView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.Spacing = 8;
            formContainer.LayoutMarginsRelativeArrangement = true;
            formContainer.Alignment = UIStackViewAlignment.Trailing;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;

            // Controls for configuring re-ordering.
            UISwitch reorderSwitch = new UISwitch();
            reorderSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            reorderSwitch.ValueChanged += ReorderSwitchOnValueChanged;

            UILabel reorderLabel = new UILabel();
            reorderLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            reorderLabel.Text = "Re-order stops";
            reorderLabel.TextAlignment = UITextAlignment.Right;
            reorderLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);

            UIStackView allowReorderRow = new UIStackView(new UIView[] { reorderSwitch, reorderLabel });
            allowReorderRow.TranslatesAutoresizingMaskIntoConstraints = false;
            allowReorderRow.Axis = UILayoutConstraintAxis.Horizontal;
            allowReorderRow.Distribution = UIStackViewDistribution.Fill;
            allowReorderRow.Spacing = 8;
            allowReorderRow.LayoutMarginsRelativeArrangement = true;
            allowReorderRow.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.AddArrangedSubview(allowReorderRow);

            // Controls for preserving first stop.
            UISwitch preserveFirstStopSwitch = new UISwitch();
            preserveFirstStopSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveFirstStopSwitch.ValueChanged += PreserveFirstStopSwitchOnValueChanged;

            UILabel preserveFirstStopLabel = new UILabel();
            preserveFirstStopLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveFirstStopLabel.Text = "Keep origin";
            preserveFirstStopLabel.TextAlignment = UITextAlignment.Right;
            preserveFirstStopLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);

            UIStackView preserveFirstStopRow = new UIStackView(new UIView[] { preserveFirstStopSwitch, preserveFirstStopLabel });
            preserveFirstStopRow.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveFirstStopRow.Axis = UILayoutConstraintAxis.Horizontal;
            preserveFirstStopRow.Distribution = UIStackViewDistribution.Fill;
            preserveFirstStopRow.Spacing = 8;
            preserveFirstStopRow.LayoutMarginsRelativeArrangement = true;
            preserveFirstStopRow.LayoutMargins = new UIEdgeInsets(8, 32, 8, 8);
            formContainer.AddArrangedSubview(preserveFirstStopRow);

            // Controls for preserving last stop.
            UISwitch preserveLastStopSwitch = new UISwitch();
            preserveLastStopSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveLastStopSwitch.ValueChanged += PreserveLastStopSwitchOnValueChanged;

            UILabel preserveLastStopLabel = new UILabel();
            preserveLastStopLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveLastStopLabel.Text = "Keep dest.";
            preserveLastStopLabel.TextAlignment = UITextAlignment.Right;
            preserveLastStopLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);

            UIStackView preserveLastStopRow = new UIStackView(new UIView[] { preserveLastStopSwitch, preserveLastStopLabel });
            preserveLastStopRow.TranslatesAutoresizingMaskIntoConstraints = false;
            preserveLastStopRow.Axis = UILayoutConstraintAxis.Horizontal;
            preserveLastStopRow.Distribution = UIStackViewDistribution.Fill;
            preserveLastStopRow.Spacing = 8;
            preserveLastStopRow.LayoutMarginsRelativeArrangement = true;
            preserveLastStopRow.LayoutMargins = new UIEdgeInsets(8, 32, 8, 8);
            formContainer.AddArrangedSubview(preserveLastStopRow);

            scrollView.AddSubview(formContainer);

            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;
            formContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
        }

        private void ReorderSwitchOnValueChanged(object sender, EventArgs e) => AllowReorderStops = ((UISwitch)sender).On;

        private void PreserveFirstStopSwitchOnValueChanged(object sender, EventArgs e) => PreserveFirstStop = ((UISwitch)sender).On;

        private void PreserveLastStopSwitchOnValueChanged(object sender, EventArgs e) => PreserveLastStop = ((UISwitch)sender).On;
    }
}