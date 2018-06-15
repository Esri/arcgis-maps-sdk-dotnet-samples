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
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeViewpoint
{
    [Register("ChangeViewpoint")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change viewpoint",
        "MapView",
        "This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.",
        "")]
    public class ChangeViewpoint : UIViewController
    {
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UISegmentedControl _viewpointsButton;

        // Create and hold reference to the used MapView
        private readonly MapView _myMapView = new MapView();

        // Coordinates for London
        private readonly MapPoint _londonCoords = new MapPoint(
            -13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private readonly double LondonScale = 8762.7156655228955;

        // Coordinates for Redlands
        private readonly Polygon _redlandsEnvelope = new Polygon(
            new List<MapPoint>
                {
                    new MapPoint(-13049785.1566222, 4032064.6003424),
                    new MapPoint(-13049785.1566222, 4040202.42595729),
                    new MapPoint(-13037033.5780234, 4032064.6003424),
                    new MapPoint(-13037033.5780234, 4040202.42595729)
                },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh
        private readonly Polygon _edinburghEnvelope = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)},
            SpatialReferences.WebMercator);

        // String array to store titles for the viewpoints specified above.
        private readonly string[] _titles = 
        {
            "Geometry",
            "Center & Scale",
            "Animate"
        };

        public ChangeViewpoint()
        {
            Title = "Change viewpoint";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            int margin = 5;
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Set up the frame for the toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);

            // Setup the visual frame for the Button
            _viewpointsButton.Frame = new CoreGraphics.CGRect(margin, View.Bounds.Height - 40 + margin, View.Bounds.Width - (2 * margin), 40 - (2 * margin));

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Add a button at the bottom to show viewpoint choices
            _viewpointsButton = new UISegmentedControl(_titles)
            {
                TintColor = View.TintColor
            };

            // Create button to show map options
            _viewpointsButton.ValueChanged += viewpointButton_ValueChanged;

            // Add MapView to the page
            View.AddSubviews(_myMapView, _toolbar, _viewpointsButton);
        }

        private async void viewpointButton_ValueChanged(object sender, EventArgs e)
        {
            nint selectedValue = ((UISegmentedControl)sender).SelectedSegment;

            switch (selectedValue){
                case 0:
                    // Set Viewpoint using Redlands envelope defined above and a padding of 20
                    await _myMapView.SetViewpointGeometryAsync(_redlandsEnvelope, 20);
                    break;
                case 1:
                    // Set Viewpoint so that it is centered on the London coordinates defined above
                    await _myMapView.SetViewpointCenterAsync(_londonCoords);

                    // Set the Viewpoint scale to match the specified scale 
                    await _myMapView.SetViewpointScaleAsync(LondonScale);
                    break;
                case 2:
                    // Navigate to full extent of the first baselayer before animating to specified geometry
                    await _myMapView.SetViewpointAsync(
                        new Viewpoint(_myMapView.Map.Basemap.BaseLayers.First().FullExtent));

                    // Create a new Viewpoint using the specified geometry
                    var viewpoint = new Viewpoint(_edinburghEnvelope);

                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                    await _myMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                    break;
            }
        }

    }
}