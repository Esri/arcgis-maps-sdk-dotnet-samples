// Copyright 2022 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntime.Samples.Managers;
using System.Linq;
using System;
using ArcGISRuntime;

namespace ArcGISRuntimeXamarin.Samples.DisplayDimensions
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display dimensions",
        category: "Layers",
        description: "Display dimension features from a mobile map package.",
        instructions: "When the sample loads, it will automatically display the map containing dimension features from the mobile map package. The name of the dimension layer containing the dimension features is displayed in the controls box. Control the visibility of the dimension layer with the \"Dimension Layer visibility\" check box, and apply a definition expression to show dimensions of greater than or equal to 450m in length using the \"Definition Expression\" checkbox.",
        tags: new[] { "dimension", "layer", "mmpk", "mobile map package", "utility" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f5ff6f5556a945bca87ca513b8729a1e")]
    public class DisplayDimensions : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private CheckBox _dimensionLayerCheckBox;
        private CheckBox _definitionExpressionCheckBox;
        private TextView _pylonLabel;

        // Mobile map package that contains dimension layers.
        private MobileMapPackage _mobileMapPackage;

        // Dimension layer, the operational layer. 
        private DimensionLayer _dimensionLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display dimensions";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Load the mobile map package.
                _mobileMapPackage = new MobileMapPackage(DataManager.GetDataFolder("f5ff6f5556a945bca87ca513b8729a1e", "Edinburgh_Pylon_Dimensions.mmpk"));
                await _mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                _myMapView.Map = _mobileMapPackage.Maps.First();

                // Set the minimum scale range of the sample to maintain readability of dimension features.
                _myMapView.Map.MinScale = 35000;

                // Get the dimension layer from the MapView operational layers.
                _dimensionLayer = (DimensionLayer)_myMapView.Map.OperationalLayers.Where(layer => layer is DimensionLayer).First();

                // Load the dimension layer.
                await _dimensionLayer.LoadAsync();

                // Enable the switches.
                _dimensionLayerCheckBox.Enabled = true;
                _definitionExpressionCheckBox.Enabled = true;

                // Set the label content.
                _pylonLabel.Text = _dimensionLayer.Name;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void DimensionLayerCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // Set the visibility of the dimension layer.
            if (_dimensionLayer != null) _dimensionLayer.IsVisible = _dimensionLayerCheckBox.Checked == true;
        }

        private void DefinitionExpressionCheckBoxChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // Set a definition expression to show dimension lengths of greater than or equal to 450m when the checkbox is selected,
            // or to reset the definition expression to show all dimension lengths when unselected.
            string definitionExpression = _definitionExpressionCheckBox.Checked == true ? "DIMLENGTH >= 450" : "";
            if (_dimensionLayer != null) _dimensionLayer.DefinitionExpression = definitionExpression;
        }

        private void CreateLayout()
        {
            // Load the UI from the axml file.
            SetContentView(Resource.Layout.DisplayDimensions);

            // Get the UI elements from the axml resource.
            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _dimensionLayerCheckBox = FindViewById<CheckBox>(Resource.Id.dimensionLayerCheckBox);
            _definitionExpressionCheckBox = FindViewById<CheckBox>(Resource.Id.definitionExpressionCheckBox);
            _pylonLabel = FindViewById<TextView>(Resource.Id.pylonLabel);

            // Add listeners for the checkboxes.
            _dimensionLayerCheckBox.CheckedChange += DimensionLayerCheckBoxChanged;
            _definitionExpressionCheckBox.CheckedChange += DefinitionExpressionCheckBoxChanged;
        }
    }
}
