// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.KmlLayerUrl
{
	public partial class KmlLayerUrl : ContentPage
	{
		// Hold the Uri for the service
		private Uri _serviceUri = new Uri("http://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml");

		public KmlLayerUrl()
		{
			InitializeComponent();

			Title = "KML layer (URL)";

			Initialize();
		}

		private void Initialize()
		{
			// Initialize the map with a dark gray basemap
			MyMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

			// Create a KML dataset
			KmlDataset fileDataSource = new KmlDataset(_serviceUri);

			// Create a KML layer from the dataset
			KmlLayer displayLayer = new KmlLayer(fileDataSource);

			// Add the layer to the map
			MyMapView.Map.OperationalLayers.Add(displayLayer);
		}
	}
}