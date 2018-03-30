// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin.Samples.AddEncExchangeSet
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Add ENC Exchange Set",
        "Hydrography",
        "This sample demonstrates how to load an ENC Exchange Set, including its component cells.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public class AddEncExchangeSet : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Add an ENC exchange set";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT", "CATALOG.031");

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet( encPath );

            // Wait for the layer to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
            {
                EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myEncLayer);

                // Wait for the layer to load
                await myEncLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(myEncLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            _myMapView.SetViewpoint(new Viewpoint(fullExtent));
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}