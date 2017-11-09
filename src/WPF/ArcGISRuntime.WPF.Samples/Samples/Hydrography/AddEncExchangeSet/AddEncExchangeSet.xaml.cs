// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.AddEncExchangeSet
{
    public partial class AddEncExchangeSet
    {
        public AddEncExchangeSet()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap
            MyMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = await GetEncPath();

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet encExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the layer to load
            await encExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataSet encDataset in encExchangeSet.DataSets)
            {
                // Create the cell and layer
                EncLayer encLayer = new EncLayer(new EncCell(encDataset));

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(encLayer);

                // Wait for the layer to load
                await encLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(encLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            MyMapView.SetViewpoint(new Viewpoint(fullExtent));
        }

        private async Task<String> GetEncPath()
        {
            #region offlinedata

            // TODO - replace with offline data code
            return @"\\apps-data\Data\test_strategy\data_for_test_designs\ENC\ExchangeSet\ENC_ROOT\CATALOG.031";

            #endregion offlinedata
        }
    }
}