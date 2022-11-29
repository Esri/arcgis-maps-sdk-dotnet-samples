// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using System.Threading;

namespace ArcGIS.WPF.Samples.AccessLoadStatus
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Map load status",
        category: "Map",
        description: "Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.",
        instructions: "The load status of the map will be displayed as the sample loads.",
        tags: new[] { "load status", "loadable pattern", "map" })]
    public partial class AccessLoadStatus
    {
        public AccessLoadStatus()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Register to handle loading status changes
            myMap.LoadStatusChanged += OnMapsLoadStatusChanged;

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnMapsLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // Update the load status information
            Dispatcher.BeginInvoke(
                new ThreadStart(() =>
                LoadStatusLabel.Content = string.Format("Map's load status : {0}", e.Status.ToString())
                ));
        }
    }
}