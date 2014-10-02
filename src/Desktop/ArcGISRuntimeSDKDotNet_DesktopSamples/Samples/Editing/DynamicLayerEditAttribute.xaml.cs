using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Esri.ArcGISRuntime.Data;
using System.Globalization;
using System.Collections.Generic;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to selectively import and update feature attributes while using dynamic layer to render the features.
    /// </summary>
    /// <title>Dynamic Layer Edit Attribute</title>
    /// <category>Editing</category>
    public partial class DynamicLayerEditAttribute : UserControl
    {
        public DynamicLayerEditAttribute()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Builds choice list for attribute editing from layer metadata.
        /// </summary>
        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is ArcGISDynamicMapServiceLayer))
                return;
            var layer = (ArcGISDynamicMapServiceLayer)e.Layer;
            var id = layer.VisibleLayers[0];
            string message = null;
            try
            {
                // Gets service metadata for specific layer.
                var details = await layer.GetDetailsAsync(id);
                if (details == null || details.Fields == null)
                    return;
                // Extracts field information for attribute.
                var field = details.Fields.FirstOrDefault(f => f.Name == "has_pool");
                if (field == null || field.Domain == null || !(field.Domain is CodedValueDomain))
                    return;
                // Uses domain information to populate choice list.
                var codedValueDomain = (CodedValueDomain)field.Domain;
                ChoiceList.ItemsSource = codedValueDomain.CodedValues;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        /// <summary>
        /// Identifies feature to highlight.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            // Builds parameters based on tapped location and map view properties.
            var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
            var task = new IdentifyTask(new Uri(layer.ServiceUri));
            var mapPoint = MyMapView.ScreenToLocation(e.Position);
            var parameter = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);

            // Clears map of any highlights.
            var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
            overlay.Graphics.Clear();

            SetAttributeEditor();

            string message = null;
            try
            {
                // Performs an identify and adds feature result as selected into GraphicsOverlay.
                var result = await task.ExecuteAsync(parameter);
                if (result == null || result.Results == null || result.Results.Count < 1)
                    return;
                var graphic = (Graphic)result.Results[0].Feature;
                graphic.IsSelected = true;
                overlay.Graphics.Add(graphic);
                SetAttributeEditor(graphic);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        /// <summary>
        /// Displays feature attributes.
        /// </summary>
        private void SetAttributeEditor(Graphic graphic = null)
        {
            if (graphic != null)
            {
                var featureID = Convert.ToInt64(graphic.Attributes["OBJECTID"], CultureInfo.InvariantCulture);
                var hasPool = Convert.ToString(graphic.Attributes["Has_Pool"], CultureInfo.InvariantCulture);
                var lookup = (IReadOnlyDictionary<object, string>)ChoiceList.ItemsSource;
                var selected = (from item in lookup
                                where string.Equals(item.Value, hasPool)
                                select item).FirstOrDefault();
                ChoiceList.SelectedItem = selected;
                ChoiceList.Tag = featureID;
                AttributeEditor.Visibility = Visibility.Visible;
            }
            else
                AttributeEditor.Visibility = Visibility.Collapsed;
        }
                
        /// <summary>
        /// Submits changes to server and refreshes dynamic layer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var featureID = (Int64)ChoiceList.Tag;
            var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
            // Builds service URL based on visible layer.
            var id = layer.VisibleLayers[0];  
            var url = layer.ServiceUri.Replace("MapServer", "FeatureServer");
            url = string.Format("{0}/{1}", url, id);
            string message = null;
            try
            {
                // Creates the table and specify fields for editing in the out fields.
                var table = await ServiceFeatureTable.OpenAsync(new Uri(url), null, MyMapView.SpatialReference);
                table.OutFields = new OutFields(new string[] { table.ServiceInfo.TypeIdField });
                // Caches the feature.
                var feature = await table.QueryAsync(featureID);
                // Updates feature attribute.
                var item = (KeyValuePair<object, string>)ChoiceList.SelectedItem;
                feature.Attributes[table.ServiceInfo.TypeIdField] = item.Key;
                // Updates the cached feature.
                await table.UpdateAsync(feature);
                if (table.HasEdits)
                {
                    // Pushes attribute changes back to the server.
                    var result = await table.ApplyEditsAsync();
                    if (result.UpdateResults == null || result.UpdateResults.Count < 1)
                        return;
                    var updateResult = result.UpdateResults[0];
                    if (updateResult.Error != null || !updateResult.Success)
                        message = updateResult.Error != null ? updateResult.Error.Message : string.Format("Update to feature attribute [{0}] failed on server.", featureID);
                    // Refreshes layer to reflect attribute change.
                    layer.Invalidate();
                }
               
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                SetAttributeEditor();
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
    }
}
