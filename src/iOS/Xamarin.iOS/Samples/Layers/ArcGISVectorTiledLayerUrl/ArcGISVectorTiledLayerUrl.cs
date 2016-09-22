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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ArcGISVectorTiledLayerUrl
{
    [Register("ArcGISVectorTiledLayerUrl")]
    public class ArcGISVectorTiledLayerUrl : UIViewController
    {
         // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        private string _navigationUrl = "http://www.arcgis.com/sharing/rest/content/items/00cd8e843bae49b3a040423e5d65416b/resources/styles/root.json";
        private string _streetUrl = "http://www.arcgis.com/sharing/rest/content/items/3b8814f6ddbd485cae67e8018992246e/resources/styles/root.json";
        private string _nightUrl = "http://www.arcgis.com/sharing/rest/content/items/f96366254a564adda1dc468b447ed956/resources/styles/root.json";
        private string _topographicUrl = "http://www.arcgis.com/sharing/rest/content/items/be44936bcdd24db588a1ae5076e36f34/resources/styles/root.json";

        private string _vectorTiledLayerUrl;
        private ArcGISVectorTiledLayer _vectorTiledLayer;

    public ArcGISVectorTiledLayerUrl()
        {
            Title = "ArcGIS vector tiled layer (URL)";
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

            // Create a new ArcGISVectorTiledLayer with the navigation serice Url
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_navigationUrl));

            myMap.Basemap = new Basemap(_vectorTiledLayer);

            // Assign the Map to the MapView
            myMapView.Map = myMap;

            // Create a segmented control to display buttons
            UISegmentedControl segmentControl = new UISegmentedControl();
            segmentControl.Frame = new CoreGraphics.CGRect(10, 8, View.Bounds.Width - 20, 24);
            segmentControl.InsertSegment("Topo", 0, false);
            segmentControl.InsertSegment("Streets", 1, false);
            segmentControl.InsertSegment("Night", 2, false);
            segmentControl.InsertSegment("Navigation", 3, false);

            segmentControl.SelectedSegment = 0;

            segmentControl.ValueChanged += (sender, e) =>
            {
                var selectedSegmentId = (sender as UISegmentedControl).SelectedSegment;

                switch (selectedSegmentId)
                {
                    case 0:
 
                        _vectorTiledLayerUrl = _topographicUrl;
                        break;

                    case 1:

                        _vectorTiledLayerUrl = _streetUrl;
                        break;

                    case 2:

                        _vectorTiledLayerUrl = _nightUrl;
                        break;

                    case 3:

                        _vectorTiledLayerUrl = _navigationUrl;
                        break;
                }

                // Create a new ArcGISVectorTiledLayer with the Url Selected by the user
                _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_vectorTiledLayerUrl));

                // Create new Map with basemap and assigning to the Mapviews Map
                myMapView.Map = new Map(new Basemap(_vectorTiledLayer));
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
