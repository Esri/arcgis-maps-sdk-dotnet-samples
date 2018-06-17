// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeSublayerVisibility
{
    [Register("ChangeSublayerVisibility")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change sublayer visibility",
        "Layers",
        "This sample demonstrates how to show or hide sublayers of a map image layer.",
        "")]
    public class ChangeSublayerVisibility : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIButton _sublayersButton = new UIButton(UIButtonType.Custom);

        public ChangeSublayerVisibility()
        {
            Title = "Change sublayer visibility";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            _sublayersButton.BackgroundColor = UIColor.White;
            _sublayersButton.SetTitle("Sublayers", UIControlState.Normal);
            _sublayersButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _sublayersButton);
        }

        private async void Initialize()
        {
            // Create a new ArcGISMapImageLayer instance and pass a URL to the service.
            var mapImageLayer = new ArcGISMapImageLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));

            // Await the load call for the layer..
            await mapImageLayer.LoadAsync();

            // Create a new Map instance with the basemap..
            Map map = new Map(SpatialReferences.Wgs84)
            {
                Basemap = Basemap.CreateTopographic()
            };

            // Add the map image layer to the map's operational layers.
            map.OperationalLayers.Add(mapImageLayer);

            // Assign the Map to the MapView.
            _myMapView.Map = map;

            // Create a new instance of the Sublayers Table View Controller. This View Controller
            // displays a table of sublayers with a switch for setting the layer visibility. 
            SublayersTable sublayersTableView = new SublayersTable();

            // When the sublayers button is clicked, load the Sublayer Table View Controller.
            _sublayersButton.TouchUpInside += (s, e) =>
            {
                if (mapImageLayer.Sublayers.Count > 0)
                {
                    sublayersTableView.MapImageLayer = mapImageLayer;
                    NavigationController.PushViewController(sublayersTableView, true);
                }
            };
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat barHeight = 40;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, barHeight, 0);
                _sublayersButton.Frame = new CGRect(0, _myMapView.Bounds.Height - barHeight, View.Bounds.Width, barHeight);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }
    }

    [Register("SublayersTable")]
    public sealed class SublayersTable : UITableViewController
    {
        public ArcGISMapImageLayer MapImageLayer;

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

        private static readonly string CellId = "cellid";

        public SublayerDataSource(List<ArcGISSublayer> sublayers)
        {
            _sublayers = sublayers;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create the cells in the table.
            var cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);

            var sublayer = _sublayers[indexPath.Row] as ArcGISMapImageSublayer;
            cell.TextLabel.Text = sublayer.Name;

            // Create a UISwitch for controlling the layer visibility.
            var visibilitySwitch = new UISwitch
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height),
                Tag = indexPath.Row,
                On = sublayer.IsVisible
            };
            visibilitySwitch.ValueChanged += VisibilitySwitch_ValueChanged;

            // Add the UISwitch to the cell's content view.
            cell.ContentView.AddSubview(visibilitySwitch);

            return cell;
        }

        private void VisibilitySwitch_ValueChanged(object sender, EventArgs e)
        {
            // Get the row containing the UISwitch that was changed.
            var index = (sender as UISwitch).Tag;

            // Set the sublayer visibility according to the UISwitch setting.
            var sublayer = _sublayers[(int) index] as ArcGISMapImageSublayer;
            sublayer.IsVisible = (sender as UISwitch).On;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _sublayers.Count;
        }
    }
}