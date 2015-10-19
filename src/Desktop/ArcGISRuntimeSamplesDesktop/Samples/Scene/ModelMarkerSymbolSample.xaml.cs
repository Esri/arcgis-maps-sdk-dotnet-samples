using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology.SceneSymbology;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to display a ModelMarkerSymbol
	/// </summary>
	/// <title>Model Marker Symbol</title>
	/// <category>Scene</category>
	/// <subcategory>Symbology</subcategory>
	public partial class ModelMarkerSymbolSample : UserControl
	{
		public ModelMarkerSymbolSample()
		{
			InitializeComponent();
			var _ = AddModelMarkerSymbol();
		}

		private async Task AddModelMarkerSymbol()
		{
			// Instantiate a new ModelMarkerSymbol
			ModelMarkerSymbol modelMarkerSymbol = new ModelMarkerSymbol();

			// Set the SourceUri property
			modelMarkerSymbol.SourceUri = Path.Combine(AssemblyDirectory, @"Samples\Scene\PT-Boat-Model\PTBoat.obj");

			// Increase the scale to achieve the desired visual size
			modelMarkerSymbol.Scale = 50;

			// Create a new MapPoint for the location
			MapPoint mapPoint = new MapPoint(-155, 19, -100);

			// Create a new Graphic to display the model symbol
			Graphic graphic = new Graphic(mapPoint);

			// Set the Graphic Symbol property
			graphic.Symbol = modelMarkerSymbol;

			// Create a GraphicsOverlay to contain the symbolized Graphic
			GraphicsOverlay graphicsoverlay = new GraphicsOverlay()
			{
				RenderingMode = GraphicsRenderingMode.Dynamic,
				SceneProperties = new LayerSceneProperties() { SurfacePlacement = SurfacePlacement.Relative }
			};
			// Add the Graphic to the GraphicsOverlay
			graphicsoverlay.Graphics.Add(graphic);

			// Add the GraphicsOverlay to the MapView's GraphicsOverlays collection
			MySceneView.GraphicsOverlays.Add(graphicsoverlay);

			// Set the Viewpoint
			await MySceneView.SetViewAsync(new Camera(mapPoint.Y - 0.5, mapPoint.X + 0.5, 25000, 315, 70));
		}

		private void MySceneView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			Debug.WriteLine(string.Format("Error while loading layer : {0} - {1}", e.Layer.ID, e.LoadError.Message));
		}

		/// <summary>
		/// Gets the assembly directory.
		/// </summary>
		/// <value>
		/// The assembly directory.
		/// </value>
		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}
