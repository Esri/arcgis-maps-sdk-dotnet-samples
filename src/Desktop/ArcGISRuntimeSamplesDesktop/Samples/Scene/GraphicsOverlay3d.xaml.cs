using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates adding Point, Polyline, and Polygon graphics to a GraphicsOverlay
	/// </summary>
	/// <title>3D Graphics Overlay</title>
	/// <category>Scene</category>
	/// <subcategory>Layers</subcategory>
	public partial class GraphicsOverlay3d : UserControl
	{
		GraphicsOverlay _graphicsOverlay;
		private MapPoint _ptStart;
		private MapPoint _ptEnd;

		public GraphicsOverlay3d()
		{
			InitializeComponent();

			MySceneView.SetViewAsync(new Viewpoint(new Envelope(-150, -40, 60, 64, SpatialReferences.Wgs84)));

			_graphicsOverlay = MySceneView.GraphicsOverlays["MyGraphicsOverlay"];

			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		void MySceneView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			AddPointGraphics();
			AddPolyLineGraphics();
			AddPolygonGraphics();
		}

		private void AddPointGraphics()
		{
			// Create point graphics for each SimpleMarkerStyle option and add to the GraphicsOverlay
			_graphicsOverlay.Graphics.Add(new Graphic(
				new MapPoint(-50, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Circle }));

			_graphicsOverlay.Graphics.Add(new Graphic(
				new MapPoint(-47, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Cross }));

			_graphicsOverlay.Graphics.Add(new Graphic(
				new MapPoint(-44, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Diamond }));

			_graphicsOverlay.Graphics.Add(new Graphic
				(new MapPoint(-41, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Square }));

			_graphicsOverlay.Graphics.Add(new Graphic(
				new MapPoint(-38, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Triangle }));

			_graphicsOverlay.Graphics.Add(new Graphic(
				new MapPoint(-35, 20),
				new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.X }));
		}

		private void AddPolyLineGraphics()
		{
			// Use the Point graphics as start and end points
			_ptStart = (MapPoint)_graphicsOverlay.Graphics[0].Geometry;
			_ptEnd = (MapPoint)_graphicsOverlay.Graphics[5].Geometry;

			var blueLineGeometry = new Polyline(
				new MapPoint[]
				{
					new MapPoint(_ptStart.X, _ptStart.Y + 3),
					new MapPoint(_ptEnd.X, _ptEnd.Y + 3)
				},
				_ptStart.SpatialReference);

			// Solid Blue line above point graphics
			Graphic blueLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Solid, Width = 4 },
				Geometry = blueLineGeometry
			};

			var greenLineGeometry = new Polyline(
				new MapPoint[]
				{
					new MapPoint(_ptStart.X, _ptStart.Y - 3),
					new MapPoint(_ptEnd.X, _ptEnd.Y - 3)
				},
				_ptStart.SpatialReference);

			// Dashed Green line below point graphics
			Graphic greenLine = new Graphic()
			{
				Symbol = new SimpleLineSymbol() { Color = Colors.Green, Style = SimpleLineStyle.Dash, Width = 4 },
				Geometry = greenLineGeometry
			};

			// Add polyline graphics to GraphicsOverlay
			_graphicsOverlay.Graphics.Add(blueLine);
			_graphicsOverlay.Graphics.Add(greenLine);
		}

		private void AddPolygonGraphics()
		{
			// Use the Point graphics as start and end points
			_ptStart = (MapPoint)_graphicsOverlay.Graphics[0].Geometry;
			_ptEnd = (MapPoint)_graphicsOverlay.Graphics[5].Geometry;

			var purpleRectGeometry = new Polygon(
				new MapPoint[]
                    {
                        new MapPoint(_ptStart.X, _ptStart.Y - 6),
                        new MapPoint(_ptStart.X, _ptStart.Y - 9),
                        new MapPoint(_ptEnd.X, _ptEnd.Y - 9),
                        new MapPoint(_ptEnd.X, _ptEnd.Y - 6)

                    }, _ptStart.SpatialReference);

			// Purple rectangle below polyline graphic
			Graphic purpleRect = new Graphic()
			{
				Symbol = new SimpleFillSymbol() { Color = Colors.Purple, Style = SimpleFillStyle.Solid },
				Geometry = purpleRectGeometry
			};

			// Add polygon graphic to GraphicsOverlay
			_graphicsOverlay.Graphics.Add(purpleRect);
		}
	}
}
