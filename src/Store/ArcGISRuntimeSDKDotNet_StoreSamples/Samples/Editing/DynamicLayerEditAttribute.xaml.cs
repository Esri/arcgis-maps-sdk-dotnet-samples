using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to selectively import and update feature attributes while using dynamic layer to render the features.
    /// </summary>
    /// <title>Dynamic Layer Edit Attribute</title>
    /// <category>Editing</category>
    public partial class DynamicLayerEditAttribute : Page
    {
        // Editing is done through this table.
        private ServiceFeatureTable table;

        public DynamicLayerEditAttribute()
        {
            InitializeComponent();
            var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
            layer.VisibleLayers = new ObservableCollection<int>(new int[] { 0 });
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
                // Gets service metadata for specific layer and extract field information for attribute editing.
                var details = await layer.GetDetailsAsync(id);
                if (details == null || details.Fields == null)
                    return;
                var field = details.Fields.FirstOrDefault(f => f.Name == "has_pool");
                if (field == null || field.Domain == null || !(field.Domain is CodedValueDomain))
                    return;
                var codedValueDomain = (CodedValueDomain)field.Domain;
                ChoiceList.ItemsSource = from item in codedValueDomain.CodedValues
                                         select item.Value;
                ChoiceList.Tag = codedValueDomain.CodedValues;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Identifies feature to highlight.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
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
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Displays feature attributes.
        /// </summary>
        private void SetAttributeEditor(Graphic graphic = null)
        {
            if (graphic != null)
            {
                var hasPool = Convert.ToString(graphic.Attributes["Has_Pool"], CultureInfo.InvariantCulture);
                var lookup = (IReadOnlyDictionary<object, string>)ChoiceList.Tag;
                var selected = (from item in lookup
                                where string.Equals(item.Value, hasPool)
                                select item.Value).FirstOrDefault();
                ChoiceList.SelectedItem = selected;
                var featureID = Convert.ToInt64(graphic.Attributes["OBJECTID"], CultureInfo.InvariantCulture);
                AttributeEditor.Tag = featureID;
                AttributeEditor.IsEnabled = true;
            }
            else
            {
                AttributeEditor.IsEnabled = false;
                AttributeEditor.Flyout.Hide();
            }
        }

        /// <summary>
        /// Submits changes to server and refreshes dynamic layer.
        /// </summary>
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var featureID = (Int64)AttributeEditor.Tag;
            var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
            string message = null;
            try
            {
                if (table == null)
                {
                    // Creates table based on visible layer of dynamic layer 
                    // using FeatureServer to enable editing and specifying the fields for editing.
                    var id = layer.VisibleLayers[0];
                    var url = layer.ServiceUri.Replace("MapServer", "FeatureServer");
                    url = string.Format("{0}/{1}", url, id);
                    table = await ServiceFeatureTable.OpenAsync(new Uri(url), null, MyMapView.SpatialReference);
                    table.OutFields = new OutFields(new string[] { "has_pool" });
                }
                // Retrieves feature identified by ID and update its attributes.
                var feature = await table.QueryAsync(featureID);
                var hasPool = Convert.ToString(ChoiceList.SelectedItem, CultureInfo.InvariantCulture);
                var lookup = (IReadOnlyDictionary<object, string>)ChoiceList.Tag;
                var selected = (from item in lookup
                                where string.Equals(item.Value, hasPool)
                                select item).FirstOrDefault();
                feature.Attributes["has_pool"] = selected.Key;
                await table.UpdateAsync(feature);
                if (table.HasEdits)
                {
                    // Pushes attribute edits back to the server.
                    var result = await table.ApplyEditsAsync();
                    if (result.UpdateResults == null || result.UpdateResults.Count < 1)
                        return;
                    var updateResult = result.UpdateResults[0];
                    if (updateResult.Error != null)
                        message = updateResult.Error.Message;
                    // Refreshes layer to reflect attribute edits.
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
                await new MessageDialog(message).ShowAsync();
        }
    }
}
