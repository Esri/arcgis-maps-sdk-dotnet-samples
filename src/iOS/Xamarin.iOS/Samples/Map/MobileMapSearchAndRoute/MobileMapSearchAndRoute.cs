﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.MobileMapSearchAndRoute
{
    [Register("MobileMapSearchAndRoute")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Mobile map (search and route)",
        "Map",
        "Display maps and use locators to enable search and routing offline using a Mobile Map Package.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("260eb6535c824209964cf281766ebe43")]
    public class MobileMapSearchAndRoute : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableViewController _tableController;
        private UIBarButtonItem _chooseMapButton;

        // Hold references to map resources for easy access.
        private MapsViewModel _viewModel;
        private List<Map> _maps = new List<Map>();
        private LocatorTask _packageLocator;
        private TransportationNetworkDataset _networkDataset;

        // Overlays for use in visualizing routes.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _waypointOverlay;

        // Track the start and end point for route calculation.
        private MapPoint _startPoint;
        private MapPoint _endPoint;

        public MobileMapSearchAndRoute()
        {
            Title = "Mobile map (search and route)";
        }

        private async void Initialize()
        {
            // Get the path to the package on disk.
            string filePath = DataManager.GetDataFolder("260eb6535c824209964cf281766ebe43", "SanFrancisco.mmpk");

            // Open the map package.
            MobileMapPackage package = await OpenMobileMapPackage(filePath);

            // Populate the list of maps.
            foreach (Map map in package.Maps)
            {
                await map.LoadAsync();
                _maps.Add(map);
            }

            // Show the first map by default.
            _myMapView.Map = _maps.First();

            // Get the locator task from the package.
            _packageLocator = package.LocatorTask;

            // Create and add an overlay for showing a route.
            _routeOverlay = new GraphicsOverlay();
            _routeOverlay.Renderer = new SimpleRenderer
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Blue, 3)
            };
            _myMapView.GraphicsOverlays.Add(_routeOverlay);

            // Create and add an overlay for showing waypoints/stops.
            _waypointOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_waypointOverlay);

            // Configure the table view for showing maps.
            _viewModel = new MapsViewModel(_maps);
            _viewModel.MapSelected += Map_Selected;
            _tableController = new UITableViewController(UITableViewStyle.Plain);
            _tableController.TableView.Source = _viewModel;
        }

        private async Task<MobileMapPackage> OpenMobileMapPackage(string path)
        {
            // Load directly or unpack then load as needed by the map package.
            if (await MobileMapPackage.IsDirectReadSupportedAsync(path))
            {
                // Open the map package.
                MobileMapPackage package = await MobileMapPackage.OpenAsync(path);

                // Load the package.
                await package.LoadAsync();

                // Return the opened package.
                return package;
            }
            else
            {
                // Create a path for the unpacked package.
                string unpackedPath = path + "unpacked";

                // Unpack the package.
                await MobileMapPackage.UnpackAsync(path, unpackedPath);

                // Open the package.
                MobileMapPackage package = await MobileMapPackage.OpenAsync(unpackedPath);

                // Load the package.
                await package.LoadAsync();

                // Return the opened package.
                return package;
            }
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Handle routing.
            try
            {
                await ProcessRouteRequest(e.Location);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                new UIAlertView("Error", "Couldn't geocode or route.", (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async Task ShowGeocodeResult(MapPoint tappedPoint)
        {
            // Reverse geocode to get an address.
            IReadOnlyList<GeocodeResult> results = await _packageLocator.ReverseGeocodeAsync(tappedPoint);

            // Process the address into usable strings.
            string address = results.First().Label;

            // Show the address in a callout.
            _myMapView.ShowCalloutAt(tappedPoint, new CalloutDefinition(address));
        }

        private async Task ProcessRouteRequest(MapPoint tappedPoint)
        {
            // Clear any existing overlays.
            _routeOverlay.Graphics.Clear();
            _myMapView.DismissCallout();

            // Return if there is no network available for routing.
            if (_networkDataset == null)
            {
                await ShowGeocodeResult(tappedPoint);
                return;
            }

            // Set the start point if it hasn't been set.
            if (_startPoint == null)
            {
                _startPoint = tappedPoint;

                await ShowGeocodeResult(tappedPoint);

                // Show the start point.
                _waypointOverlay.Graphics.Add(await GraphicForPoint(_startPoint));

                return;
            }

            if (_endPoint == null)
            {
                await ShowGeocodeResult(tappedPoint);

                // Show the end point.
                _endPoint = tappedPoint;
                _waypointOverlay.Graphics.Add(await GraphicForPoint(_endPoint));

                // Create the route task from the local network dataset.
                RouteTask routingTask = await RouteTask.CreateAsync(_networkDataset);

                // Configure route parameters for the route between the two tapped points.
                RouteParameters routingParameters = await routingTask.CreateDefaultParametersAsync();
                List<Stop> stops = new List<Stop> {new Stop(_startPoint), new Stop(_endPoint)};
                routingParameters.SetStops(stops);

                // Get the first route result.
                RouteResult result = await routingTask.SolveRouteAsync(routingParameters);
                Route firstRoute = result.Routes.First();

                // Show the route on the map. Note that symbology for the graphics overlay is defined in Initialize().
                Polyline routeLine = firstRoute.RouteGeometry;
                _routeOverlay.Graphics.Add(new Graphic(routeLine));

                return;
            }

            // Reset graphics and route.
            _routeOverlay.Graphics.Clear();
            _waypointOverlay.Graphics.Clear();
            _startPoint = null;
            _endPoint = null;
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        private void Map_Selected(object sender, Map selectedMap)
        {
            // Clear existing overlays.
            _myMapView.DismissCallout();
            _waypointOverlay.Graphics.Clear();
            _routeOverlay.Graphics.Clear();

            try
            {
                // Show the map in the view.
                _myMapView.Map = selectedMap;

                // Get the transportation network if there is one. Will be set to null otherwise.
                _networkDataset = selectedMap.TransportationNetworks.FirstOrDefault();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                new UIAlertView("Couldn't select map", exception.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void ChangeMap_Click(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            var controller = new UINavigationController(_tableController);
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => controller.DismissViewController(true, null));
            controller.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 250);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem) sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(controller, true, null);
        }

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _chooseMapButton = new UIBarButtonItem();
            _chooseMapButton.Title = "Choose map";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _chooseMapButton
            };

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to show address or route between points if network available.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
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
            _myMapView.GeoViewTapped += MapView_Tapped;
            _chooseMapButton.Clicked += ChangeMap_Click;

            // Subscribe to events, removing any existing subscriptions.
            if (_viewModel != null)
            {
                _viewModel.MapSelected -= Map_Selected;
                _viewModel.MapSelected += Map_Selected;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapView_Tapped;
            _viewModel.MapSelected -= Map_Selected;
            _chooseMapButton.Clicked -= ChangeMap_Click;
        }
    }

    class MapsViewModel : UITableViewSource
    {
        private readonly List<Map> _maps;
        private const string CellIdentifier = "LayerTableCell";

        public MapsViewModel(List<Map> maps)
        {
            _maps = maps;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Gets a cell for the specified section and row.
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);
            Map selectedMap = _maps[indexPath.Row];
            if (selectedMap.TransportationNetworks.Any())
            {
                cell.DetailTextLabel.Text = "✔ Has network";
            }
            else
            {
                cell.DetailTextLabel.Text = "❌ No networks";
            }

            cell.TextLabel.Text = selectedMap.Item.Title;
            cell.ImageView.Image = selectedMap.Item.Thumbnail.ToImageSourceAsync().Result;
            cell.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;

            return cell;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _maps.Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Map selectedMap = _maps[indexPath.Row];

            // Notify subscribers of the new selection.
            RaiseMapSelected(selectedMap);
        }

        // Allow the app to detect when a row is selected.
        // This is an event that can be subscribed to with code like _viewModel.MapSelected += (o, map) => { };
        public delegate void MapSelectedHandler(object sender, Map map);

        public event MapSelectedHandler MapSelected;

        private void RaiseMapSelected(Map selectedMap)
        {
            MapSelected?.Invoke(this, selectedMap);
        }
    }
}