using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// Shows various types of the more advanced symbol types: Composite and CIM symbology.
	/// </summary>
    /// <category>Symbology</category>
	public sealed partial class SymbolsAndLabels : Page
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
			var layer = mapView1.Map.Layers.OfType<GraphicsLayer>().First();
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
		}

		// Helper method
		private static CoordinateCollection FromArray(params double[] parameters)
		{
			CoordinateCollection coll = new CoordinateCollection();
			for (int i = 0; i < parameters.Length - 1; i+=2)
			{
				coll.Add(new Coordinate(parameters[i], parameters[i + 1]));
			}
			return coll;
		}
    }
}
