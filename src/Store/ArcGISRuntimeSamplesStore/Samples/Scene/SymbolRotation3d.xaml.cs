using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Symbology.SceneSymbology;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to rotate 3d symbols.
	/// </summary>
	/// <title>3D Symbol Rotation</title>
	/// <category>Scene</category>
	public sealed partial class SymbolRotation3d : Page
	{
		public SymbolRotation3d()
		{
			this.InitializeComponent();
		}
		private void MySceneView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
		{
			if (e.LoadError == null && e.Layer.ID == "AGOLayer")
			{
				MySceneView.SetViewAsync(new Camera(new MapPoint(-106.57, 39.01, 14614.24), 281.66, 74.47), new TimeSpan(0, 0, 3), true);

				AddGraphics();
			}
		}

		private void AddGraphics()
		{
			Graphic graphic = new Graphic(new MapPoint(-106.981, 39.028, 6000, SpatialReferences.Wgs84));

			// Add a graphic to each graphics layer. It will use the renderer specified in the XAML to render the graphic.
			foreach (GraphicsOverlay gOverlay in MySceneView.GraphicsOverlays)
			{
				gOverlay.Graphics.Add(graphic);
			}
		}

		/// <summary>
		/// Change the Heading of the graphics based on the slider values (0-360).
		/// </summary>
		private void OnHeadingSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			foreach (GraphicsOverlay gOverlay in MySceneView.GraphicsOverlays)
			{
				foreach (Graphic g in gOverlay.Graphics)
				{
					g.Attributes["Heading"] = (sender as Slider).Value;
				}
			}

			// Display the slider Heading value
			txtHeading.Text = String.Format("Heading: {0:0.00}", (sender as Slider).Value.ToString());
		}

		/// <summary>
		/// Change the Pitch of the graphics based on the slider values (0-360)
		/// </summary>
		private void OnPitchSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			foreach (GraphicsOverlay gOverlay in MySceneView.GraphicsOverlays)
			{
				foreach (Graphic g in gOverlay.Graphics)
				{
					g.Attributes["Pitch"] = (sender as Slider).Value;
				}
			}

			// Display the slider Pitch value
			txtPitch.Text = String.Format("Pitch: {0:0.00}", (sender as Slider).Value.ToString());
		}

		/// <summary>
		/// Change the Roll of the graphics based on the slider values (0-360)
		/// </summary>
		private void OnRollSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			foreach (GraphicsOverlay gOverlay in MySceneView.GraphicsOverlays)
			{
				foreach (Graphic g in gOverlay.Graphics)
				{
					g.Attributes["Roll"] = (sender as Slider).Value;
				}
			}

			// Display the slider Roll value
			txtRoll.Text = String.Format("Roll: {0:0.00}", (sender as Slider).Value.ToString());
		}
	}
}
