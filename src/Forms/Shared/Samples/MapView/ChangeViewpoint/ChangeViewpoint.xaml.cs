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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ChangeViewpoint
{
    public partial class ChangeViewpoint : ContentPage
    {
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
            InitializeComponent ();

            Title = "Change viewpoint";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnViewpointsClicked(object sender, EventArgs e)
        {
            // Show sheet and get title from the selection
            var selectedMapTitle =
                await DisplayActionSheet("Select viewpoint", "Cancel", null, titles);

            // If selected cancel do nothing
            if (selectedMapTitle == "Cancel") return;

            switch (selectedMapTitle)
            {
                case "Geometry":
   
                    // Set Viewpoint using Redlands envelope defined above and a padding of 20
                    await MyMapView.SetViewpointGeometryAsync(RedlandsEnvelope, 20);
                    break;

                case "Center & Scale":
                    
                    // Set Viewpoint so that it is centered on the London coordinates defined above
                    await MyMapView.SetViewpointCenterAsync(LondonCoords);
                    
                    // Set the Viewpoint scale to match the specified scale 
                    await MyMapView.SetViewpointScaleAsync(LondonScale);
                    break;

                case "Animate":
                    
                    // Navigate to full extent of the first baselayer before animating to specified geometry
                    await MyMapView.SetViewpointAsync(
                        new Viewpoint(MyMapView.Map.Basemap.BaseLayers.First().FullExtent));
                    
                    // Create a new Viewpoint using the specified geometry
                    var viewpoint = new Viewpoint(EdinburghEnvelope);
                    
                    // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                    await MyMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                    break;

                default:
                    break;
            }
        }
    }
}
