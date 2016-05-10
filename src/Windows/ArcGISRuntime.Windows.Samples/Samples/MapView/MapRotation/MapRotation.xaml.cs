// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
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

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
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