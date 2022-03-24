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
        "Visualize temporal data using time slider",
        "Data",
        "The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.",
        "")]
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
