//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ArcGISRuntime.Desktop.Samples.ChangeViewpoint
{
    public partial class ChangeViewpoint
    {
        private MapPoint LondonCoords = new MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private double LondonScale = 8762.7156655228955;
        private Polygon EdinburghEnvelope = new Polygon(new List<MapPoint> {
            (new MapPoint(-13049785.1566222, 4032064.6003424)),
            (new MapPoint(-13049785.1566222, 4040202.42595729)),
            (new MapPoint(-13037033.5780234, 4032064.6003424)),
            (new MapPoint(-13037033.5780234, 4040202.42595729))},
            SpatialReferences.WebMercator);
        private Polygon RedlandsEnvelope = new Polygon(new List<MapPoint> {
            (new MapPoint(-354262.156621384, 7548092.94093301)),
            (new MapPoint(-354262.156621384, 7548901.50684376)),
            (new MapPoint(-353039.164455303, 7548092.94093301)),
            (new MapPoint(-353039.164455303, 7548901.50684376))},
            SpatialReferences.WebMercator);
        

        public ChangeViewpoint()
        {
            InitializeComponent(); 
        }

        private void OnAnimateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewpoint = new Esri.ArcGISRuntime.Viewpoint(EdinburghEnvelope);
                //Animates the changing of the viewpoint giving a smooth transition from the old to the new view
                MyMapView.SetViewpointAsync(viewpoint, System.TimeSpan.FromSeconds(5));
            }
            catch(Exception ex)
            {
                var errorMessage = "Viewpoint could not be set. "+ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }         
        }

        private void OnGeomtryButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Sets the viewpoint extent to the provide bounding geometry   
                MyMapView.SetViewpointGeometryAsync(RedlandsEnvelope);
            }
            catch(Exception ex)
            {
                var errorMessage = "Viewpoint could not be set. " + ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }           
        }

        private void OnCentreScaleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Centers the viewpoint on the provided map point 
                MyMapView.SetViewpointCenterAsync(LondonCoords);
                //Sets the viewpoint's zoom scale to the provided double value  
                MyMapView.SetViewpointScaleAsync(LondonScale);
            }
            catch(Exception ex)
            {
                var errorMessage = "Viewpoint could not be set. " + ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }          
        }

        private async void OnRotateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Gets the current rotation value of the map view
                var currentRotation = MyMapView.Rotation;
                //Rotate the viewpoint by the given number of degrees, In this case the current rotation value 
                //plus 90 is passed, this will result in a the map rotating 90 degrees anti-clockwise  
                await MyMapView.SetViewpointRotationAsync(currentRotation + 90.00);
            }
            catch(Exception ex)
            {
                var errorMessage = "Viewpoint could not be set. " + ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }         
        }
    }
}