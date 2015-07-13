using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop.DynamicLayers
{
    /// <summary>
    /// This sample demonstrates dynamic layer labeling.  Labels are configured in the LayerDrawingOptions of the dyanmic layer.  In this sample, labels are shown for major and minor US cities with different fonts and scale ranges.  Major city label info is configured in the XAML while minor city label info is added via the code-behind.
    /// </summary>
    /// <title>Dynamic Layer Labeling</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class DynamicLayerLabeling : UserControl
    {
        public DynamicLayerLabeling()
        {
            InitializeComponent();

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;

            // Minor city label info
            DynamicLabelingInfo minorCityLabelInfo = new DynamicLabelingInfo();
            minorCityLabelInfo.LabelExpression = "[areaname]";
            minorCityLabelInfo.Symbol = new Esri.ArcGISRuntime.Symbology.TextSymbol()
            {
                Color = Colors.Black,
                Font = new SymbolFont("Arial", 10, SymbolFontStyle.Normal, SymbolTextDecoration.None, SymbolFontWeight.Normal)
            };
            minorCityLabelInfo.Where = "pop2000 <= 500000";
            minorCityLabelInfo.MaxScale = 0;
            minorCityLabelInfo.MinScale = 5000000;

            // Add minor city label info
            var labelInfos = dynamicLayer.LayerDrawingOptions.First(ldo => ldo.LayerID == 0).LabelingInfos;
            labelInfos.Add(minorCityLabelInfo);
        }
    }
}
