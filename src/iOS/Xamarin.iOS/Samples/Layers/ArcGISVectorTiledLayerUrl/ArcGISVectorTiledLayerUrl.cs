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

        private UIToolbar _toolbar = new UIToolbar();
        private UISegmentedControl _segmentControl = new UISegmentedControl();

        private string _navigationUrl = "https://www.arcgis.com/home/item.html?id=dcbbba0edf094eaa81af19298b9c6247";
        private string _streetUrl = "https://www.arcgis.com/home/item.html?id=4e1133c28ac04cca97693cf336cd49ad";
        private string _nightUrl = "https://www.arcgis.com/home/item.html?id=bf79e422e9454565ae0cbe9553cf6471";
        private string _darkGrayUrl = "https://www.arcgis.com/home/item.html?id=850db44b9eb845d3bd42b19e8aa7a024";

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

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            // Create a new Map instance with the basemap
            Map myMap = new Map(SpatialReferences.WebMercator);

            // Create a new ArcGISVectorTiledLayer with the navigation service Url
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_navigationUrl));

            myMap.Basemap = new Basemap(_vectorTiledLayer);

            // Assign the Map to the MapView
            _myMapView.Map = myMap;

            // Update the segmented control to display buttons
            _segmentControl.InsertSegment("Dark gray", 0, false);
            _segmentControl.InsertSegment("Streets", 1, false);
            _segmentControl.InsertSegment("Night", 2, false);
            _segmentControl.InsertSegment("Navigation", 3, false);

            _segmentControl.SelectedSegment = 3;

            _segmentControl.ValueChanged += (sender, e) =>
            {
                var selectedSegmentId = (sender as UISegmentedControl).SelectedSegment;

                switch (selectedSegmentId)
                {
                    case 0:

                        _vectorTiledLayerUrl = _darkGrayUrl;
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

                // Create new Map with basemap and assigning to the MapView's Map
                _myMapView.Map = new Map(new Basemap(_vectorTiledLayer));
            };

            View.AddSubviews(_myMapView, _toolbar, _segmentControl);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);

            _segmentControl.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width - 20, 30);

            base.ViewDidLayoutSubviews();
        }
    }
}