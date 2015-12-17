﻿//Copyright 2015 Esri.
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
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls.Primitives;

namespace ArcGISRuntime.Windows.Samples.MapRotation
{
    public partial class MapRotation
    {
        public MapRotation()
        {
            InitializeComponent();
            // TODO Move this to XAML
            var myMap = new Map();

            var baseLayer = new ArcGISTiledLayer(
                new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer"));
            myMap.Basemap.BaseLayers.Add(baseLayer);
            MyMapView.Map = myMap;
        }

        private async void OnDegreeSliderChange(object sender, RangeBaseValueChangedEventArgs e)
        {
            MessageDialog errorDialog = null;
            try
            {
                //Set Viewpoint's rotation to that of the slider value
                await MyMapView.SetViewpointRotationAsync(degreeSlider.Value);
            }
            catch (Exception ex)
            {
                var errorMessage = "MapView Viewpoint could not be rotated. " + ex.Message;
                errorDialog = new MessageDialog(errorMessage, "Sample error");
            }
            if (errorDialog != null)
                await errorDialog.ShowAsync();
        }
    }
}