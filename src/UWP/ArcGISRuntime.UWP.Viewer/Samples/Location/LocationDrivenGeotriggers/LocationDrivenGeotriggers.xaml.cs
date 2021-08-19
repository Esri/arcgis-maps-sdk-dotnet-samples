﻿// Copyright 2021 Esri.
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
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.LocationDrivenGeotriggers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Set up location-driven Geotriggers",
        category: "Location",
        description: "Create a notification every time a given location data source has entered and/or exited a set of features or graphics.",
        instructions: "Observe a virtual walking tour of the Santa Barbara Botanic Garden. Information about the user's current Garden Section, as well as information about nearby points of interest within 10 meters will display or be removed from the UI when the user enters or exits the buffer of each feature.",
        tags: new[] { "alert", "arcade", "fence", "geofence", "geotrigger", "location", "navigation", "notification", "notify", "routing", "trigger" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class LocationDrivenGeotriggers
    {
        public LocationDrivenGeotriggers()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}
