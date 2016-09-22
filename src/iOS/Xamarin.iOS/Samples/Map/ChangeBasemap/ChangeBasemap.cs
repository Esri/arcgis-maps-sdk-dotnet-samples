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
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeBasemap
{
    [Register("ChangeBasemap")]
    public class ChangeBasemap : UIViewController
    {
        public ChangeBasemap()
        {
            Title = "Change basemap";
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview
            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a variable to hold the yOffset where the MapView control should start
            var yOffset = 60;

            // Create a new MapView control and provide its location coordinates on the frame
            MapView myMapView = new MapView();
            myMapView.Frame = new CoreGraphics.CGRect(0, yOffset, View.Bounds.Width, View.Bounds.Height - yOffset);

            // Create a new Map instance with the basemap               
            Map myMap = new Map(SpatialReferences.WebMercator);
            myMap.Basemap = Basemap.CreateTopographic();

            // Assign the Map to the MapView
            myMapView.Map = myMap;

            // Create a segmented control to display buttons
            UISegmentedControl segmentControl = new UISegmentedControl();
            segmentControl.Frame = new CoreGraphics.CGRect(10, 8, View.Bounds.Width - 20, 24);
            segmentControl.InsertSegment("Topo", 0, false);
            segmentControl.InsertSegment("Streets", 1, false);
            segmentControl.InsertSegment("Imagery", 2, false);
            segmentControl.InsertSegment("Ocean", 3, false);

            segmentControl.SelectedSegment = 0;

            segmentControl.ValueChanged += (sender, e) =>
            {
                var selectedSegmentId = (sender as UISegmentedControl).SelectedSegment;

                switch (selectedSegmentId)
                {
                    case 0:

                        // Set the basemap to Topographic
                        myMapView.Map.Basemap = Basemap.CreateTopographic();
                        break;

                    case 1:
                    
                        // Set the basemap to Streets
                        myMapView.Map.Basemap = Basemap.CreateStreets();
                        break;

                    case 2:
                    
                        // Set the basemap to Imagery
                        myMapView.Map.Basemap = Basemap.CreateImagery();
                        break;

                    case 3:
                    
                        // Set the basemap to Oceans
                        myMapView.Map.Basemap = Basemap.CreateOceans();
                        break;
                }
            };

            // Create a UIBarButtonItem where its view is the SegmentControl
            UIBarButtonItem barButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            barButtonItem.CustomView = segmentControl;

            // Create a toolbar on the bottom of the display 
            UIToolbar toolbar = new UIToolbar();
            toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, View.Bounds.Height);
            toolbar.AutosizesSubviews = true;

            // Add the bar button item to an array of UIBarButtonItems
            UIBarButtonItem[] barButtonItems = new UIBarButtonItem[] { barButtonItem };

            // Add the UIBarButtonItems array to the toolbar
            toolbar.SetItems(barButtonItems, true);

            View.AddSubviews(myMapView, toolbar);
        }
    }
}