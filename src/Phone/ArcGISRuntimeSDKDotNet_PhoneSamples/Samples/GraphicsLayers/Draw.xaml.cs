using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System.Windows.Controls.Primitives;
using Esri.ArcGISRuntime.Layers;
using System.Text;
using System.ComponentModel;
using System.Linq;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Graphics Layers</category>
	public partial class Draw : PhoneApplicationPage
    {
        public Draw()
        {
            InitializeComponent();
        }
		private async void AddButton_Click(object sender, EventArgs e)
		{
			try
			{
				Esri.ArcGISRuntime.Geometry.Geometry geom = await mapView1.Editor.RequestShapeAsync(DrawShape.Freehand);

				mapView1.Map.Layers.OfType<GraphicsLayer>().First().Graphics.Add(new Graphic(geom));
			}
			catch (OperationCanceledException) { }
		}
        private void ClearButton_Click(object sender, EventArgs e)
        {
			mapView1.Map.Layers.OfType<GraphicsLayer>().First().Graphics.Clear();
        }
    }
}