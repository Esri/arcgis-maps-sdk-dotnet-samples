﻿// Copyright 2019 Esri.
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
using Windows.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISRuntime.UWP.Samples.MapReferenceScale
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map reference scale",
        "Map",
        "Set a map's reference scale and control which feature layers honor that scale.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("Converters/DoubleToScaleStringConverter")]
    public partial class MapReferenceScale
    {
        // List of reference scale options.
        private readonly double[] _referenceScales =
        {
            50000,
            100000,
            250000,
            500000
        };

        public MapReferenceScale()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a portal and an item; the map will be loaded from portal item.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri("https://runtime.maps.arcgis.com"));
            PortalItem mapItem = await PortalItem.CreateAsync(portal, "3953413f3bd34e53a42bf70f2937a408");

            // Create the map from the item.
            Map webMap = new Map(mapItem);

            // Display the map.
            MyMapView.Map = webMap;

            // Populate the reference scale choices.
            ReferenceScaleBox.ItemsSource = _referenceScales;

            // NOTE: this sample uses binding, so explicit control of reference scale isn't seen here.
            // See the iOS or Android samples for an implementation that does not rely on data binding.
        }
    }
}