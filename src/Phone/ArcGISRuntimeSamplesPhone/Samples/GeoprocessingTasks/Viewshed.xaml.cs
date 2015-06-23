using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the Geoprocessor to call a Viewshed geoprocessing service.
	/// </summary>
	/// <title>Viewshed</title>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class CalculateViewshed : Page
	{
		GraphicsLayer inputLayer;
		GraphicsLayer viewshedLayer;
		public CalculateViewshed()
		{
			InitializeComponent();
			InitializePMS();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-12004036, 4652780, -11735714, 4808810));
			inputLayer = MyMapView.Map.Layers["InputLayer"] as GraphicsLayer;
			viewshedLayer = MyMapView.Map.Layers["viewShedLayer"] as GraphicsLayer;
		}

		private async void InitializePMS()
		{
			try
			{
				var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesPhone/Assets/i_pushpin.png"));
				var imageSource = await imageFile.OpenReadAsync();
				var pms = LayoutRoot.Resources["StartMarkerSymbol"] as PictureMarkerSymbol;
				await pms.SetSourceAsync(imageSource);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
		{
			//remove all previous results
			inputLayer.Graphics.Clear();
			viewshedLayer.Graphics.Clear();
		}

		private async void StartButton_Click(object sender, RoutedEventArgs e)
		{
			StartButton.IsEnabled = false;
			ClearResultsButton.Visibility = Visibility.Collapsed;

			//get the user's input point
			var inputPoint = await MyMapView.Editor.RequestPointAsync();

			//update UI elements
			MyProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
			MyProgressRing.IsActive = true;

			viewshedLayer.Graphics.Clear();

			inputLayer.Graphics.Clear();
			inputLayer.Graphics.Add(new Graphic() { Geometry = inputPoint });

			Geoprocessor gpTask = new Geoprocessor(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed"));

			var parameter = new GPInputParameter() { OutSpatialReference = SpatialReferences.WebMercator };
			parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Observation_Point", inputPoint));
			parameter.GPParameters.Add(new GPLinearUnit("Viewshed_Distance ", LinearUnits.Miles, Convert.ToDouble(MilesTextBox.Text)));

			var result = await gpTask.ExecuteAsync(parameter);

			if (result == null || result.OutParameters == null || !(result.OutParameters[0] is GPFeatureRecordSetLayer))
				throw new Exception("No viewshed graphics returned for this start point.");

			var viewshedResult = result.OutParameters[0] as GPFeatureRecordSetLayer;
			viewshedLayer.Graphics.AddRange(viewshedResult.FeatureSet.Features.OfType<Graphic>());

			//Reset the UI
			StatusTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			StartButton.IsEnabled = true;
			ClearResultsButton.Visibility = Visibility.Visible;
			MyProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			MyProgressRing.IsActive = false;
		}
	}
}
