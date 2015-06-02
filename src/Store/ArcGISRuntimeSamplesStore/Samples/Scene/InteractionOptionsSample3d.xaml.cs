using Esri.ArcGISRuntime.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// This sample shows how to change interaction settings that controls the SceneView.
	/// </summary>
	/// <title>Interaction Options 3d</title>
	/// <category>Scene</category>
	public sealed partial class InteractionOptionsSample3d : Page
	{
		public InteractionOptionsSample3d()
		{
			this.InitializeComponent();
			MouseWheelInteraction.ItemsSource = Enum.GetValues(typeof(MouseWheelDirection)).Cast<MouseWheelDirection>();
			PanOptions.DataContext = MySceneView.InteractionOptions.PanOptions;
			ZoomOptions.DataContext = MySceneView.InteractionOptions.ZoomOptions;
			RotateToggle.DataContext = MySceneView.InteractionOptions.RotationOptions;
			OptionsToggle.DataContext = MySceneView.InteractionOptions;
		}

		private void MySceneView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			var _x = new MessageDialog(
				string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Interaction Options Sample").ShowAsync();
		}

		private void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("SceneView Tapped");
		}

		private void MySceneView_SceneViewDoubleTapped(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("SceneView DoubleTapped");
		}

		private void MySceneView_SceneViewHolding(object sender, MapViewInputEventArgs e)
		{
			Debug.WriteLine("SceneView Holding");
		}
	}
}
