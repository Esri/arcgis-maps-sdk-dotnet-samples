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
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.ChangeViewpoint
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Change viewpoint",
        category: "MapView",
        description: "Set the map view to a new viewpoint.",
        instructions: "The map view has several methods for setting its current viewpoint. Select a viewpoint from the UI to see the viewpoint changed using that method.",
        tags: new[] { "animate", "extent", "pan", "rotate", "scale", "view", "zoom" })]
    public partial class ChangeViewpoint
    {
        // Coordinates for London
        private MapPoint _londonCoords = new MapPoint(
            -13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);

        private double _londonScale = 8762.7156655228955;

        // Coordinates for Redlands
        private Polygon _redlandsEnvelope = new Polygon(new List<MapPoint>
            {
                new MapPoint(-13049785.1566222, 4032064.6003424),
                new MapPoint(-13049785.1566222, 4040202.42595729),
                new MapPoint(-13037033.5780234, 4032064.6003424),
                new MapPoint(-13037033.5780234, 4040202.42595729)
            },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh
        private Polygon _edinburghEnvelope = new Polygon(new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)
            },
            SpatialReferences.WebMercator);

        public ChangeViewpoint()
        {
            InitializeComponent();
        }

        private async void OnButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get content from the selected item
                Button myButton = (Button)sender;

                switch (myButton.Content.ToString())
                {
                    case "Geometry":

                        // Set Viewpoint using Redlands envelope defined above and a padding of 20
                        await MyMapView.SetViewpointGeometryAsync(_redlandsEnvelope, 20);
                        break;

                    case "Center and scale":

                        // Set Viewpoint so that it is centered on the London coordinates defined above
                        await MyMapView.SetViewpointCenterAsync(_londonCoords);

                        // Set the Viewpoint scale to match the specified scale
                        await MyMapView.SetViewpointScaleAsync(_londonScale);
                        break;

                    case "Animate":

                        // Navigate to full extent of the first baselayer before animating to specified geometry
                        await MyMapView.SetViewpointAsync(
                            new Viewpoint(Basemap.FullExtent));

                        // Create a new Viewpoint using the specified geometry
                        Viewpoint viewpoint = new Viewpoint(_edinburghEnvelope);

                        // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                        await MyMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}