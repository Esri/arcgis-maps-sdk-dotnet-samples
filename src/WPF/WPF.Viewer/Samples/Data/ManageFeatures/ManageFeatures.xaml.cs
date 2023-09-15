// Copyright 2023 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArcGIS.WPF.Samples.ManageFeatures
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Manage features",
        category: "Data",
        description: "Create, update, and delete features to manage a feature layer.",
        instructions: "Pick an operation, then tap on the map to perform the operation at that location. Available feature management operations include: \"Create feature\", \"Delete feature\", \"Update attribute\", and \"Update geometry\".",
        tags: new[] { "amend", "attribute", "create", "delete", "deletion", "details", "edit", "editing", "feature", "feature layer", "feature table", "geodatabase", "information", "moving", "online service", "service", "update", "updating", "value" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ManageFeatures
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Name of the field that will be updated.
        private const string AttributeFieldName = "typdamage";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the feature table.
        private ServiceFeatureTable _damageFeatureTable;

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        // Create a button for deleting features.
        private Button _deleteButton;

        private readonly string[] _methodList = new string[]
        {
            "Create feature",
            "Delete feature",
            "Update attribute",
            "Update geometry"
        };

        private readonly string[] _instructions = new string[]
        {
            "Tap on the map to create a new feature.",
            "Tap an existing feature to delete it.",
            "Tap an existing feature to edit its attribute.",
            "Tap an existing feature to select it, tap the map to move it to a new position."
        };

        public ManageFeatures()
        {
            InitializeComponent();
            _ = Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                // Create the map with streets basemap.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create a service geodatabase from the feature service.
                var serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
                await serviceGeodatabase.LoadAsync();

                // Get the feature table from the service geodatabase referencing the Damage Assessment feature service.
                // Creating the feature table from the feature service will cause the service geodatabase to be null.
                _damageFeatureTable = serviceGeodatabase.GetTable(0);

                // Update attributes - when the table loads, use it to discover the domain of the typdamage field.
                _damageFeatureTable.Loaded += DamageTable_Loaded;

                // Create a feature layer to visualize the features in the table.
                _damageLayer = new FeatureLayer(_damageFeatureTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(_damageLayer);

                // Zoom to the United States.
                _ = MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);

                // Bind the list of method names to the ComboBox.
                OperationChooser.ItemsSource = _methodList;

                // Create a button for deleting the feature.
                _deleteButton = new Button
                {
                    Content = "Delete incident",
                    Padding = new Thickness(5)
                };

                // Handle button clicks.
                _deleteButton.Click += DeleteButton_Click;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void MapView_Tapped_CreateFeature(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create the feature.
                var feature = (ArcGISFeature)_damageFeatureTable.CreateFeature();

                // Get the normalized geometry for the tapped location and use it as the feature's geometry.
                var tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
                feature.Geometry = tappedPoint;

                // Set feature attributes.
                feature.SetAttributeValue("typdamage", "Minor");
                feature.SetAttributeValue("primcause", "Earthquake");

                // Add the feature to the table.
                await _damageFeatureTable.AddFeatureAsync(feature);

                // Apply the edits to the service on the service geodatabase.
                await _damageFeatureTable.ServiceGeodatabase.ApplyEditsAsync();

                // Update the feature to get the updated objectid - a temporary ID is used before the feature is added.
                feature.Refresh();

                // Confirm feature addition.
                MessageBox.Show("Created feature " + feature.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        #region Delete feature

        private async void MapView_Tapped_DeleteFeature(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            try
            {
                // Determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await TrySelectFeature(e.Position);

                // Do nothing if there are no results.
                if (identifyResult == null) return;

                // Otherwise, get the ID of the first result.
                var featureId = (long)identifyResult.GeoElements[0].Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                var queryParameters = new QueryParameters();
                queryParameters.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(queryParameters);
                Feature tappedFeature = queryResult.First();

                // Show the deletion callout.
                _deleteButton.Tag = tappedFeature;
                MyMapView.ShowCalloutAt((MapPoint)tappedFeature.Geometry, _deleteButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Dismiss the callout.
            MyMapView.DismissCallout();

            try
            {
                var featureToDelete = (Feature)_deleteButton.Tag;

                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(featureToDelete);

                // Sync the change with the service on the service geodatabase.
                var serviceTable = (ServiceFeatureTable)_damageLayer.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();

                // Show a message confirming the deletion.
                MessageBox.Show("Deleted feature with ID " + featureToDelete.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        #endregion Delete feature

        #region Update attribute

        private async void MapView_Tapped_UpdateAttribute(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await TrySelectFeature(e.Position);

                // Reset the instruction label and return if there are no results.
                if (identifyResult == null)
                {
                    Instructions.Text = _instructions[2];
                    DamageTypeChooser.Visibility = Visibility.Collapsed;
                    return;
                }

                // Update instruction label.
                Instructions.Text = "Select damage type:";

                // Get the current value.
                string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

                // Update the ComboBox selection without triggering the event.
                DamageTypeChooser.SelectionChanged -= DamageType_Changed;
                DamageTypeChooser.SelectedValue = currentAttributeValue;
                DamageTypeChooser.SelectionChanged += DamageType_Changed;

                // Enable the ComboBox.
                DamageTypeChooser.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void DamageType_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Skip if nothing is selected.
            if (DamageTypeChooser.SelectedIndex == -1) return;

            try
            {
                // Get the new value.
                string selectedAttributeValue = DamageTypeChooser.SelectedValue.ToString();

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(AttributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service on the service geodatabase.
                var table = (ServiceFeatureTable)_selectedFeature.FeatureTable;
                await table.ServiceGeodatabase.ApplyEditsAsync();

                // Show just the initial instructions for update geometry.
                Instructions.Text = _instructions[2];
                DamageTypeChooser.Visibility = Visibility.Collapsed;

                MessageBox.Show("Edited feature " + _selectedFeature.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                // Reset the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;
            }
        }

        #endregion Update attribute

        #region Update geometry

        private void MapView_Tapped_UpdateGeometry(object sender, GeoViewInputEventArgs e)
        {
            // Select the feature if none selected, move the feature otherwise.
            if (_selectedFeature == null)
            {
                // Select the feature.
                _ = TrySelectFeature(e.Position);
            }
            else
            {
                // Move the feature.
                _ = MoveSelectedFeature(e.Location);
            }
        }

        private async Task MoveSelectedFeature(MapPoint destinationPoint)
        {
            try
            {
                // Normalize the point - needed when the tapped location is over the international date line.
                destinationPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(destinationPoint);

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the geometry of the selected feature.
                _selectedFeature.Geometry = destinationPoint;

                // Apply the edit to the feature table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Push the update to the service with the service geodatabase.
                var serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();
                MessageBox.Show("Moved feature " + _selectedFeature.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                // Reset the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;
            }
        }

        #endregion Update geometry

        private void DamageTable_Loaded(object sender, EventArgs e)
        {
            // This code needs to work with the UI, so it needs to run on the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Get the relevant field from the table.
                var table = (ServiceFeatureTable)sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                var attributeDomain = (CodedValueDomain)typeDamageField.Domain;

                // Update the ComboBox with the attribute values.
                DamageTypeChooser.ItemsSource = attributeDomain.CodedValues.Select(codedValue => codedValue.Name);
            });
        }

        private void OperationChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear all potentially hooked GeoViewTapped events.
            MyMapView.GeoViewTapped -= MapView_Tapped_CreateFeature;
            MyMapView.GeoViewTapped -= MapView_Tapped_DeleteFeature;
            MyMapView.GeoViewTapped -= MapView_Tapped_UpdateAttribute;
            MyMapView.GeoViewTapped -= MapView_Tapped_UpdateGeometry;

            // Reset UI elements.
            _damageLayer.ClearSelection();
            _selectedFeature = null;
            DamageTypeChooser.Visibility = Visibility.Collapsed;
            DamageTypeChooser.SelectedIndex = -1;
            MyMapView.DismissCallout();

            // Store the selected operation's item index.
            int index = OperationChooser.SelectedIndex;

            // Update the label with the new instruction.
            Instructions.Text = _instructions[index];

            switch (index)
            {
                case 0:
                    MyMapView.GeoViewTapped += MapView_Tapped_CreateFeature;
                    break;

                case 1:
                    MyMapView.GeoViewTapped += MapView_Tapped_DeleteFeature;
                    break;

                case 2:
                    MyMapView.GeoViewTapped += MapView_Tapped_UpdateAttribute;
                    break;

                case 3:
                    MyMapView.GeoViewTapped += MapView_Tapped_UpdateGeometry;
                    break;
            }
        }

        private async Task<IdentifyLayerResult> TrySelectFeature(Point point)
        {
            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, point, 2, false);

                // Return null if there are no results.
                if (!identifyResult.GeoElements.Any()) return null;

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements[0];

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                return identifyResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
                return null;
            }
        }
    }
}