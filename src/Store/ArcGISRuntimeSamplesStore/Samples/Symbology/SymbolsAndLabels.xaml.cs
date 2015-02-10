using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Linq;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows various types of the more advanced symbol types: Composite and CIM symbology.
	/// </summary>
	/// <title>Symbols and Labels</title>
    /// <category>Symbology</category>
	public sealed partial class SymbolsAndLabels : Windows.UI.Xaml.Controls.Page
    {
		public SymbolsAndLabels()
        {
            this.InitializeComponent();
			CreateGeometries();
        }

		/// <summary>
		/// XAML creation of polygon and polyline geometries are currently not supported, so
		/// here they are created in code. Points are generated in the XAML for this sample.
		/// </summary>
		private void CreateGeometries()
		{
			var layer = MyMapView.Map.Layers.OfType<GraphicsLayer>().First();

			layer.Graphics.Add(new Graphic(new MapPoint(-6000000, 4800000), (Symbol)Resources["NumberedMarkerSymbol1"]));
			layer.Graphics.Add(new Graphic(new MapPoint(-5000000, 3900000), (Symbol)Resources["NumberedMarkerSymbolA"]));
			layer.Graphics.Add(new Graphic(new MapPoint(-4000000, 4800000), (Symbol)Resources["NumberedMarkerSymbol1"]));
			layer.Graphics.Add(new Graphic(new MapPoint(-3000000, 3900000), (Symbol)Resources["NumberedMarkerSymbolA"]));
			layer.Graphics.Add(new Graphic(new MapPoint(-2000000, 4800000), (Symbol)Resources["NumberedMarkerSymbol1"]));
			layer.Graphics.Add(new Graphic(new MapPoint(-1000000, 3900000), (Symbol)Resources["NumberedMarkerSymbolA"]));

			int i = 0;
			foreach (var g in layer.Graphics)
				g.Attributes["Label"] = "Label #" + (++i).ToString();

			Polyline line = new Polyline(FromArray(-100,-30, -80,0, -60,-30, -40,0), SpatialReferences.Wgs84);
			var graphic = new Graphic(line, (Esri.ArcGISRuntime.Symbology.Symbol)Resources["OutlinedAndDashedSymbol"]);
			graphic.Attributes["Label"] = "OutlinedAndDashedSymbol";
			layer.Graphics.Add(graphic);

			Polygon polygon = new Polygon(FromArray(-30,-30, 0,-30, 0,0, -15,-10, -30,0, -30,-30), SpatialReferences.Wgs84);
			graphic = new Graphic(polygon, (Esri.ArcGISRuntime.Symbology.Symbol)Resources["VertexFillSymbol"]);
			graphic.Attributes["Label"] = "VertexFillSymbol";
			layer.Graphics.Add(graphic);

			//CIM symbols can only be created from JSON. The JSON is currently only constructed by publishing services to ArcGIS Server with advanced symbology
			string CIMSymbolJson = "{\"type\":\"CIMSymbolReference\",\"symbol\":{\"type\":\"CIMLineSymbol\",\"symbolLayers\":[{\"type\":\"CIMFilledStroke\",\"enable\":true,\"effects\":[{\"type\":\"CIMGeometricEffectArrow\",\"geometricEffectArrowType\":\"Block\",\"primitiveName\":null,\"width\":35}],\"capStyle\":\"Round\",\"pattern\":{\"type\":\"CIMSolidPattern\",\"color\":[0,0,0,255]},\"width\":2,\"lineStyle3D\":\"Strip\",\"alignment\":\"Center\",\"joinStyle\":\"Miter\",\"miterLimit\":10,\"patternFollowsStroke\":true}]},\"symbolName\":null}";
			var cimsymbol = Esri.ArcGISRuntime.Symbology.Symbol.FromJson(CIMSymbolJson);
			Polyline line2 = new Polyline(FromArray(20, -30, 30, 0, 50, -30, 70, 0), SpatialReferences.Wgs84);
			graphic = new Graphic(line2, cimsymbol);
			graphic.Attributes["Label"] = "CIM Symbol";
			layer.Graphics.Add(graphic);

			i = 0;
			foreach (var g in layer.Graphics)
			{
				g.Attributes["SymbolType"] = g.Symbol.GetType().Name;
				g.Attributes["ID"] = ++i;
			}
			var symbols = this.Resources.OfType<MarkerSymbol>();
			double x = -7000000;
			foreach (var symbol in symbols)
			{
				Graphic g = new Graphic(new MapPoint(x, 3900000), symbol);
				layer.Graphics.Add(g);
				x += 1000000;
			}
		}

		// Helper method
		private static PointCollection FromArray(params double[] parameters)
		{
			PointCollection coll = new PointCollection();
			for (int i = 0; i < parameters.Length - 1; i+=2)
			{
				coll.Add(new MapPoint(parameters[i], parameters[i + 1]));
			}
			return coll;
		}

		private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
		{
			MyMapView.SetRotationAsync(e.NewValue);
		}
    }
}
