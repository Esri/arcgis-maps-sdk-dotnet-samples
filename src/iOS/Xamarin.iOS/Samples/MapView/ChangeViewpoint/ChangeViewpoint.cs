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
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeViewpoint
{
    [Register("ChangeViewpoint")]
    public class ChangeViewpoint : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Coordinates for London
        private MapPoint LondonCoords = new MapPoint(
            -13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private double LondonScale = 8762.7156655228955;

        // Coordinates for Redlands
        private Polygon RedlandsEnvelope = new Polygon(
            new List<MapPoint>
                {
                    new MapPoint(-13049785.1566222, 4032064.6003424),
                    new MapPoint(-13049785.1566222, 4040202.42595729),
                    new MapPoint(-13037033.5780234, 4032064.6003424),
                    new MapPoint(-13037033.5780234, 4040202.42595729)
                },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh
        private Polygon EdinburghEnvelope = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)},
            SpatialReferences.WebMercator);

        // String array to store titles for the viewpoints specified above.
        private string[] titles = new string[]
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

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnViewpointsButtonTouch(object sender, EventArgs e)
        {
            // Initialize an UIAlertController
            UIAlertController viewpointAlert = UIAlertController.Create(
                "Select viewpoint", "", UIAlertControllerStyle.Alert);

            // Add actions to alert. Selecting an option set the new viewpoint
            viewpointAlert.AddAction(UIAlertAction.Create(titles[0], UIAlertActionStyle.Default, 
                async (action) =>
                {
                    // Set Viewpoint using Redlands envelope defined above and a padding of 20
                    await _myMapView.SetViewpointGeometryAsync(RedlandsEnvelope, 20);
                }));
            viewpointAlert.AddAction(UIAlertAction.Create(titles[1], UIAlertActionStyle.Default, 
                async (action) =>
                {
                    // Set Viewpoint so that it is centered on the London coordinates defined above
                    await _myMapView.SetViewpointCenterAsync(LondonCoords);
            
                    // Set the Viewpoint scale to match the specified scale 
                    await _myMapView.SetViewpointScaleAsync(LondonScale);
                }));
            viewpointAlert.AddAction(UIAlertAction.Create(titles[2], UIAlertActionStyle.Default, 
                async (action) =>
                {
                    // Navigate to full extent of the first baselayer before animating to specified geometry
                    await _myMapView.SetViewpointAsync(
                        new Viewpoint(_myMapView.Map.Basemap.BaseLayers.First().FullExtent));
                    
                    // Create a new Viewpoint using the specified geometry
                    var viewpoint = new Viewpoint(EdinburghEnvelope);
                    
                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                    await _myMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                }));
            PresentViewController(viewpointAlert, true, null);
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(
                0, yPageOffset, View.Bounds.Width, View.Bounds.Height - yPageOffset);

            // Add a button at the bottom to show viewpoint choices
            UIButton viewpointsButton = new UIButton(UIButtonType.Custom)
            {
                Frame = new CoreGraphics.CGRect(
                    0, View.Bounds.Height - 40, View.Bounds.Width, 40),
                BackgroundColor = UIColor.White
            };

            // Create button to show map options
            viewpointsButton.SetTitle("Viewpoints", UIControlState.Normal);
            viewpointsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            viewpointsButton.TouchUpInside += OnViewpointsButtonTouch;

            // Add MapView to the page
            View.AddSubviews(_myMapView, viewpointsButton);
        }
    }
}