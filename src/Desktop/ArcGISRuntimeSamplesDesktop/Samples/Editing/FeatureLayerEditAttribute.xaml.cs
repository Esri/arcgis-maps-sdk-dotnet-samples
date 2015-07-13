using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to update feature attributes in feature layer.
	/// </summary>
	/// <title>Feature Layer Edit Attribute</title>
	/// <category>Editing</category>
	public partial class FeatureLayerEditAttribute : UserControl
	{
		// Used to populate choice list for editing field.
		private IEnumerable<KeyValuePair<object, string>> choices;

		public FeatureLayerEditAttribute()
		{
			InitializeComponent();
		}
		
		/// <summary>
		/// Selects feature for editing.
		/// </summary>
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			var layer = MyMapView.Map.Layers["Incidents"] as FeatureLayer;
			layer.ClearSelection();
			SetAttributeEditor();
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
				SetAttributeEditor(feature);
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			if (!string.IsNullOrWhiteSpace(message))
				MessageBox.Show(message);
		}

		/// <summary>
		/// Returns choice list for attribute editing.
		/// </summary>
		private IEnumerable<KeyValuePair<object, string>> GetChoices()
		{
			var layer = MyMapView.Map.Layers["Incidents"] as FeatureLayer;
			var table = (ArcGISFeatureTable)layer.FeatureTable;
			// Gets service metadata from table and extract type field information for attribute editing.            
			// Since req_type is also the type id field and no domain was specified in the service, use Types.
			if (!string.IsNullOrWhiteSpace(table.ServiceInfo.TypeIdField) && table.ServiceInfo.Types != null)
			{
				return  from t in table.ServiceInfo.Types
						select new KeyValuePair<object, string>(t.ID, t.Name);
			}
			return null;
		}

		/// <summary>        
		/// Prepares AttributeEditor for editing.
		/// </summary>
		private void SetAttributeEditor(Feature feature = null)
		{
			if (choices == null)
				choices = GetChoices();
			if (ChoiceList.ItemsSource == null && choices != null)
				ChoiceList.ItemsSource = from item in choices
										 select item.Key;
			AttributeEditor.Tag = feature;
			AttributeEditor.Visibility = feature == null ? Visibility.Collapsed : Visibility.Visible;
			if (feature != null)
			{
				var reqType = Convert.ToString(feature.Attributes["req_type"], CultureInfo.InvariantCulture);
				if (!string.IsNullOrWhiteSpace(reqType) && choices != null)
				{
					var selected = choices.FirstOrDefault(item => string.Equals(Convert.ToString(item.Key), reqType)).Key;
					ChoiceList.SelectedItem = selected;
				}
				ChoiceList.SelectionChanged += ChoiceList_SelectionChanged;
			}
		}
				
		/// <summary>
		/// Enables attribute editing and submits attribute edit back to the server.
		/// </summary>
		private async void ChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ChoiceList.SelectionChanged -= ChoiceList_SelectionChanged;
			var feature = (GeodatabaseFeature)AttributeEditor.Tag;
			var layer = MyMapView.Map.Layers["Incidents"] as FeatureLayer;
			var table = (ArcGISFeatureTable)layer.FeatureTable;
			string message = null;
			try
			{
				var item = (string)ChoiceList.SelectedItem;
				feature.Attributes["req_type"] = item;
				await table.UpdateAsync(feature);
				if (table.HasEdits)
				{
					if (table is ServiceFeatureTable)
					{
						var serviceTable = (ServiceFeatureTable)table;
						// Pushes attribute edits back to the server.
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
				layer.ClearSelection();
				SetAttributeEditor();
			}
			if (!string.IsNullOrWhiteSpace(message))
				MessageBox.Show(message);
		}
	}
}
