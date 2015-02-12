using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates how to update feature geometry in feature layer.
    /// </summary>
    /// <title>Feature Layer Edit Geometry</title>
    /// <category>Editing</category>
    public partial class FeatureLayerEditGeometry : Page
    {
        public FeatureLayerEditGeometry()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Selects feature for editing.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            // Ignore tap events while in edit mode so we do not interfere with edit geometry.
            var inEditMode = EditButton.IsEnabled;
            if (inEditMode)
                return;
            var layer = MyMapView.Map.Layers["Incidents"] as FeatureLayer;
            layer.ClearSelection();
            SetGeometryEditor();

            string message = null;
            try
            {
                // Performs hit test on layer to select feature.
                var features = await layer.HitTestAsync(MyMapView, e.Position);
                if (features == null || !features.Any())
                    return;
                var featureID = features.FirstOrDefault();
                layer.SelectFeatures(new long[] { featureID });
                var feature = await layer.FeatureTable.QueryAsync(featureID);
                SetGeometryEditor(feature);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Prepares GeometryEditor for editing.
        /// </summary>
        private void SetGeometryEditor(Feature feature = null)
        {
            EditButton.Tag = feature;
            EditButton.IsEnabled = feature == null ? false : true;
        }

        /// <summary>
        /// Enables geometry editing and submits geometry edit back to the server.
        /// </summary>
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (Feature)EditButton.Tag;
            var layer = MyMapView.Map.Layers["Incidents"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;

            // Hides feature from feature layer while its geometry is being modified.
            layer.SetFeatureVisibility(layer.SelectedFeatureIDs, false);

            string message = null;
            try
            {
                // Enables geometry editing and update its geometry 
                // using GeometryEngine to correct ring orientation.
                var geometry = await MyMapView.Editor.EditGeometryAsync(feature.Geometry);
                feature.Geometry = GeometryEngine.Simplify(geometry);
                await table.UpdateAsync(feature);
                if (table.HasEdits)
                {
                    if (table is ServiceFeatureTable)
                    {
                        var serviceTable = (ServiceFeatureTable)table;
                        // Pushes geometry edits back to the server.
                        var result = await serviceTable.ApplyEditsAsync();
                    if (result.UpdateResults == null || result.UpdateResults.Count < 1)
                        return;
                    var updateResult = result.UpdateResults[0];
                    if (updateResult.Error != null)
                        message = updateResult.Error.Message;
                }

            }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                layer.SetFeatureVisibility(layer.SelectedFeatureIDs, true);
                layer.ClearSelection();
                SetGeometryEditor();
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }
    }
}
