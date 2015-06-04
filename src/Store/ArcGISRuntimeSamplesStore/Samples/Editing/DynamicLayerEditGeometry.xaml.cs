using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates how to selectively import and update feature geometry in dynamic layer.
    /// </summary>
    /// <title>Dynamic Layer Edit Geometry</title>
    /// <category>Editing</category>
    public partial class DynamicLayerEditGeometry : Page
    {
        // Editing is done through this table.
        private ServiceFeatureTable table;

        public DynamicLayerEditGeometry()
        {
            InitializeComponent();
            var layer = MyMapView.Map.Layers["RecreationalArea"] as ArcGISDynamicMapServiceLayer;
            layer.VisibleLayers = new ObservableCollection<int>(new int[] { 2 });
        }

        /// <summary>
        /// Identifies feature to highlight.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            // Ignore tap events while in edit mode so we do not interfere with edit geometry.
            var inEditMode = EditButton.IsEnabled;
            if (inEditMode)
                return;

            var layer = MyMapView.Map.Layers["RecreationalArea"] as ArcGISDynamicMapServiceLayer;
            var task = new IdentifyTask(new Uri(layer.ServiceUri));
            var mapPoint = MyMapView.ScreenToLocation(e.Position);

            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
            var parameter = new IdentifyParameters(mapPoint, viewpointExtent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);

            // Clears map of any highlights.
            var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
            overlay.Graphics.Clear();

            SetGeometryEditor();

            string message = null;
            try
            {
                // Performs an identify and adds feature result as selected into overlay.
                var result = await task.ExecuteAsync(parameter);
                if (result == null || result.Results == null || result.Results.Count < 1)
                    return;
                var graphic = (Graphic)result.Results[0].Feature;
                graphic.IsSelected = true;
                overlay.Graphics.Add(graphic);

                // Prepares geometry editor.
                var featureID = Convert.ToInt64(graphic.Attributes["Objectid"], CultureInfo.InvariantCulture);
                SetGeometryEditor(featureID);
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
        private void SetGeometryEditor(long featureID = 0)
        {
            EditButton.Tag = featureID;
            EditButton.IsEnabled = featureID == 0 ? false : true;
        }

        /// <summary>
        /// Enables geometry editing, submits geometry edit back to the server and refreshes dynamic layer.
        /// </summary>
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Map.Layers["RecreationalArea"] as ArcGISDynamicMapServiceLayer;
            var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
            var featureID = (long)EditButton.Tag;
            string message = null;
            try
            {
                // Hides graphic from overlay and dynamic layer while its geometry is being modified.
                overlay.Graphics.Clear();
                // Negative IDs indicate they do not exist on server yet.
                if (featureID > 0)
                {
                    if (layer.LayerDefinitions == null)
                        layer.LayerDefinitions = new ObservableCollection<LayerDefinition>();
                    else
                        layer.LayerDefinitions.Clear();
                    layer.LayerDefinitions.Add(new LayerDefinition()
                    {
                        LayerID = layer.VisibleLayers[0],
                        Definition = string.Format("Objectid <> {0}", featureID)
                    });
                }
                if (table == null)
                {
                    // Creates table based on visible layer of dynamic layer 
                    // using FeatureServer specifying shape field to enable editing.
                    var id = layer.VisibleLayers[0];
                    var url = layer.ServiceUri.Replace("MapServer", "FeatureServer");
                    url = string.Format("{0}/{1}", url, id);
                    table = await ServiceFeatureTable.OpenAsync(new Uri(url), null, MyMapView.SpatialReference);
                }
                // Retrieves feature identified by ID and updates its geometry 
                // using GeometryEngine to correct ring orientation.
                var feature = await table.QueryAsync(featureID);
                var geometry = await MyMapView.Editor.EditGeometryAsync(feature.Geometry);
                feature.Geometry = GeometryEngine.Simplify(geometry);
                await table.UpdateAsync(feature);
                if (table.HasEdits)
                {
                    // Pushes geometry edits back to the server.
                    var result = await table.ApplyEditsAsync();
                    if (result.UpdateResults == null || result.UpdateResults.Count < 1)
                        return;
                    var updateResult = result.UpdateResults[0];
                    if (updateResult.Error != null)
                        message = updateResult.Error.Message;
                }

            }
            catch (TaskCanceledException te)
            {
                // Handles canceling out of Editor.
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                SetGeometryEditor();
                if (layer.LayerDefinitions != null)
                {
                    layer.LayerDefinitions.Clear();
                    // Refreshes layer to reflect geometry edits.
                    layer.Invalidate();
                }
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }
    }
}
