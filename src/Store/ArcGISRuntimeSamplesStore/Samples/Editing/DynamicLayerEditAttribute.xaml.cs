using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to selectively import and update feature attributes in dynamic layer.
	/// </summary>
	/// <title>Dynamic Layer Edit Attribute</title>
	/// <category>Editing</category>
	public partial class DynamicLayerEditAttribute : Page
	{
		// Editing is done through this table.
		private ServiceFeatureTable table;
		// Used to populate choice list for editing field.
		private IReadOnlyDictionary<object, string> choices;

		public DynamicLayerEditAttribute()
		{
			InitializeComponent();
			var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
			layer.VisibleLayers = new ObservableCollection<int>(new int[] { 0 });
		}

		/// <summary>
		/// Identifies feature to highlight.
		/// </summary>
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
			var task = new IdentifyTask(new Uri(layer.ServiceUri));
			var mapPoint = MyMapView.ScreenToLocation(e.Position);
            
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
			var parameter = new IdentifyParameters(mapPoint, viewpointExtent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);

			// Clears map of any highlights.
			var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
			overlay.Graphics.Clear();

			SetAttributeEditor();

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

				// Prepares attribute editor.
				var featureID = Convert.ToInt64(graphic.Attributes["OBJECTID"], CultureInfo.InvariantCulture);
				var hasPool = Convert.ToString(graphic.Attributes["Has_Pool"], CultureInfo.InvariantCulture);
				if (choices == null)
					choices = await GetChoicesAsync();
				SetAttributeEditor(featureID, hasPool);
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			if (!string.IsNullOrWhiteSpace(message))
				await new MessageDialog(message).ShowAsync();
		}

		/// <summary>
		/// Returns choice list for attribute editing.
		/// </summary>
		private async Task<IReadOnlyDictionary<object, string>> GetChoicesAsync()
		{
			var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
			var id = layer.VisibleLayers[0];
			string message = null;
			try
			{
				// Gets service metadata for specific layer and extract field information for attribute editing.
				var details = await layer.GetDetailsAsync(id);
				if (details != null && details.Fields != null)
				{
					var field = details.Fields.FirstOrDefault(f => f.Name == "has_pool");
					if (field.Domain is CodedValueDomain)
						return ((CodedValueDomain)field.Domain).CodedValues;
				}
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			if (!string.IsNullOrWhiteSpace(message))
				await new MessageDialog(message).ShowAsync();
			return null;
		}

		/// <summary>        
		/// Prepares AttributeEditor for editing.
		/// </summary>
		private void SetAttributeEditor(long featureID = 0, string hasPool = null)
		{
			if (ChoiceList.ItemsSource == null && choices != null)
			{
				ChoiceList.ItemsSource = from item in choices
										 select item.Value;
			}
			if (!string.IsNullOrWhiteSpace(hasPool) && choices != null)
			{
				var selected = choices.FirstOrDefault(item => string.Equals(item.Value, hasPool)).Value;
				ChoiceList.SelectedItem = selected;
			}
			EditButton.Tag = featureID;
			EditButton.IsEnabled = featureID == 0 ? false : true;
			if(featureID > 0)
				ChoiceList.SelectionChanged += ChoiceList_SelectionChanged;
			else
				EditButton.Flyout.Hide();
		}

		/// <summary>
		/// Enables attribute editing, submits attribute edit back to the server and refreshes dynamic layer.
		/// </summary>
		private async void ChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ChoiceList.SelectionChanged -= ChoiceList_SelectionChanged;
			var featureID = (Int64)EditButton.Tag;
			var selected = (string)ChoiceList.SelectedItem;
			var layer = MyMapView.Map.Layers["PoolPermit"] as ArcGISDynamicMapServiceLayer;
			var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
			string message = null;
			try
			{
				if (table == null)
				{
					// Creates table based on visible layer of dynamic layer 
					// using FeatureServer specifying has_pool field to enable editing.
					var id = layer.VisibleLayers[0];
					var url = layer.ServiceUri.Replace("MapServer", "FeatureServer");
					url = string.Format("{0}/{1}", url, id);
					table = await ServiceFeatureTable.OpenAsync(new Uri(url), null, MyMapView.SpatialReference);
					table.OutFields = new OutFields(new string[] { "has_pool" });
				}
				// Retrieves feature identified by ID and updates its attributes.
				var feature = await table.QueryAsync(featureID);
				if (choices != null)
				{
					var value = choices.FirstOrDefault(item => string.Equals(item.Value, selected)).Key;
					feature.Attributes["has_pool"] = value;
					await table.UpdateAsync(feature);
				}
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
				overlay.Graphics.Clear();
				SetAttributeEditor();
			}
			if (!string.IsNullOrWhiteSpace(message))
				await new MessageDialog(message).ShowAsync();
		}
	}
}
