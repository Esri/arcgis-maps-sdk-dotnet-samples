using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to use a custom Server Object Extension (SOE) hosted by ArcGIS for Server. The SOE operation used in this example returns a set of interpolated elevation values for a user defined grid. The grid extent and number of rows and columns are provided as inputs to the SOE.
	/// </summary>
	/// <title>SOE Elevation Data</title>
	/// <category>Extras</category>
	public partial class SOEElevationData : Page
	{
		private HttpClient httpClient;
		private List<Color> colorRanges = new List<Color>();
		private GraphicsLayer _graphicsLayer;


		// Construct SOE Elevation Data sample control
		public SOEElevationData()
		{
			InitializeComponent();
			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;


			colorRanges.Add(Colors.Blue);
			colorRanges.Add(Colors.Green);
			colorRanges.Add(Colors.Yellow);
			colorRanges.Add(Colors.Orange);
			colorRanges.Add(Colors.Red);
		}

		// Process elevation data requests on button click
		private async void GetElevationDataButton_Click(object sender, RoutedEventArgs e)
		{
			ElevationView.Visibility = Visibility.Collapsed;
			_graphicsLayer.Graphics.Clear();

			await ProcessUserRequests();
		}

		// Process elevation data requests by the user
		private async Task ProcessUserRequests()
		{
			try
			{
				// Get user rectangle
				var userEnvelope = await MyMapView.Editor.RequestShapeAsync(DrawShape.Envelope) as Envelope;
				if (userEnvelope.Height == 0 || userEnvelope.Width == 0)
					throw new ArgumentNullException("Please click and drag a box to define an extent.");

				// Display the graphics
				_graphicsLayer.Graphics.Add(new Graphic(userEnvelope));


				// Take account of WrapAround
				var polygon = GeometryEngine.NormalizeCentralMeridian(userEnvelope) as Polygon;
				Envelope envelope = polygon.Extent;

				// Retrieve elevation data from the service
				ElevationData elevationData = await GetElevationData(envelope);

				// Create the image for the display
				WriteableBitmap writeableBitmapElevation = await CreateElevationImageAsync(elevationData);
				ElevationImage.Source = writeableBitmapElevation;
				ElevationView.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}

		}

		// Call a REST SOE to get elevation data
		private async Task<ElevationData> GetElevationData(Envelope rect)
		{
			string SOEurl = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?";
			SOEurl += string.Format(CultureInfo.InvariantCulture,
				"Extent={{\"xmin\" : {0}, \"ymin\" : {1}, \"xmax\" : {2}, \"ymax\" :{3},\"spatialReference\" : {{\"wkid\" : {4}}}}}&Rows={5}&Columns={6}&f=json",
				rect.XMin, rect.YMin, rect.XMax, rect.YMax,
				MyMapView.SpatialReference.Wkid, HeightTextBox.Text, WidthTextBox.Text);

			/* 
			 * e.g. 
			 * http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?
			 * Extent={"xmin" : -1507310.85889877, "ymin" : 6406071.15031974, "xmax" : 879264.66769094, "ymax" :8164600.48570163,"spatialReference" : {"wkid" : 102100}}&Rows=15&Columns=15&f=json
			*/		


			httpClient = new HttpClient();
			var stream = await httpClient.GetStreamAsync(SOEurl);

			DataContractJsonSerializer serializer = null;
			ElevationData elevationData = null;

			try
			{
				serializer = new DataContractJsonSerializer(typeof(ElevationData));
				elevationData = serializer.ReadObject(stream) as ElevationData;
			}
			catch
			{
				// Check for a service error.
				serializer = new DataContractJsonSerializer(typeof(ServiceError));
				ServiceError serviceError = serializer.ReadObject(stream) as ServiceError;

				// e.g. {"error":{"code":400,"message":"Value cannot be null.\r\nParameter name: Rows"}}
				var _x = new MessageDialog(string.Format("Error: {0} meters", serviceError.error.message));
			}

			if (elevationData.data == null)
			{
				var _x = new MessageDialog("No Data Returned. Please try again.").ShowAsync();
			}
			return elevationData;
		}

		// Create a bitmap image from the elevation data
		private async Task<WriteableBitmap> CreateElevationImageAsync(ElevationData elevationData)
		{
			int thematicMin = elevationData.data[0];
			int thematicMax = elevationData.data[0];
			foreach (int elevValue in elevationData.data)
			{
				if (elevValue < thematicMin)
					thematicMin = elevValue;
				if (elevValue > thematicMax)
					thematicMax = elevValue;
			}

			int totalRange = thematicMax - thematicMin;
			int portion = totalRange / 5;
			List<Color> cellColor = new List<Color>();
			foreach (int elevValue in elevationData.data)
			{
				int startValue = thematicMin;
				for (int i = 0; i < 5; i++)
				{
					if (Enumerable.Range(startValue, portion).Contains(elevValue))
					{
						cellColor.Add(colorRanges[i]);
						break;
					}
					else if (i == 4)
						cellColor.Add(colorRanges.Last());

					startValue = startValue + portion;
				}
			}

			int rows = Convert.ToInt32(HeightTextBox.Text);
			int cols = Convert.ToInt32(WidthTextBox.Text);

			byte[] pixelData = new byte[rows * cols * 4];
			int cell = 0;
			int pos = 0;
			for (int x = 0; x < rows; x++)
			{
				for (int y = 0; y < cols; y++)
				{
					Color color = cellColor[cell++];
					pixelData[pos++] = color.B;
					pixelData[pos++] = color.G;
					pixelData[pos++] = color.R;
					pixelData[pos++] = (byte)255;
				}
			}
			WriteableBitmap writeableBitmapElevation = new WriteableBitmap(rows, cols);
			Stream stream = writeableBitmapElevation.PixelBuffer.AsStream();
			await stream.WriteAsync(pixelData, 0, pixelData.Length);
			stream.Flush();
			return writeableBitmapElevation;
		}
	}

#pragma warning disable 1591

	// Classes for serialization in the case of elevation data 
	
	public class RasterProperties
	{
		public bool IsInteger { get; set; }
		public int datasetMin { get; set; }
		public int datasetMax { get; set; }
	}

	public class ElevationData
	{
		public int nCols { get; set; }
		public int nRows { get; set; }
		public double xLLCenter { get; set; }
		public double yLLCenter { get; set; }
		public double cellSize { get; set; }
		public List<int> noDataValue { get; set; }
		// public SpatialReference spatialReference { get; set; }
		public List<int> data { get; set; }
		public RasterProperties rasterProperties { get; set; }
	}

	// Classes for serialization in the case of a service error
	public class Error
	{
		public int code { get; set; }
		public string message { get; set; }
	}

	public class ServiceError
	{
		public Error error { get; set; }
	}

#pragma warning restore 1591
}
