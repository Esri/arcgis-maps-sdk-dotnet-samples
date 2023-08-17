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
        description: "Manage a feature layer's features in four distinct ways.",
        instructions: "Pick an operation, then tap a location on the map to perform the operation at that location. Available feature management operations include \"Create feature\", \"Delete feature\", \"Update attribute\", and \"Update geometry\".",
        tags: new[] { "amend", "attribute", "deletion", "details", "edit", "editing", "feature", "feature layer", "feature table", "information", "moving", "online service", "service", "updating", "value" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ManageFeatures
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the feature table.
        private ServiceFeatureTable _damageFeatureTable;

        // Name of the field that will be updated.
        private const string AttributeFieldName = "typdamage";

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

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
                ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
                await serviceGeodatabase.LoadAsync();

                // Gets the feature table from the service geodatabase, referring to the Damage Assessment feature service.
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void DamageTable_Loaded(object sender, EventArgs e)
        {
            // This code needs to work with the UI, so it needs to run on the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable)sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                CodedValueDomain attributeDomain = (CodedValueDomain)typeDamageField.Domain;

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
            MyMapView.DismissCallout();

            // Store the ComboBox's selected item index.
            int index = ((ComboBox)sender).SelectedIndex;

            // Update the label with the new instruction.
            InstructionLabel.Content = _instructions[index];

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

        private async void MapView_Tapped_CreateFeature(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create the feature.
                ArcGISFeature feature = (ArcGISFeature)_damageFeatureTable.CreateFeature();

                // Get the normalized geometry for the tapped location and use it as the feature's geometry.
                MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
                feature.Geometry = tappedPoint;

                // Set feature attributes.
                feature.SetAttributeValue("typdamage", "Minor");
                feature.SetAttributeValue("primcause", "Earthquake");

                // Add the feature to the table.
                await _damageFeatureTable.AddFeatureAsync(feature);

                // Apply the edits to the service on the service geodatbase.
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

        private async void MapView_Tapped_DeleteFeature(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Otherwise, get the ID of the first result.
                long featureId = (long)identifyResult.GeoElements.First().Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                QueryParameters qp = new QueryParameters();
                qp.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(qp);
                Feature tappedFeature = queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(tappedFeature);

                // Show the callout.
                ShowDeletionCallout(tappedFeature);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void ShowDeletionCallout(Feature tappedFeature)
        {
            // Create a button for deleting the feature.
            Button deleteButton = new Button();
            deleteButton.Content = "Delete incident";
            deleteButton.Padding = new Thickness(5);
            deleteButton.Tag = tappedFeature;

            // Handle button clicks.
            deleteButton.Click += DeleteButton_Click;

            // Show the callout.
            MyMapView.ShowCalloutAt((MapPoint)tappedFeature.Geometry, deleteButton);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Dismiss the callout.
            MyMapView.DismissCallout();

            try
            {
                // Get the feature to delete from the layer.
                Button deleteButton = (Button)sender;
                Feature featureToDelete = (Feature)deleteButton.Tag;

                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(featureToDelete);

                // Sync the change with the service on the service geodatabase.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_damageLayer.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();

                // Show a message confirming the deletion.
                MessageBox.Show("Deleted feature with ID " + featureToDelete.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private async void MapView_Tapped_UpdateAttribute(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            // Reset the Chooser.
            DamageTypeChooser.Visibility = Visibility.Collapsed;
            DamageTypeChooser.SelectedIndex = -1;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

                // Reset the instruction label and return if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    InstructionLabel.Content = _instructions[2];
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                // Update instruction label.
                InstructionLabel.Content += "Select damage type:";

                // Update the UI for the selection.
                UpdateUIForSelectedFeature();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void UpdateUIForSelectedFeature()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Update the ComboBox selection without triggering the event.
            DamageTypeChooser.SelectionChanged -= DamageType_Changed;
            DamageTypeChooser.SelectedValue = currentAttributeValue;
            DamageTypeChooser.SelectionChanged += DamageType_Changed;

            // Enable the ComboBox.
            DamageTypeChooser.Visibility = Visibility.Visible;
        }

        private async void DamageType_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Skip if nothing is selected.
            if (DamageTypeChooser.SelectedIndex == -1)
            {
                return;
            }

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
                ServiceFeatureTable table = (ServiceFeatureTable)_selectedFeature.FeatureTable;
                await table.ServiceGeodatabase.ApplyEditsAsync();

                MessageBox.Show("Edited feature " + _selectedFeature.Attributes["objectid"], "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void MapView_Tapped_UpdateGeometry(object sender, GeoViewInputEventArgs e)
        {
            // Select the feature if none selected, move the feature otherwise.
            if (_selectedFeature == null)
            {
                // Select the feature.
                TrySelectFeature(e);
            }
            else
            {
                // Move the feature.
                MoveSelectedFeature(e);
            }
        }

        private async void MoveSelectedFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Get the MapPoint from the EventArgs for the tap.
                MapPoint destinationPoint = tapEventDetails.Location;

                // Normalize the point - needed when the tapped location is over the international date line.
                destinationPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(destinationPoint);

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the geometry of the selected feature.
                _selectedFeature.Geometry = destinationPoint;

                // Apply the edit to the feature table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Push the update to the service with the service geodatabase.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;
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

        private async void TrySelectFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, tapEventDetails.Position, 10, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}