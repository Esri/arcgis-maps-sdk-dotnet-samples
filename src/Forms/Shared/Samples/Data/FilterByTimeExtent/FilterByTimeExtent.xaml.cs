// Copyright 2022 Esri.
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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.FilterByTimeExtent
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Filter by time extent",
        category: "Data",
        description: "The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.",
        instructions: "Use the slider at the bottom of the map to customize the date range for which you want to view the data. The date for the hurricanes sample data ranges from September 1st, 2005 to December 31st, 2005. Once the start and end dates have been selected, the map view will update to only show the relevant data that falls in the time extent selected. Use the play button to step through the data one day at a time. Use the previous and next buttons to go back and forth in 2 day increments as demonstrated below.",
        tags: new[] { "animate", "data", "filter", "time", "time extent", "time frame", "toolkit" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("49925d814d7e40fb8fa64864ef62d55e")]
    public partial class FilterByTimeExtent : ContentPage
    {
        public FilterByTimeExtent()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}
