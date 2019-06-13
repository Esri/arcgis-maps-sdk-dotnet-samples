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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISegmentedControl _layerSegmentedControl;

        // Dictionary associates names with layers.
        private readonly Dictionary<string, Layer> _nameToLayerDictionary = new Dictionary<string, Layer>();

        // Names of layers not being displayed.
        private readonly ObservableCollection<string> _layersNotInMap = new ObservableCollection<string>();

        // Names of layers being displayed.
        private readonly ObservableCollection<string> _layersInMap = new ObservableCollection<string>();

        public ReadGeoPackage()
        {
            Title = "Read a GeoPackage";
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado.
            _myMapView.Map = new Map(BasemapType.Streets, 39.7294, -104.8319, 11);

            // Get the full path to the GeoPackage on the device.
            string geoPackagePath = DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");

            try
            {
                // Open the GeoPackage.
                GeoPackage geoPackage = await GeoPackage.OpenAsync(geoPackagePath);

                // Loop through each GeoPackageRaster.
                foreach (GeoPackageRaster oneGeoPackageRaster in geoPackage.GeoPackageRasters)
                {
                    // Create a RasterLayer from the GeoPackageRaster.
                    RasterLayer rasterLayer = new RasterLayer(oneGeoPackageRaster)
                    {
                        // Set the opacity on the RasterLayer to partially visible.
                        Opacity = 0.55
                    };

                    // Load the RasterLayer - that way we can get to its properties.
                    await rasterLayer.LoadAsync();

                    // Create a string variable to hold the human-readable name of the RasterLayer for display.
                    string rasterLayerName = "";

                    if (rasterLayer.Name != "")
                    {
                        // We have a good human-readable name for the RasterLayer that came from the RasterLayer.Name property.
                        rasterLayerName = rasterLayer.Name;
                    }
                    else if (oneGeoPackageRaster.Path.Split('/').Last() != "")
                    {
                        // We did not get a good human-readable name from the RasterLayer from the .Name
                        // property, get the good human-readable name from the GeoPackageRaster.Path instead.
                        rasterLayerName = oneGeoPackageRaster.Path.Split('/').Last();
                    }

                    // Append the 'type of layer' to the raster layer name string to display in the 
                    // ListBox and as the key for the dictionary.
                    rasterLayerName = $"{rasterLayerName} - RasterLayer";

                    // Add the name of the RasterLayer and the RasterLayer itself into the dictionary.
                    _nameToLayerDictionary[rasterLayerName] = rasterLayer;

                    // Add the name of the RasterLayer to the layers not in the map collection
                    // which displays the human-readable layer names used by the UISegmentedControl.
                    _layersNotInMap.Add(rasterLayerName);
                }

                // Loop through each GeoPackageFeatureTable from the GeoPackage.
                foreach (GeoPackageFeatureTable oneGeoPackageFeatureTable in geoPackage.GeoPackageFeatureTables)
                {
                    // Create a FeatureLayer from the GeoPackageFeatureLayer.
                    FeatureLayer featureLayer = new FeatureLayer(oneGeoPackageFeatureTable);

                    // Load the FeatureLayer - that way we can get to its properties.
                    await featureLayer.LoadAsync();

                    // Create a string variable to hold the human-readable name of the FeatureLayer for 
                    // display in the UISegmentedControl and the dictionary.
                    string featureLayerName = featureLayer.Name;

                    // Append the 'type of layer' to the feature layer name string to display in the 
                    // ListBox and as the key for the dictionary.
                    featureLayerName = $"{featureLayerName} - FeatureLayer";

                    // Add the name of the FeatureLayer and the FeatureLayer itself into the dictionary.
                    _nameToLayerDictionary[featureLayerName] = featureLayer;

                    // Add the name of the RasterLayer to the collection of layers not in the map.
                    // which displays the human-readable layer names used by the UISegmentedControl.
                    _layersNotInMap.Add(featureLayerName);
                }

                // Enable the UI.
                _layerSegmentedControl.Enabled = true;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void LayerSegmentedControl_ValueChanged(object sender, EventArgs e)
        {
            // Get the UISegmentedControl that raised the event.
            UISegmentedControl segmentedControl = (UISegmentedControl) sender;

            switch (segmentedControl.SelectedSegment)
            {
                case 0:
                    // Show the list of layers to removed from the map.
                    UISegmentButton_RemoveLayerFromMap();
                    break;
                case 1:
                    // Show a list of layers to add to the map.
                    UISegmentButton_AddLayerToMap();
                    break;
            }

            // Deselect all segments (user might want to click the same control twice).
            segmentedControl.SelectedSegment = -1;
        }

        private void UISegmentButton_AddLayerToMap()
        {
            // Create a new Alert Controller - this will show the layer names that can be added to the map.
            UIAlertController alertController = UIAlertController.Create("Add a layer to the map", "", UIAlertControllerStyle.ActionSheet);

            // Add actions to add a layer to the map.
            foreach (string oneLayerName in _layersNotInMap)
            {
                alertController.AddAction(UIAlertAction.Create(oneLayerName, UIAlertActionStyle.Default, action => Action_AddLayerToMap(oneLayerName)));
            }

            // Add a choice to cancel.
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover.
            UIPopoverPresentationController presentationPopover = alertController.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of layers to add to the map.
            PresentViewController(alertController, true, null);
        }

        private void Action_AddLayerToMap(string layerName)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the UISegmentedControl
            // It finds the actual layer in the dictionary based upon the user selection and add it to the map
            // Then the human-readable layer name is removed from the _layersNotInMap and 
            // added to the _layersInMap.

            // Ensure there is a valid selection.
            if (!String.IsNullOrWhiteSpace(layerName))
            {
                // Get the layer from the dictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class).
                Layer layer = _nameToLayerDictionary[layerName];

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(layer);

                // Remove the human-readable layer name from the collection of layers not in the map.
                _layersNotInMap.Remove(layerName);

                // Add the human-readable layer name to the collection of layers in the map.
                _layersInMap.Add(layerName);
            }
        }

        private void UISegmentButton_RemoveLayerFromMap()
        {
            // Create a new Alert Controller.
            UIAlertController layersActionSheet = UIAlertController.Create("Remove a layer from the map", "", UIAlertControllerStyle.ActionSheet);

            // Add actions to remove a layer from the map.
            foreach (string oneLayerName in _layersInMap)
            {
                layersActionSheet.AddAction(UIAlertAction.Create(oneLayerName, UIAlertActionStyle.Default, action => Action_RemoveLayerFromMap(oneLayerName)));
            }

            // Add a choice to cancel.
            layersActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover.
            UIPopoverPresentationController presentationPopover = layersActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of layers to add/remove.
            PresentViewController(layersActionSheet, true, null);
        }

        private void Action_RemoveLayerFromMap(string layerName)
        {
            // This function executes when the user clicks on the human-readable name of a layer in the UISegmentedControl
            // It finds the actual layer in the dictionary based upon the user selection and add it to the map
            // Then the human-readable layer name is removed from the _layersInMap and 
            // added to the _layersNotInMap.

            // Ensure there is a valid selection.
            if (!String.IsNullOrEmpty(layerName))
            {
                // Get the layer from the dictionary (it could be either a RasterLayer
                // or a FeatureLayer - both inherit from the abstract/base Layer class).
                Layer layer = _nameToLayerDictionary[layerName];

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Remove(layer);

                // Remove the human-readable layer name from the collection of layers in the map.
                _layersInMap.Remove(layerName);

                // Add the human-readable layer name to the collection of layers not in the map.
                _layersNotInMap.Add(layerName);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _layerSegmentedControl = new UISegmentedControl("Remove layers", "Add layers")
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                Enabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Clean up borders of segmented control - avoid corner pixels.
            _layerSegmentedControl.ClipsToBounds = true;
            _layerSegmentedControl.Layer.CornerRadius = 5;

            // Add the views.
            View.AddSubviews(_myMapView, _layerSegmentedControl);

            // Lay out the views
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _layerSegmentedControl.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _layerSegmentedControl.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _layerSegmentedControl.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _layerSegmentedControl.ValueChanged += LayerSegmentedControl_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _layerSegmentedControl.ValueChanged -= LayerSegmentedControl_ValueChanged;
        }
    }
}