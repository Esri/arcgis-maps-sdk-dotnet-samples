using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// This sample demonstrates adding a KML to a Scene in XAML.
	/// </summary>
	/// <title>3D KML Layer</title>
	/// <category>Scene</category>
	public sealed partial class KMLLayerSample3d : Page
	{
		public KMLLayerSample3d()
		{
			this.InitializeComponent();
			Initialize();
		}

		public async void Initialize()
		{
			await MySceneView.SetViewAsync(new Camera(new MapPoint(-99.343, 26.143, 5881928.401), 2.377, 10.982));
		}
	}
}
