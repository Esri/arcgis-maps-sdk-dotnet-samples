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
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.ChangeViewpoint
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change viewpoint",
        "MapView",
        "This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.",
        "Click any of the available buttons to change the current viewpoint")]
    public partial class ChangeViewpoint
    {
        // Coordinates for London
        private MapPoint _londonCoords = new MapPoint(
            -13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private double _londonScale = 8762.7156655228955;

        // Coordinates for Redlands
        private Polygon _redlandsEnvelope = new Polygon(
            new List<MapPoint>
                {
                    new MapPoint(-13049785.1566222, 4032064.6003424),
                    new MapPoint(-13049785.1566222, 4040202.42595729),
                    new MapPoint(-13037033.5780234, 4032064.6003424),
                    new MapPoint(-13037033.5780234, 4040202.42595729)
                },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh
        private Polygon _edinburghEnvelope = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)},
            SpatialReferences.WebMercator);

        public ChangeViewpoint()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Get .Content from the selected item
            Button myButton = sender as Button;
            var selectedMapTitle = myButton.Content.ToString();

            switch (selectedMapTitle)
            {
                case "Geometry":

                    // Set Viewpoint using Redlands envelope defined above and a padding of 20
                    await MyMapView.SetViewpointGeometryAsync(_redlandsEnvelope, 20);
                    break;

                case "Center and Scale":

                    // Set Viewpoint so that it is centered on the London coordinates defined above
                    await MyMapView.SetViewpointCenterAsync(_londonCoords);

                    // Set the Viewpoint scale to match the specified scale 
                    await MyMapView.SetViewpointScaleAsync(_londonScale);
                    break;

                case "Animate":

                    // Navigate to full extent of the first baselayer before animating to specified geometry
                    await MyMapView.SetViewpointAsync(
                        new Viewpoint(MyMapView.Map.Basemap.BaseLayers.First().FullExtent));

                    // Create a new Viewpoint using the specified geometry
                    var viewpoint = new Viewpoint(_edinburghEnvelope);

                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                    await MyMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                    break;

                default:
                    break;
            }
        }
    }
}