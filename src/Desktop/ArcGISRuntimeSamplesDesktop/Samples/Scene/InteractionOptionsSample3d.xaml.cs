using Esri.ArcGISRuntime.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to change interaction settings that controls the MapView.
    /// </summary>
    /// <title>Interaction Options</title>
    /// <category>Scene</category>
		/// <subcategory>Navigation</subcategory>
    public partial class InteractionOptionsSample3d : UserControl
    {
        public InteractionOptionsSample3d()
        {
            InitializeComponent();
        }

        private void MySceneView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError == null)
                return;

            MessageBox.Show(
                string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
                              "Error loading layer");
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

        private void rotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
					if (MySceneView.InteractionOptions.RotationOptions.IsEnabled)
            {
							MySceneView.SetView(new Camera(MySceneView.Camera.Location,e.NewValue,MySceneView.Camera.Pitch));
            }
        }
    }
}
