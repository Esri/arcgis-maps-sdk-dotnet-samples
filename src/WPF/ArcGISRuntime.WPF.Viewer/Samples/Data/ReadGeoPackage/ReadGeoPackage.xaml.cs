// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ArcGISRuntime.WPF.Samples.ReadGeoPackage
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read a GeoPackage",
        "Data",
        "This sample demonstrates how to open a GeoPackage file from the local file system and list the available GeoPackageRasters and GeoPackageFeatureTables from the GeoPackage. Users can add and remove the selected datasets as RasterLayers or FeatureLayers to the map.",
        "Select a layer name in the 'Layers Not in the Map' ListBox and then click the 'Add Layer to Map' button to add it to the map. Conversely to remove a layer from the map select a layer name in the 'Layers in the Map' ListBox and click the 'Remove Layer from Map' button. NOTE: The GeoPackage will be downloaded from an ArcGIS Online portal automatically.")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public partial class ReadGeoPackage
    {
        public ReadGeoPackage()
        {
            InitializeComponent();
            
            // Set up the GUI of the sample
            Initialize();
        }

        // Member HybridDictionary to hold the multiple key/object pairs that represent: 
        // human-readable string name of a layer - key
        // the layer itself (RasterLayer or FeatureLayer) - object
        // NOTE: According to MSDN, a HybridDictionary is useful for cases where the number 
        // of elements in a dictionary is unknown
        HybridDictionary _myHybridDictionary_Layers = new HybridDictionary();

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            MyMapView.Map = new Map(BasemapType.Streets, 39.7294, -104.8319, 11);

            // Get the full path to the GeoPackage on the device
            string myGeoPackagePath = GetGeoPackagePath();

            // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(myGeoPackagePath);

            // Get the read only list of GeoPackageRasters from the GeoPackage
            IReadOnlyList<GeoPackageRaster> myReadOnlyListOfGeoPackageRasters = myGeoPackage.GeoPackageRasters;
            
            // Loop through each GeoPackageRaster
            foreach (GeoPackageRaster oneGeoPackageRaster in myReadOnlyListOfGeoPackageRasters)
            {
                // Create a RasterLayer from the GeoPackageRaster
                RasterLayer myRasterLayer = new RasterLayer(oneGeoPackageRaster);

                // Set the opacity on the RasterLayer to partially visible 
                myRasterLayer.Opacity = 0.55;

                // Load the RasterLayer - that way we can get to it's properties
                await myRasterLayer.LoadAsync();

                // Create a string variable to hold the human-readable name of the RasterLayer for display
                // in the ListBox and the HybridDictonary - it will initially be an empty string
                string myRasterLayerName = "";

                if (myRasterLayer.Name != "")
                {
                    // We have a good human-readable name for the RasterLayer that came from
                    // the RasterLayer.Name property
                    myRasterLayerName = myRasterLayer.Name;
                }
                else if (oneGeoPackageRaster.Path.Split('/').Last() != "")
                {
                    // We did not get a good human-readable name from the RasterLayer from the .Name
                    // property, get the good human-readable name from the GeoPackageRaster.Path instead
                    myRasterLayerName = oneGeoPackageRaster.Path.Split('/').Last();
                }

                // Append the 'type of layer' to the myRasterLayerName string to display in the 
                // ListBox and as the key for the HybridDictonary
                myRasterLayerName = myRasterLayerName + " - RasterLayer";

                // Add the name of the RasterLayer and the RasterLayer itself into the HybridDictionary
                _myHybridDictionary_Layers.Add(myRasterLayerName, myRasterLayer);

                // Add the name of the RasterLayer to the ListBox of layers not in map
                ListBox_LayersNotInTheMap.Items.Add(myRasterLayerName);
            }

            // Get the read only list of GeoPackageFeatureTabless from the GeoPackage
            IReadOnlyList<GeoPackageFeatureTable> myReadOnlyListOfGeoPackageFeatureTables = myGeoPackage.GeoPackageFeatureTables;

            // Loop through each GeoPackageFeatureTable
            foreach (GeoPackageFeatureTable oneGeoPackageFeatureTable in myReadOnlyListOfGeoPackageFeatureTables)
            {
                // Create a FeatureLayer from the GeoPackageFeatureLayer
                FeatureLayer myFeatureLayer = new FeatureLayer(oneGeoPackageFeatureTable);

                // Load the FeatureLayer - that way we can get to it's properties
                await myFeatureLayer.LoadAsync();

                // Create a string variable to hold the human-readable name of the FeatureLayer for 
                // display in the ListBox and the HybridDictonary 
                string myFeatureLayerName = myFeatureLayer.Name;

                // Append the 'type of layer' to the myFeatureLayerName string to display in the 
                // ListBox and as the key for the HybridDictonary
                myFeatureLayerName = myFeatureLayerName + " - FeatureLayer";

                // Add the name of the FeatureLayer and the FeatureLayer itself into the HybridDictionary
                _myHybridDictionary_Layers.Add(myFeatureLayerName, myFeatureLayer);

                // Add the name of the FeatureLayer to the ListBox of layers not in map
                ListBox_LayersNotInTheMap.Items.Add(myFeatureLayerName);
            }
        }

        private void Button_AddLayerToMap_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the user selected value from the ListBox_LayersNotInTheMap
            object myLayerSelection = ListBox_LayersNotInTheMap.SelectedValue;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection.ToString();

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(myLayer);

                // Remove the human-readable layer name from the ListBox_LayersNoInTheMap
                ListBox_LayersNotInTheMap.Items.Remove(myLayerName);

                // Add the human-readable layer name to the LisBox_LayersInTheMap
                ListBox_LayersInTheMap.Items.Add(myLayerName);
            }
        }

        private void Button_RemoveLayerFromMap_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get the user selected value from the ListBox_LayersInTheMap
            object myLayerSelection = ListBox_LayersInTheMap.SelectedValue;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection.ToString();

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Remove the layer from the map
                MyMapView.Map.OperationalLayers.Remove(myLayer);

                // Remove the human-readable layer name from the ListBox_LayersInTheMap
                ListBox_LayersInTheMap.Items.Remove(myLayerName);

                // Add the human-readable layer name to the ListBox_LayersNotInTheMap
                ListBox_LayersNotInTheMap.Items.Add(myLayerName);
            }
        }

        private static string GetGeoPackagePath()
        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
        }

    }
}