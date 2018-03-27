// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.ReadGeoPackage
{
    [Register("ReadGeoPackage")]
	[Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    [Shared.Attributes.Sample(
        "Read a GeoPackage",
        "Data",
        "This sample demonstrates how to open a GeoPackage file from the local file system and list the available GeoPackageRasters and GeoPackageFeatureTables from the GeoPackage. Users can add and remove the selected datasets as RasterLayers or FeatureLayers to the map.",
        "Select a layer name in the 'Layers Not in the Map' UISegmentedControl to add it to the map. Conversely to remove a layer from the map select a layer name in the 'Layers in the Map' UISegmentedControl. NOTE: The GeoPackage will be downloaded from an ArcGIS Online portal automatically.")]
    public class ReadGeoPackage : UIViewController
    {
        // Member MapView control to display layers in the sample
        private MapView _myMapView = new MapView();

        // Member UISegmentedControl control to add and removed layers in the MapView
        private UISegmentedControl _myUISegmentedControl = new UISegmentedControl();

        // Member HybridDictionary to hold the multiple key/object pairs that represent: 
        // human-readable string name of a layer - key
        // the layer itself (RasterLayer or FeatureLayer) - object
        // NOTE: According to MSDN, a HybridDictionary is useful for cases where the number 
        // of elements in a dictionary is unknown
        HybridDictionary _myHybridDictionary_Layers = new HybridDictionary();

        // Member ObservableCollection to hold the human-readable string name of the layers
        // that are currently Not displayed in the MapView 
        ObservableCollection<string> _myObservableCollection_LayerNamesNotInTheMap = new ObservableCollection<string>();

        // Member ObservableCollection to hold the human-readable string name of the layers
        // that are currently displayed in the MapView
        ObservableCollection<string> _myObservableCollection_LayerNamesInTheMap = new ObservableCollection<string>();

        public ReadGeoPackage()
        {
            Title = "Read a GeoPackage";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Function to create the UI layout of the sample
            CreateLayout();

            // Function to setup the state of what is shown when the sample opens 
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Define the viewable area of the MapView in the sample
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Define the viewable area of the UISegmentedControl in the sample
            _myUISegmentedControl.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado
            _myMapView.Map = new Map(BasemapType.Streets, 39.7294, -104.8319, 11);

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
                // in the UISegmentedControl and the HybridDictonary - it will initially be an empty string
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

                // Add the name of the RasterLayer to _MyObservableCollectionLayerNamesNotInTheMap 
                // which displays the human-readable layer names used by the UISegmentedControl
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
                // display in the UISegmentedControl and the HybridDictonary 
                string myFeatureLayerName = myFeatureLayer.Name;

                // Append the 'type of layer' to the myFeatureLayerName string to display in the 
                // ListBox and as the key for the HybridDictonary
                myFeatureLayerName = myFeatureLayerName + " - FeatureLayer";

                // Add the name of the FeatureLayer and the FeatureLayer itself into the HybridDictionary
                _myHybridDictionary_Layers.Add(myFeatureLayerName, myFeatureLayer);

                // Add the name of the RasterLayer to _myObservableCollectionLayerNamesNotInTheMap 
                // which displays the human-readable layer names used by the UISegmentedControl
                _myObservableCollection_LayerNamesNotInTheMap.Add(myFeatureLayerName);
            }
        }

        private void CreateLayout()
        {
            // Configure UISegmentedControl
            _myUISegmentedControl.BackgroundColor = UIColor.White;
            _myUISegmentedControl.InsertSegment("Layers in map", 0, false);
            _myUISegmentedControl.InsertSegment("Layers Not in map", 1, false);

            // Handle the "click" for each segment (new segment is selected)
            _myUISegmentedControl.ValueChanged += _MyUISegmentedControl_ValueChanged;

            // Add the MapView and UISegmentedControl to the page
            View.AddSubviews(_myMapView, _myUISegmentedControl);
        }

        private void _MyUISegmentedControl_ValueChanged(object sender, EventArgs e)
        {
            // Get the UISegmentedControl that raised the event
            var myUISegmentedControl = sender as UISegmentedControl;

            // Get the selected segment in the control
            var mySelectedSegment = myUISegmentedControl.SelectedSegment;

            // Execute the appropriate action for the control
            if (mySelectedSegment == 0)
            {
                // Show the list of layers to removed from the map
                UISegmentButton_RemoveLayerFromMap();
            }
            else if (mySelectedSegment == 1)
            {
                // Show a list of layers to add to the map
                UISegmentButton_AddLayerToMap();
            }

            // Unselect all segments (user might want to click the same control twice)
            myUISegmentedControl.SelectedSegment = -1;
        }

        private void UISegmentButton_AddLayerToMap()
        {
            // Create a new Alert Controller - this will show the layer names that can be added to the map
            UIAlertController myUIAlertController = UIAlertController.Create("Add a layer to the map", "", UIAlertControllerStyle.ActionSheet);

            // Add actions to add a layer to the map
            foreach (string oneLayerName in _myObservableCollection_LayerNamesNotInTheMap)
            {
                myUIAlertController.AddAction(UIAlertAction.Create(oneLayerName, UIAlertActionStyle.Default, (action) => Action_AddLayerToMap(oneLayerName)));
            }

            // Add a choice to cancel
            myUIAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover
            UIPopoverPresentationController presentationPopover = myUIAlertController.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of layers to add to the map
            PresentViewController(myUIAlertController, true, null);
        }

        private void Action_AddLayerToMap(string layerName)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the UISegmentedControl
            // It finds the actual layer in the HybridDictionary based upon the user selection and add it to the map
            // Then the human-readable layer name is removed from the _myObservableCollection_LayerNamesNotInTheMap and 
            // added to the _myObservableCollection_LayerNamesInTheMap

            // Get the user selected item from the TextView
            string myLayerSelection = layerName;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection;

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myLayer);

                // Remove the human-readable layer name from the ObservableCollection _myLayerNamesNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Remove(myLayerName);

                // Add the human-readable layer name to the ObservableCollection _myLayerNamesInTheMap
                _myObservableCollection_LayerNamesInTheMap.Add(myLayerName);
            }
        }

        private void UISegmentButton_RemoveLayerFromMap()
        {
            // Create a new Alert Controller
            UIAlertController layersActionSheet = UIAlertController.Create("Remove a layer from the map", "", UIAlertControllerStyle.ActionSheet);

            // Add actions to remove a layer from the map
            foreach (string oneLayerName in _myObservableCollection_LayerNamesInTheMap)
            {
                layersActionSheet.AddAction(UIAlertAction.Create(oneLayerName, UIAlertActionStyle.Default, (action) => Action_RemoveLayerFromMap(oneLayerName)));
            }

            // Add a choice to cancel
            layersActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover
            UIPopoverPresentationController presentationPopover = layersActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of layers to add/remove
            PresentViewController(layersActionSheet, true, null);
        }

        private void Action_RemoveLayerFromMap(string layerName)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the UISegmentedControl
            // It finds the actual layer in the HybridDictionary based upon the user selection and add it to the map
            // Then the human-readable layer name is removed from the _myObservableCollection_LayerNamesInTheMap and 
            // added to the _myObservableCollection_LayerNamesNotInTheMap

            // Get the user selected item from the TextView
            string myLayerSelection = layerName;

            // Ensure we have a valid selection
            if (myLayerSelection != null)
            {
                // Get the human-readable name of the layer 
                string myLayerName = myLayerSelection;

                // Get the layer from the HybridDictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class)
                Layer myLayer = (Layer)_myHybridDictionary_Layers[myLayerName];

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Remove(myLayer);

                // Remove the human-readable layer name from the _myObservableCollectionLayerNamesInTheMap
                _myObservableCollection_LayerNamesInTheMap.Remove(myLayerName);

                // Add the human-readable layer name to the _myObservableCollectionLayerNamesNotInTheMap
                _myObservableCollection_LayerNamesNotInTheMap.Add(myLayerName);
            }
        }

        private static string GetGeoPackagePath()
        {
            return DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");
        }

    }
}