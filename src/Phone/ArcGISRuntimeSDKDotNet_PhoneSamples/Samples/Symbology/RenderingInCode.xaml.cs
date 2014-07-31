using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Layers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Symbology</category>
	public sealed partial class RenderingInCode : Page
    {       
        public RenderingInCode()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000));
            SetRenderers();
            
        }

        private void SetRenderers()
        {
            SimpleRenderer simpleRenderer = new SimpleRenderer()
            {
                Description = "Rivers",
                Label = "Rivers",
                Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Dash, Width = 2 }
            };
            (mapView1.Map.Layers["MyFeatureLayerSimple"] as FeatureLayer).Renderer = simpleRenderer;

            UniqueValueRenderer uvr = new UniqueValueRenderer();
            uvr.Fields = new ObservableCollection<string>(new string [] { "STATE_NAME" });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] {"New Mexico" }), Symbol = new SimpleFillSymbol() { Color = Colors.Yellow } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "Texas" }), Symbol = new SimpleFillSymbol() { Color = Colors.PaleGreen } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "Arizona" }), Symbol = new SimpleFillSymbol() { Color = Colors.YellowGreen } });

            (mapView1.Map.Layers["MyFeatureLayerUnique"] as FeatureLayer).Renderer = uvr;


            ClassBreaksRenderer CBR = new ClassBreaksRenderer()
            {
                DefaultLabel = "All Other Values",
                DefaultSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, Style = SimpleMarkerStyle.Cross, Size = 10 },
                Field = "POP1990",
                Minimum = 0
            };

            CBR.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 30000,
                Label = "0-30000",
                Description = "Pop between 0 and 30000",
                Symbol = new SimpleMarkerSymbol() { Color = Colors.Yellow, Size = 8, Style = SimpleMarkerStyle.Circle }
            });
            CBR.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 300000,
                Label = "30000-300000",
                Description = "Pop between 30000 and 300000",
                Symbol = new SimpleMarkerSymbol() { Color = Colors.Red, Size = 10, Style = SimpleMarkerStyle.Circle }
            });

            CBR.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 5000000,
                Label = "300000-5000000",
                Description = "Pop between 300000 and 5000000",
                Symbol = new SimpleMarkerSymbol() { Color = Colors.Orange, Size = 12, Style = SimpleMarkerStyle.Circle }
            });
            (mapView1.Map.Layers["MyFeatureLayerClassBreak"] as FeatureLayer).Renderer = CBR;
        }

    }
}
