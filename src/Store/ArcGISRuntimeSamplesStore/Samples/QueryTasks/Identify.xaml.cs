using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates performing identify operations.
	/// </summary>
	/// <title>Identify</title>
	/// <category>Query Tasks</category>
	public sealed partial class Identify : Page
	{
		public Identify()
		{
			this.InitializeComponent();
		}

		private async void MyMapView_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			try
			{
				progress.Visibility = Visibility.Visible;
				resultsGrid.DataContext = null;

				GraphicsOverlay graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
				graphicsOverlay.Graphics.Clear();
				graphicsOverlay.Graphics.Add(new Graphic(e.Location));

				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				IdentifyParameters identifyParams = new IdentifyParameters(e.Location, viewpointExtent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth)
				{
					LayerOption = LayerOption.Visible,
					SpatialReference = MyMapView.SpatialReference,
				};

				IdentifyTask identifyTask = new IdentifyTask(
					new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer"));

				var result = await identifyTask.ExecuteAsync(identifyParams);

				resultsGrid.DataContext = result.Results;
				if (result != null && result.Results != null && result.Results.Count > 0)
					titleComboBox.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}

		private void TitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (titleComboBox.SelectedIndex >= 0)
			{
				var items = titleComboBox.ItemsSource as IReadOnlyList<IdentifyItem>;
				var item = items[titleComboBox.SelectedIndex];
				attributesList.ItemsSource = item.Feature.Attributes
					.Select(attr => new Tuple<string, string>(attr.Key, attr.Value.ToString()));
			}
		}
	}
}