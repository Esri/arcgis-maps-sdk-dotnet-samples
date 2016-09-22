// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.ChangeBasemap
{
    public partial class ChangeBasemap
    {
        // String array to store titles for the viewpoints specified above.
        private string[] titles = new string[]
        {
            "Topo",
            "Streets",
            "Imagery",
            "Ocean"
        };

        public ChangeBasemap()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set titles as a items source
            basemapChooser.ItemsSource = titles;
        }

        private void OnBasemapListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBasemap = e.AddedItems[0].ToString();

            switch (selectedBasemap)
            {
                case "Topo":
                    // Set the basemap to Topographic
                    MyMapView.Map.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    MyMapView.Map.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    MyMapView.Map.Basemap = Basemap.CreateImagery();
                    break;
                case "Ocean":
                    // Set the basemap to Oceans
                    MyMapView.Map.Basemap = Basemap.CreateOceans();
                    break;
                default:
                    break;
            }
        }
    }
}
