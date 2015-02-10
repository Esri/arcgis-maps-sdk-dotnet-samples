using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates performing an attribute query, adding the results to the map and UI, and zooming to the resulting geometry.
	/// </summary>
	/// <title>Attribute Query</title>
	/// <category>Query Tasks</category>
	public sealed partial class AttributeQuery : Page
	{
		public AttributeQuery()
		{
			this.InitializeComponent();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000));
			InitializeComboBox();
		}

		private async void InitializeComboBox()
		{
			QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));


			Query query = new Query("1=1")
			{
				ReturnGeometry = false,
			};
			query.OutFields.Add("STATE_NAME");

			try
			{
				var result = await queryTask.ExecuteAsync(query);
				QueryComboBox.ItemsSource = result.FeatureSet.Features.OrderBy(x => x.Attributes["STATE_NAME"]);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			await GetAttributes();
		}

		private async Task GetAttributes()
		{
			QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

			var qryText = (string)(QueryComboBox.SelectedItem as Graphic).Attributes["STATE_NAME"];
			Query query = new Query(qryText)
			{
				OutFields = OutFields.All,
				ReturnGeometry = true,
				OutSpatialReference = MyMapView.SpatialReference
			};
			try
			{
				ResultsGrid.ItemsSource = null;
				progress.IsActive = true;
				var result = await queryTask.ExecuteAsync(query);
				var featureSet = result.FeatureSet;
				// If an item has been selected            
				GraphicsLayer graphicsLayer = MyMapView.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
				graphicsLayer.Graphics.Clear();

				if (featureSet != null && featureSet.Features.Count > 0)
				{
					var symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol;
					var g = featureSet.Features[0];
					graphicsLayer.Graphics.Add(g as Graphic);
					var selectedFeatureExtent = g.Geometry.Extent;
					Envelope displayExtent = selectedFeatureExtent.Expand(1.3);
					MyMapView.SetView(displayExtent);
					ResultsGrid.ItemsSource = g.Attributes;
				}
			}
			catch (Exception ex)
			{

				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			finally
			{
				progress.IsActive = false;
			}
		}

		private void KeyLoaded(object sender, object e)
		{
			TextBlock textBlock = (TextBlock)sender;
			dynamic dyn = textBlock.DataContext;
			textBlock.Text = dyn.Key;
		}

		private void ValueLoaded(object sender, object e)
		{
			TextBlock textBlock = (TextBlock)sender;
			dynamic dyn = textBlock.DataContext;
			textBlock.Text = Convert.ToString(dyn.Value, CultureInfo.InvariantCulture);
		}
	}
}