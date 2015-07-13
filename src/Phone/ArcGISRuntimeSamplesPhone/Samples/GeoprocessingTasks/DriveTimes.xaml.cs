using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the Geoprocessor to call a DriveTimes geoprocessing service. To use the sample, click a point in the map. Drive time polygons of 1, 2, and 3 minutes will be calculated and displayed on the map.
	/// </summary>
	/// <title>Drive Times</title>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class DriveTimes : Page
	{
		public DriveTimes()
		{
			InitializeComponent();
			InitializePMS();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-122.5009, 37.741, -122.3721, 37.8089));
		}

		private async void InitializePMS()
		{
			try
			{
				var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesPhone/Assets/car-red-16x16.png"));
				var imageSource = await imageFile.OpenReadAsync();
				var pms = LayoutRoot.Resources["DefaultMarkerSymbol"] as PictureMarkerSymbol;
				await pms.SetSourceAsync(imageSource);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			// Convert screen point to map point
			var mapPoint = MyMapView.ScreenToLocation(e.Position);
			var layer = MyMapView.Map.Layers["InputLayer"] as GraphicsLayer;
			layer.Graphics.Clear();
			layer.Graphics.Add(new Graphic() { Geometry = mapPoint });

			string error = null;
			Geoprocessor task = new Geoprocessor(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons"));

			var parameter = new GPInputParameter();

			parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Location", mapPoint));
			parameter.GPParameters.Add(new GPString("Drive_Times", "1 2 3"));

			try
			{
				var result = await task.ExecuteAsync(parameter);
				var r = MyMapView.Map.Layers["ResultLayer"] as GraphicsLayer;
				r.Graphics.Clear();
				foreach (GPParameter gpParameter in result.OutParameters)
				{
					if (gpParameter is GPFeatureRecordSetLayer)
					{
						GPFeatureRecordSetLayer gpLayer = gpParameter as GPFeatureRecordSetLayer;
						List<Esri.ArcGISRuntime.Symbology.Symbol> bufferSymbols = new List<Esri.ArcGISRuntime.Symbology.Symbol>(
							  new Esri.ArcGISRuntime.Symbology.Symbol[] { LayoutRoot.Resources["FillSymbol1"] as Esri.ArcGISRuntime.Symbology.Symbol, 
								  LayoutRoot.Resources["FillSymbol2"] as Esri.ArcGISRuntime.Symbology.Symbol, 
								  LayoutRoot.Resources["FillSymbol3"] as Esri.ArcGISRuntime.Symbology.Symbol });

						int count = 0;
						foreach (Graphic graphic in gpLayer.FeatureSet.Features)
						{
							graphic.Symbol = bufferSymbols[count];
							graphic.Attributes.Add("Info", String.Format("{0} minute buffer ", 3 - count));
							r.Graphics.Add(graphic);
							count++;
						}
					}
				}
			}
			catch (Exception ex)
			{
				error = "Geoprocessor service failed: " + ex.Message;
			}
			if (error != null)
				await new MessageDialog(error).ShowAsync();
		}
	  
	}
}