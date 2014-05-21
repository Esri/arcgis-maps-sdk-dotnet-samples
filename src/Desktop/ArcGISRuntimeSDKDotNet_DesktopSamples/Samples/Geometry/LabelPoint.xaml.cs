using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates use of the GeometryEngine.LabelPoint method to calculate the location of label points.
    /// </summary>
    /// <title>Label Point</title>
	/// <category>Geometry</category>
	public partial class LabelPoint : UserControl
    {
        private PictureMarkerSymbol _pictureMarkerSymbol;

        /// <summary>Construct Label Point sample control</summary>
        public LabelPoint()
        {
            InitializeComponent();

            mapView.ExtentChanged += mapView_ExtentChanged;
            var task = SetupSymbolsAsync();
        }

        // Load the picture symbol image
        private async Task SetupSymbolsAsync()
        {
            try
            {
                _pictureMarkerSymbol = layoutGrid.Resources["PictureMarkerSymbol"] as PictureMarkerSymbol;
				await _pictureMarkerSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/x-24x24.png"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // Start accepting user polygons and calculating label points
        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            mapView.ExtentChanged -= mapView_ExtentChanged;
            await CalculateLabelPointsAsync();
        }

        // Continuosly accept polygons from the user and calculate label points
        private async Task CalculateLabelPointsAsync()
        {
            try
            {
                await mapView.LayersLoadedAsync();

                while (mapView.Extent != null)
                {
                    if (mapView.Editor.IsActive)
                        mapView.Editor.Cancel.Execute(null);

                    //Get the input polygon geometry from the user
                    var poly = await mapView.Editor.RequestShapeAsync(DrawShape.Polygon, ((SimpleRenderer)labelGraphics.Renderer).Symbol);
                    if (poly != null)
                    {
                        //Add the polygon drawn by the user
                        labelGraphics.Graphics.Add(new Graphic(poly));

                        //Get the label point for the input geometry
                        var labelPoint = GeometryEngine.LabelPoint(poly);
                        if (labelPoint != null)
                        {
                            labelGraphics.Graphics.Add(new Graphic(labelPoint, _pictureMarkerSymbol));
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Label Point Error: " + ex.Message, "Label Point Sample");
            }
        }

        // Clear label graphics and restart calculating label points
        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            labelGraphics.Graphics.Clear();
            await CalculateLabelPointsAsync();
        }
    }
}
