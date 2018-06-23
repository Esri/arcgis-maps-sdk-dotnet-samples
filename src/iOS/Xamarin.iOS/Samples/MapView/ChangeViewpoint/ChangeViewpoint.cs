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
        // Create and hold references to the UI controls.
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly MapView _myMapView = new MapView();
        private readonly UISegmentedControl _viewpointsButton = new UISegmentedControl(new string[] {"Geometry", "Center & Scale", "Animate"});

        // Coordinates for London.
        private readonly MapPoint _londonCoords = new MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private readonly double _londonScale = 8762.7156655228955;

        // Coordinates for Redlands.
        private readonly Polygon _redlandsEnvelope = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-13049785.1566222, 4032064.6003424),
                new MapPoint(-13049785.1566222, 4040202.42595729),
                new MapPoint(-13037033.5780234, 4032064.6003424),
                new MapPoint(-13037033.5780234, 4040202.42595729)
            },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh.
        private readonly Polygon _edinburghEnvelope = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)
            },
            SpatialReferences.WebMercator);

        public ChangeViewpoint()
        {
            Title = "Change viewpoint";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the views.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _viewpointsButton.Frame = new CoreGraphics.CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());
        }

        private void CreateLayout()
        {
            // Handle button taps.
            _viewpointsButton.ValueChanged += ViewpointButton_ValueChanged;

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar, _viewpointsButton);
        }

        private async void ViewpointButton_ValueChanged(object sender, EventArgs e)
        {
            switch (_viewpointsButton.SelectedSegment)
            {
                case 0:
                    // Set Viewpoint using Redlands envelope defined above and a padding of 20.
                    await _myMapView.SetViewpointGeometryAsync(_redlandsEnvelope, 20);
                    break;
                case 1:
                    // Set Viewpoint so that it is centered on the London coordinates defined above.
                    await _myMapView.SetViewpointCenterAsync(_londonCoords);

                    // Set the Viewpoint scale to match the specified scale.
                    await _myMapView.SetViewpointScaleAsync(_londonScale);
                    break;
                case 2:
                    // Navigate to full extent of the first base layer before animating to specified geometry.
                    await _myMapView.SetViewpointAsync(new Viewpoint(_myMapView.Map.Basemap.BaseLayers.First().FullExtent));

                    // Create a new Viewpoint using the specified geometry.
                    var viewpoint = new Viewpoint(_edinburghEnvelope);

                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds.
                    await _myMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                    break;
            }

            // Reset the segment button.
            _viewpointsButton.SelectedSegment = -1;
        }
    }
}