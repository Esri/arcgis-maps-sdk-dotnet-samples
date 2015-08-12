using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates elevation exaggeration in a scene.
	/// </summary>
	/// <title>3D Elevation Exaggeration</title>
	/// <category>Scene</category>
	public sealed partial class ElevationExaggeration : Page
	{
		private List<MapPoint> _coordinates = new List<MapPoint>();
		private GraphicsLayer _absoluteModeGL;
		private GraphicsLayer _relativeModeGL;
		private GraphicsLayer _drapedModeGL;

		public ElevationExaggeration()
		{
			this.InitializeComponent();

			_coordinates.Add(new MapPoint(-106.981, 39.028, 6000, SpatialReferences.Wgs84));
			_coordinates.Add(new MapPoint(-106.956, 39.081, 6000, SpatialReferences.Wgs84));
			_coordinates.Add(new MapPoint(-106.869, 39.081, 6000, SpatialReferences.Wgs84));
			_coordinates.Add(new MapPoint(-106.879, 39.014, 6000, SpatialReferences.Wgs84));

			_absoluteModeGL = MySceneView.Scene.Layers["AbsoluteModeGraphicsLayer"] as GraphicsLayer;
			_relativeModeGL = MySceneView.Scene.Layers["RelativeModeGraphicsLayer"] as GraphicsLayer;
			_drapedModeGL = MySceneView.Scene.Layers["DrapedModeGraphicsLayer"] as GraphicsLayer;
		}


		private void MySceneView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
			{
				if (e.Layer.ID == "AGOLayer")
					MySceneView.SetView(new Camera(new MapPoint(-106.882128302391, 38.7658957449754, 12994.1727461051),
						358.607816178049, 70.0562968167998));
			}
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			ClearAll();

			string shapeTag = (string)((RadioButton)sender).Tag;
			switch (shapeTag)
			{
				case "Point":
					GraphicCollection absoluteGC = new GraphicCollection();
					GraphicCollection relativeGC = new GraphicCollection();
					GraphicCollection drapedGC = new GraphicCollection();

					foreach (MapPoint mp in _coordinates)
					{
						absoluteGC.Add(new Graphic(mp, GetPointSymbol(SurfacePlacement.Absolute)));
						relativeGC.Add(new Graphic(mp, GetPointSymbol(SurfacePlacement.Relative)));
						drapedGC.Add(new Graphic(mp, GetPointSymbol(SurfacePlacement.Draped)));
					}

					_absoluteModeGL.Graphics.AddRange(absoluteGC);
					_relativeModeGL.Graphics.AddRange(relativeGC);
					_drapedModeGL.Graphics.AddRange(drapedGC);

					break;
				case "Line":
					var line = new Polyline(_coordinates, SpatialReferences.Wgs84);
					_absoluteModeGL.Graphics.Add(new Graphic(line, GetPolylineSymbol(SurfacePlacement.Absolute)));
					_relativeModeGL.Graphics.Add(new Graphic(line, GetPolylineSymbol(SurfacePlacement.Relative)));
					_drapedModeGL.Graphics.Add(new Graphic(line, GetPolylineSymbol(SurfacePlacement.Draped)));
					break;
				case "Polygon":
					var polygon = new Polygon(_coordinates, SpatialReferences.Wgs84);
					_absoluteModeGL.Graphics.Add(new Graphic(polygon, GetPolygonSymbol(SurfacePlacement.Absolute)));
					_relativeModeGL.Graphics.Add(new Graphic(polygon, GetPolygonSymbol(SurfacePlacement.Relative)));
					_drapedModeGL.Graphics.Add(new Graphic(polygon, GetPolygonSymbol(SurfacePlacement.Draped)));
					break;
			}
		}

		private void ClearAll()
		{
			IEnumerable<GraphicsLayer> graphicLayers = MySceneView.Scene.Layers.Where(l => l is GraphicsLayer).Cast<GraphicsLayer>();
			foreach (GraphicsLayer gl in graphicLayers)
				gl.Graphics.Clear();
		}

		private Esri.ArcGISRuntime.Symbology.Symbol GetPolylineSymbol(SurfacePlacement mode)
		{
			SimpleLineSymbol sls = new SimpleLineSymbol()
			{
				Style = SimpleLineStyle.Solid,
				Color = mode == SurfacePlacement.Absolute ?
					Colors.Red : mode == SurfacePlacement.Draped ? Colors.Yellow : Colors.LightBlue,
				Width = 4
			};

			return sls;
		}

		private Esri.ArcGISRuntime.Symbology.Symbol GetPolygonSymbol(SurfacePlacement mode)
		{
			SimpleFillSymbol sfs = new SimpleFillSymbol()
			{
				Style = SimpleFillStyle.Solid,
				Color = mode == SurfacePlacement.Absolute ?
					Colors.Red : mode == SurfacePlacement.Draped ? Colors.Yellow : Colors.LightBlue
			};

			return sfs;
		}

		private Esri.ArcGISRuntime.Symbology.Symbol GetPointSymbol(SurfacePlacement mode)
		{
			SimpleMarkerSymbol sms = new SimpleMarkerSymbol()
			{
				Style = SimpleMarkerStyle.Circle,
				Color = mode == SurfacePlacement.Absolute ?
					Colors.Red : mode == SurfacePlacement.Draped ? Colors.Yellow : Colors.LightBlue,
				Size = 20
			};

			return sms;
		}

		private void OnElevationExaggerationChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			if (sender == null || MySceneView == null)
				return;

			MySceneView.ElevationExaggeration = (sender as Slider).Value;
		}
	}
}
