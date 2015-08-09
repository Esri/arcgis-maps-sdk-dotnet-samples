using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates adding a KML to a Scene in XAML.
	/// </summary>
	/// <title>3D KML Layer</title>
	/// <category>Scene</category>
	/// <subcategory>Layers</subcategory>
	public partial class KMLLayerSample3d : UserControl
	{
		public KMLLayerSample3d()
		{
			InitializeComponent();
			Initialize();
		}

		public async void Initialize()
		{
			await MySceneView.SetViewAsync(new Camera(new MapPoint(-99.343, 26.143, 5881928.401), 2.377, 10.982));
		}
	}
}
