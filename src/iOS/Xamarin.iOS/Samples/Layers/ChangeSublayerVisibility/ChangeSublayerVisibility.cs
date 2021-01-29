// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeSublayerVisibility
{
    [Register("ChangeSublayerVisibility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map image layer sublayer visibility",
        category: "Layers",
        description: "Change the visibility of sublayers.",
        instructions: "Each sublayer has a check box which can be used to toggle the visibility of the sublayer.",
        tags: new[] { "layers", "sublayers", "visibility" })]
    public class ChangeSublayerVisibility : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private SublayersTable _sublayersTableView;
        private UIBarButtonItem _chooseLayersButton;

        // Hold a reference to the layer.
        private ArcGISMapImageLayer _mapImageLayer;

        public ChangeSublayerVisibility()
        {
            Title = "Change sublayer visibility";
        }

        private async void Initialize()
        {
            // Create a new Map instance with the basemap.
            Map map = new Map(SpatialReferences.Wgs84)
            {
                Basemap = new Basemap(BasemapStyle.ArcGISTopographic)
            };

            // Create a new ArcGISMapImageLayer instance and pass a URL to the service.
            _mapImageLayer = new ArcGISMapImageLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));

            // Assign the Map to the MapView.
            _myMapView.Map = map;

            // Create a new instance of the Sublayers Table View Controller. This View Controller
            // displays a table of sublayers with a switch for setting the layer visibility. 
            _sublayersTableView = new SublayersTable();

            try
            {
                // Await the load call for the layer.
                await _mapImageLayer.LoadAsync();

                // Add the map image layer to the map's operational layers.
                map.OperationalLayers.Add(_mapImageLayer);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void sublayerButton_Clicked(object sender, EventArgs e)
        {
            if (_mapImageLayer.Sublayers.Count > 0)
            {
                _sublayersTableView.MapImageLayer = _mapImageLayer;
                NavigationController.PushViewController(_sublayersTableView, true);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _chooseLayersButton = new UIBarButtonItem();
            _chooseLayersButton.Title = "Choose sublayers";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _chooseLayersButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _chooseLayersButton.Clicked += sublayerButton_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _chooseLayersButton.Clicked -= sublayerButton_Clicked;
        }
    }

    [Register("SublayersTable")]
    public sealed class SublayersTable : UITableViewController
    {
        public ArcGISMapImageLayer MapImageLayer { private get; set; }

        public SublayersTable()
        {
            Title = "Sublayers";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            List<ArcGISSublayer> sublayers = new List<ArcGISSublayer>();

            // Add all the map image layer sublayers to a list and then pass that list to the SublayerDataSource.
            if (MapImageLayer != null)
            {
                sublayers.AddRange(MapImageLayer.Sublayers);

                TableView.Source = new SublayerDataSource(sublayers);
                TableView.Frame = new CGRect(0, 0, 100, 100);
            }
        }
    }

    public class SublayerDataSource : UITableViewSource
    {
        private readonly List<ArcGISSublayer> _sublayers;
        private UISwitch _visibilitySwitch;

        private const string CellId = "cellid";

        public SublayerDataSource(List<ArcGISSublayer> sublayers)
        {
            _sublayers = sublayers;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create the cells in the table.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);

            ArcGISMapImageSublayer sublayer = (ArcGISMapImageSublayer) _sublayers[indexPath.Row];
            cell.TextLabel.Text = sublayer.Name;

            // Create a UISwitch for controlling the layer visibility.
            _visibilitySwitch = new UISwitch
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height),
                Tag = indexPath.Row,
                On = sublayer.IsVisible
            };
            _visibilitySwitch.ValueChanged += VisibilitySwitch_ValueChanged;

            // Add the UISwitch to the cell's content view.
            cell.ContentView.AddSubview(_visibilitySwitch);

            return cell;
        }

        private void VisibilitySwitch_ValueChanged(object sender, EventArgs e)
        {
            // Get the row containing the UISwitch that was changed.
            var index = ((UISwitch) sender).Tag;

            // Set the sublayer visibility according to the UISwitch setting.
            ArcGISMapImageSublayer sublayer = (ArcGISMapImageSublayer) _sublayers[(int) index];
            sublayer.IsVisible = ((UISwitch) sender).On;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _sublayers.Count;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // Unsubscribe from events, per best practice.
            BeginInvokeOnMainThread(() => _visibilitySwitch.ValueChanged -= VisibilitySwitch_ValueChanged);
        }
    }
}