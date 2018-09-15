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
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeViewpoint
{
    [Register("ChangeViewpoint")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change viewpoint",
        "MapView",
        "This sample demonstrates different ways in which you can change the viewpoint of the MapView.",
        "")]
    public class ChangeViewpoint : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView = new MapView();
        private UISegmentedControl _viewpointsButton;

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

        public override void LoadView()
        {
            // Create the views.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            _viewpointsButton = new UISegmentedControl("Geometry", "Center & Scale", "Animate")
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            // Clean up borders of segmented control - avoid corner pixels.
            _viewpointsButton.ClipsToBounds = true;
            _viewpointsButton.Layer.CornerRadius = 5;

            _viewpointsButton.ValueChanged += ViewpointButton_ValueChanged;

            // Add the views.
            View = new UIView();
            View.AddSubviews(_myMapView, _viewpointsButton);

            // Apply constraints.
            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _viewpointsButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor).Active = true;
            _viewpointsButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor).Active = true;
            _viewpointsButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());
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
                    Viewpoint viewpoint = new Viewpoint(_edinburghEnvelope);

                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds.
                    await _myMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                    break;
            }

            // Reset the segment button.
            _viewpointsButton.SelectedSegment = -1;
        }
    }
}