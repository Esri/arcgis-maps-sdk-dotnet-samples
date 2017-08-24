// Copyright 2016 Esri.
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
using System;
using System.Collections.Generic;
using Xamarin.Forms;
#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntimeXamarin.Samples.ExportTiles
{
	public partial class ExportTiles : ContentPage
	{
		public ExportTiles ()
		{
            InitializeComponent ();

            Title = "Create a feature collection layer";

            // call a function to initialize a map to display in the MyMapView control
            Initialize();
		}

        private void Initialize()
        {
            Map myMap = new Map(Basemap.CreateStreets());
            MyMapView.Map = myMap;
        }
    }
}
