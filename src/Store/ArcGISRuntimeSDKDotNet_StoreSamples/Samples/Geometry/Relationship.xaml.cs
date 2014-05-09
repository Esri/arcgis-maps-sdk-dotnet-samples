using Windows.UI.Xaml;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Sample shows how to to test the spatial relationship of two geometries.
    /// </summary>
    /// <title>Relationship</title>
	/// <category>Geometry</category>
	public partial class Relationship : Windows.UI.Xaml.Controls.Page
    {
        private List<Symbol> _symbols;
        private GraphicsLayer _graphicsLayer;

        /// <summary>Construct Relationship sample control</summary>
        public Relationship()
        {
            InitializeComponent();

            _symbols = new List<Symbol>();
            _symbols.Add(LayoutRoot.Resources["PointSymbol"] as Symbol);
            _symbols.Add(LayoutRoot.Resources["LineSymbol"] as Symbol);
            _symbols.Add(LayoutRoot.Resources["FillSymbol"] as Symbol);

            _graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        // Start map interaction
        private void mapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                mapView.ExtentChanged -= mapView_ExtentChanged;
                btnDraw.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Accepts two user shapes and adds them to the graphics layer
        private async Task AcceptShapeAsync()
        {
            // Shape One
            Geometry shapeOne = await mapView.Editor.RequestShapeAsync(
                (DrawShape)comboShapeOne.SelectedValue, _symbols[comboShapeOne.SelectedIndex]);

            _graphicsLayer.Graphics.Add(new Graphic(shapeOne, _symbols[comboShapeOne.SelectedIndex]));

            // Shape Two
            Geometry shapeTwo = await mapView.Editor.RequestShapeAsync(
                (DrawShape)comboShapeTwo.SelectedValue, _symbols[comboShapeTwo.SelectedIndex]);

            _graphicsLayer.Graphics.Add(new Graphic(shapeTwo, _symbols[comboShapeTwo.SelectedIndex]));

            var relations = new List<Tuple<string, bool>>();
            relations.Add(new Tuple<string,bool>("Contains", GeometryEngine.Contains(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Crosses", GeometryEngine.Crosses(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Disjoint", GeometryEngine.Disjoint(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Equals", GeometryEngine.Equals(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Intersects", GeometryEngine.Intersects(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Overlaps", GeometryEngine.Overlaps(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Touches", GeometryEngine.Touches(shapeOne, shapeTwo)));
            relations.Add(new Tuple<string,bool>("Within", GeometryEngine.Within(shapeOne, shapeTwo)));
            resultsListView.ItemsSource = relations;

            resultsPanel.Visibility = Visibility.Visible;
        }

        private async void StartDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDraw.IsEnabled = false;
                resultsPanel.Visibility = Visibility.Collapsed;

                _graphicsLayer.Graphics.Clear();
                await AcceptShapeAsync();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                btnDraw.IsEnabled = true;
            }
        }
    }
}
