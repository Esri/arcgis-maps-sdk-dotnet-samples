using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
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
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000));
		}

		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			await RunIdentify(e.Location);
		}

		private async Task RunIdentify(MapPoint mp)
		{
			// Get current viewpoints extent from the MapView
			var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
			var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
			IdentifyParameters identifyParams = new IdentifyParameters(mp, viewpointExtent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth)
			{
				LayerOption = LayerOption.Visible,
				SpatialReference = MyMapView.SpatialReference,
			};

			IdentifyTask identifyTask = new IdentifyTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
				"Demographics/ESRI_Census_USA/MapServer"));
			progress.IsActive = true;

			try
			{
				TitleComboBox.ItemsSource = null;
				ResultsGrid.ItemsSource = null;
				var result = await identifyTask.ExecuteAsync(identifyParams);

				GraphicsLayer graphicsLayer = MyMapView.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
				graphicsLayer.Graphics.Clear();
				graphicsLayer.Graphics.Add(new Graphic() { Geometry = mp });

				var _dataItems = new List<DataItem>();
				if (result != null && result.Results != null && result.Results.Count > 0)
				{
					foreach (var r in result.Results)
					{
						Feature feature = r.Feature;
						string title = r.Value.ToString() + " (" + r.LayerName + ")";
						_dataItems.Add(new DataItem()
						{
							Title = title,
							Data = feature.Attributes
						});
					}
				}
				TitleComboBox.ItemsSource = _dataItems;
				if (_dataItems.Count > 0)
					TitleComboBox.SelectedIndex = 0;
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

		private void TitleComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
		{
			int index = TitleComboBox.SelectedIndex;
			var _dataItems = TitleComboBox.ItemsSource as IList<DataItem>;
			if (index > -1)
				ResultsGrid.ItemsSource = _dataItems[index].Data;
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

	public class DataItem : INotifyPropertyChanged
	{
		private string title;
		public string Title
		{
			get { return title; }
			set
			{
				if (title != value)
				{
					title = value;
					OnPropertyChanged("Title");
				}
			}
		}

		private IDictionary<string, object> data;
		public IDictionary<string, object> Data
		{
			get { return data; }
			set
			{
				if (data != value)
				{
					data = value;
					OnPropertyChanged("Data");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}