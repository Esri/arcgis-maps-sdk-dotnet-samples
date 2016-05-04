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

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Windows;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Windows.Samples.ChangeViewpoint
{
    public partial class ChangeViewpoint
    {
        private MapPoint _londonCoords = new MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);
        private double _londonScale = 8762.7156655228955;
        private Polygon _redlandsEnvelope = new Polygon(new List<MapPoint> {
            (new MapPoint(-13049785.1566222, 4032064.6003424)),
            (new MapPoint(-13049785.1566222, 4040202.42595729)),
            (new MapPoint(-13037033.5780234, 4032064.6003424)),
            (new MapPoint(-13037033.5780234, 4040202.42595729))},
            SpatialReferences.WebMercator);
        private Polygon _edinburghEnvelope = new Polygon(new List<MapPoint> {
            (new MapPoint(-354262.156621384, 7548092.94093301)),
            (new MapPoint(-354262.156621384, 7548901.50684376)),
            (new MapPoint(-353039.164455303, 7548092.94093301)),
            (new MapPoint(-353039.164455303, 7548901.50684376))},
            SpatialReferences.WebMercator);

        MessageDialog errorDialog = null;


        public ChangeViewpoint()
        {
            InitializeComponent();

            var myMap = new Map();

            var baseLayer = new ArcGISTiledLayer(
                new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer"));
            myMap.Basemap.BaseLayers.Add(baseLayer);
            MyMapView.Map = myMap;
        }

        private async void OnAnimateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Return to initial viewpoint so Animation curve can be demonstrated clearly. 
                await MyMapView.SetViewpointAsync(MyMapView.Map.InitialViewpoint);
                var viewpoint = new Viewpoint(_edinburghEnvelope);
                // Animates the changing of the viewpoint giving a smooth transition from the old to the new view.
                await MyMapView.SetViewpointAsync(viewpoint, System.TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                errorDialog = new MessageDialog(errorMessage, "Sample error");
            }
            if (errorDialog != null)
                await errorDialog.ShowAsync();
        }

        private async void OnGeometryButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Sets the viewpoint extent to the provide bounding geometry   
                MyMapView.SetViewpointGeometryAsync(_redlandsEnvelope);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                errorDialog = new MessageDialog(errorMessage, "Sample error");
            }
            if (errorDialog != null)
                await errorDialog.ShowAsync();
        }

        private async void OnCentreScaleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Centers the viewpoint on the provided map point 
                MyMapView.SetViewpointCenterAsync(_londonCoords);
                //Sets the viewpoint's zoom scale to the provided double value  
                MyMapView.SetViewpointScaleAsync(_londonScale);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                errorDialog = new MessageDialog(errorMessage, "Sample error");
            }
            if (errorDialog != null)
                await errorDialog.ShowAsync();
        }

        private async void OnRotateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Gets the current rotation value of the map view
                var currentRotation = MyMapView.MapRotation;
                //Rotate the viewpoint by the given number of degrees. In this case the current rotation value 
                //plus 90 is passed, this will result in a the map rotating 90 degrees anti-clockwise  
                await MyMapView.SetViewpointRotationAsync(currentRotation + 90.00);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                errorDialog = new MessageDialog(errorMessage, "Sample error");
            }
            if (errorDialog != null)
                await errorDialog.ShowAsync();
        }
    }
}