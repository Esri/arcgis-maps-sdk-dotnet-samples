// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace ArcGIS.WinUI.Samples.DisplayOverviewMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Display Overview Map",
        "Map",
        "Include an overview or inset map as an additional map view to show the wider context of the primary view.",
        "Pan or zoom across the map view to browse through the tourist attractions feature layer and notice the viewpoint and scale of the linked overview map update automatically. When running the sample on a desktop, you can also navigate by panning and zooming on the overview map. However, interactivity of the overview map is disabled on mobile devices.")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayOverviewMap
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://services6.arcgis.com/Do88DoK2xjTUCXd1/arcgis/rest/services/OSM_Tourism_NA/FeatureServer/0";

        // Hold a reference to the feature table.
        private ServiceFeatureTable _featureTable;


        public DisplayOverviewMap()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new Map with basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Set the initial map location.
            MyMapView.SetViewpoint(new Viewpoint(49.28299, -123.12052, 70000));

            // Create the feature table, referring to the feature service.
            _featureTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            FeatureLayer featureLayer = new FeatureLayer(_featureTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(featureLayer);

        }
    }
}