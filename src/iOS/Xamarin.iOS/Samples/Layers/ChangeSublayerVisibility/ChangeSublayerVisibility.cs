// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeSublayerVisibility
{
    [Register("ChangeSublayerVisibility")]
    public class ChangeSublayerVisibility : UIViewController
    {
        public ChangeSublayerVisibility()
        {
            Title = "Change sublayer visibility";
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new ArcGISMapImageLayer instance and pass a Url to the service
            var mapImageLayer = new ArcGISMapImageLayer(
                new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));

            // Await the load call for the layer.
            await mapImageLayer.LoadAsync();

            // Create a new Map instance with the basemap               
            Map myMap = new Map(SpatialReferences.Wgs84);
            myMap.Basemap = Basemap.CreateTopographic();

            // Add the map image layer to the map's operational layers
            myMap.OperationalLayers.Add(mapImageLayer);

            // Create a new MapView control and provide its location coordinates on the frame.
            MapView myMapView = new MapView();
            myMapView.Frame = new CoreGraphics.CGRect(0, 60, View.Bounds.Width, View.Bounds.Height - 40);

            // Assign the Map to the MapView
            myMapView.Map = myMap;

            // Create a button to show sublayers
            UIButton sublayersButton = new UIButton(UIButtonType.Custom);
            sublayersButton.Frame = new CoreGraphics.CGRect(0, myMapView.Bounds.Height, View.Bounds.Width, 40);
            sublayersButton.BackgroundColor = UIColor.White;
            sublayersButton.SetTitle("Sublayers", UIControlState.Normal);
            sublayersButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Create a new instance of the Sublayers Table View Controller. This View Controller
            // displays a table of sublayers with a switch for setting the layer visibility. 
            SublayersTable sublayersTableView = new SublayersTable();

            // When the sublayers button is clicked, load the Sublayers Table View Controller
            sublayersButton.TouchUpInside += (s, e) =>
            {
                if (mapImageLayer.Sublayers.Count > 0)
                {
                    sublayersTableView.mapImageLayer = mapImageLayer;
                    this.NavigationController.PushViewController(sublayersTableView, true);
                }
            };

            // Add the MapView and Sublayers button to the View
            View.AddSubviews(myMapView, sublayersButton);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }
    }

    [Register("SublayersTable")]
    public class SublayersTable : UITableViewController
    {
        public ArcGISMapImageLayer mapImageLayer;

        public SublayersTable()
        {
            Title = "Sublayers";
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            List<ArcGISSublayer> sublayers = new List<ArcGISSublayer>();

            // Add all the map image layer sublayers to a list and then pass that list to the SublayerDataSource
            if (mapImageLayer != null)
            {
                foreach (var item in mapImageLayer.Sublayers)
                {
                    sublayers.Add(item);
                }

                TableView.Source = new SublayerDataSource(sublayers);
                TableView.Frame = new CoreGraphics.CGRect(0, 0, 100, 100);
            }
        }
    }

    public class SublayerDataSource : UITableViewSource
    {
        private List<ArcGISSublayer> sublayers;

        static string CELL_ID = "cellid";

        public SublayerDataSource(List<ArcGISSublayer> sublayers)
        {
            this.sublayers = sublayers;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create the cells in the table
            var cell = new UITableViewCell(UITableViewCellStyle.Default, CELL_ID);

            var sublayer = sublayers[indexPath.Row] as ArcGISMapImageSublayer;
            cell.TextLabel.Text = sublayer.Name;

            // Create a UISwitch for controlling the layer visibility
            var visibilitySwitch = new UISwitch()
            {
                Frame = new CoreGraphics.CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height)
            };
            visibilitySwitch.Tag = indexPath.Row;
            visibilitySwitch.On = sublayer.IsVisible;
            visibilitySwitch.ValueChanged += VisibilitySwitch_ValueChanged;

            // Add the UISwitch to the cell's content view
            cell.ContentView.AddSubview(visibilitySwitch);

            return cell;
        }

        private void VisibilitySwitch_ValueChanged(object sender, EventArgs e)
        {
            // Get the row containing the UISwitch that was changed
            var index = (sender as UISwitch).Tag;

            // Set the sublayer visibility according to the UISwitch setting
            var sublayer = sublayers[(int)index] as ArcGISMapImageSublayer;
            sublayer.IsVisible = (sender as UISwitch).On;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return sublayers.Count;
        }
    }
}


