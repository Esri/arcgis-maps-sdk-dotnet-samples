// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Windows.UI.Xaml.Controls.Primitives;

namespace ArcGISRuntime.UWP.Samples.MapRotation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map rotation",
        "MapView",
        "This sample demonstrates how to rotate a map.",
        "Use the available slider to dictate the number of degrees the map is rotated")]
    public partial class MapRotation
    {
        public MapRotation()
        {
            InitializeComponent();

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
    
        private void MySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Display the rotation value in the Label formatted nicely with degree symbol.
            MyTextBlock.Text = string.Format("{0:0}°", MySlider.Value);
           
            // Set the MapView rotation to that of the Slider.
            MyMapView.SetViewpointRotationAsync(e.NewValue);           
        }
    }
}