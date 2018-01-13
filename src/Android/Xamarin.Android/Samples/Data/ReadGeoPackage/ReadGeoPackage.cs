// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using System.IO;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntimeXamarin.Managers;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArcGISRuntimeXamarin.Samples.ReadGeoPackage
{
    [Activity]
    public class ReadGeoPackage : Activity
    {

        // Member MapView UI control used in the sample
        private MapView _myMapView;

        // Hold a reference to the ListView for layer names that are currently displayed in the map
        private ListView _myListView_LayersInTheMap;

        // Hold a reference to the ListView for layer names that are currently Not displayed in the map
        private ListView _myListView_LayersNotInTheMap;

        // Member HybridDictionary to hold the multiple key/object pairs that represent: 
        // human-readable string name of a layer - key
        // the layer itself (RasterLayer or FeatureLayer) - object
        // NOTE: According to MSDN, a HybridDictionary is useful for cases where the number 
        // of elements in a dictionary is unknown
        private HybridDictionary _myHybridDictionary_Layers = new HybridDictionary();

        // Member ObservableCollection to hold the human-readable string name of the 
        // layers - used as the ListView_LayersNotInTheMap.ItemsSource 
        ObservableCollection<string> _myObservableCollection_LayerNamesNotInTheMap = new ObservableCollection<string>();

        // Member ObservableCollection to hold the human-readable string name of the 
        // layers - used as the ListView_LayersInTheMap.ItemsSource 
        ObservableCollection<string> _myObservableCollection_LayerNamesInTheMap = new ObservableCollection<string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Read a GeoPackage";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();

            // Set up the initial rendering of the sample
            Initialize();
        }
        
        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            _myMapView.Map = new Map(BasemapType.Streets, 39.7294, -104.8319, 11);

            // Get the full path to the GeoPackage on the device
            string myGeoPackagePath = await GetGeoPackagePath();

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

                // Add the name of the RasterLayer to _myObservableCollection_LayerNamesNotInTheMap 
                // which displays the human-readable layer names used by the _myListView_LayersNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Add(myRasterLayerName);
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

                // Add the name of the RasterLayer to _myObservableCollection_LayerNamesNotInTheMap 
                // which displays the human-readable layer names used by the _myListView_LayersNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Add(myFeatureLayerName);
            }

            // Create a simple string array of the human-readable layer names from the ObservableCollection
            string[] myStringArray_LayerNamesNotInTheMap = _myObservableCollection_LayerNamesNotInTheMap.ToArray();

            // Create an ArrayAdapter from the simple string array
            ArrayAdapter myArrayAdapter_LayerNamesNotInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_LayerNamesNotInTheMap);

            // Set the _myListView_LayersNotInTheMap.Adapter to the myArrayAdapter_LayerNamesNotInTheMap
            // This allows the human-readable layer names to be displayed a ListView
            _myListView_LayersNotInTheMap.Adapter = myArrayAdapter_LayerNamesNotInTheMap;
        }

        private void _myListView_LayersNotInTheMap_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the ListView
            // It finds the actual layer in the HybridDictionary based upon the user selection and add it to the map
            // Then the human-readable layer name is removed from the _myListView_LayersNotInTheMap and added to the 
            // _myListView_LayersInTheMap

            // Get the TextView from the AdapterView.ItemClickEventArgs
            TextView myTextView = (TextView)e.View;

            // Get the user selected item from the TextView
            string myLayerSelection = myTextView.Text;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection.ToString();

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myLayer);

                // ------------------------------------------------------------------------------------------
                // Remove the human-readable layer name from the _myObservableCollection_LayerNamesNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Remove(myLayerName);

                // Create a simple string array of the human-readable layer names from the ObservableCollection
                string[] myStringArray_LayerNamesNotInTheMap = _myObservableCollection_LayerNamesNotInTheMap.ToArray();

                // Create an ArrayAdapter from the simple string array
                ArrayAdapter myArrayAdapter_LayerNamesNotInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_LayerNamesNotInTheMap);

                // Set the _myListView_LayersNotInTheMap.Adapter to the myArrayAdapter_LayerNamesNotInTheMap
                // This allows the human-readable layer names to be displayed a ListView
                _myListView_LayersNotInTheMap.Adapter = myArrayAdapter_LayerNamesNotInTheMap;
                // ------------------------------------------------------------------------------------------

                // ------------------------------------------------------------------------------------------
                // Add the human-readable layer name to the _myObservableCollection_LayerNamesInTheMap
                _myObservableCollection_LayerNamesInTheMap.Add(myLayerName);

                // Create a simple string array of the human-readable layer names from the ObservableCollection
                string[] myStringArray_LayerNamesInTheMap = _myObservableCollection_LayerNamesInTheMap.ToArray();

                // Create an ArrayAdapter from the simple string array
                ArrayAdapter myArrayAdapter_LayerNamesInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_LayerNamesInTheMap);

                // Set the _myListView_LayersInTheMap.Adapter to the myArrayAdapter_LayerNamesInTheMap
                // This allows the human-readable layer names to be displayed a ListView
                _myListView_LayersInTheMap.Adapter = myArrayAdapter_LayerNamesInTheMap;
                // ------------------------------------------------------------------------------------------
            }
        }

        private void _myListView_LayersInTheMap_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the ListView
            // It finds the actual layer in the HybridDictionary based upon the user selection and removes it from the map
            // Then the human-readable layer name is added to the _myListView_LayersNotInTheMap and removed from to the 
            // _myListView_LayersInTheMap

            // Get the TextView from the AdapterView.ItemClickEventArgs
            TextView myTextView = (TextView)e.View;

            // Get the user selected item from the TextView
            string myLayerSelection = myTextView.Text;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection;

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Remove the layer from the map
                _myMapView.Map.OperationalLayers.Remove(myLayer);

                // ------------------------------------------------------------------------------------------
                // Remove the human-readable layer name from the _myObservableCollection_LayerNamesInTheMap
                _myObservableCollection_LayerNamesInTheMap.Remove(myLayerName);

                // Create a simple string array of the human-readable layer names from the ObservableCollection
                string[] myStringArray_LayerNamesInTheMap = _myObservableCollection_LayerNamesInTheMap.ToArray();

                // Create an ArrayAdapter from the simple string array
                ArrayAdapter myArrayAdapter_LayerNamesInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_LayerNamesInTheMap);

                // Set the _myListView_LayersNotInTheMap.Adapter to the myArrayAdapter_LayerNamesInTheMap
                // This allows the human-readable layer names to be displayed a ListView
                _myListView_LayersInTheMap.Adapter = myArrayAdapter_LayerNamesInTheMap;
                // ------------------------------------------------------------------------------------------

                // ------------------------------------------------------------------------------------------
                // Add the human-readable layer name to the _myObservableCollection_LayerNamesNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Add(myLayerName);

                // Create a simple string array of the human-readable layer names from the ObservableCollection
                string[] myStringArray_LayerNamesNotInTheMap = _myObservableCollection_LayerNamesNotInTheMap.ToArray();

                // Create an ArrayAdapter from the simple string array
                ArrayAdapter myArrayAdapter_LayerNamesNotInTheMap = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myStringArray_LayerNamesNotInTheMap);

                // Set the _myListView_LayersNotInTheMap.Adapter to the myArrayAdapter_LayerNamesNotInTheMap
                // This allows the human-readable layer names to be displayed a ListView
                _myListView_LayersNotInTheMap.Adapter = myArrayAdapter_LayerNamesNotInTheMap;
                // ------------------------------------------------------------------------------------------
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView to show which layers are currently in the map
            TextView myTextViewLabel1 = new TextView(this) { Text = "Layers in the Map" };
            layout.AddView(myTextViewLabel1);

            // Create a ListView to show which layers are currently in the map - also
            // wire up an event handler to remove layers from the map upon a user selection
            _myListView_LayersInTheMap = new ListView(this);
            _myListView_LayersInTheMap.ItemClick += _myListView_LayersInTheMap_ItemClick;
            layout.AddView(_myListView_LayersInTheMap);

            // Create a TextView to show which layers are currently Not in the map
            TextView myTextViewLabel2 = new TextView(this) { Text = "Layers Not in the Map" };
            layout.AddView(myTextViewLabel2);

            // Create a ListView to show which layers are currently in Not the map - also
            // wire up an event handler to add layers to the map upon a user selection
            _myListView_LayersNotInTheMap = new ListView(this);
            _myListView_LayersNotInTheMap.ItemClick += _myListView_LayersNotInTheMap_ItemClick;
            layout.AddView(_myListView_LayersNotInTheMap);

            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private async Task<string> GetGeoPackagePath()
        {
            #region offlinedata

            // The GeoPackage will be downloaded from ArcGIS Online.
            // The data manager (a component of the sample viewer), *NOT* the runtime handles the offline data process

            // The desired GPKG is expected to be called "AuroraCO.shp"
            string filename = "AuroraCO.gpkg";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "ReadGeoPackage", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // If it's missing, download the GeoPackage
                await DataManager.GetData("68ec42517cdd439e81b036210483e8e7", "ReadGeoPackage");
            }

            // Return the path
            return filepath;

            #endregion offlinedata
        }
    }
}