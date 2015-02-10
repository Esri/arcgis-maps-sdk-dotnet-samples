using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Sample shows how to test the spatial relationship of two geometries.
    /// </summary>
    /// <title>Relationship</title>
	/// <category>Geometry</category>
	public partial class Relationship : Windows.UI.Xaml.Controls.Page
    {
        private List<Symbol> _symbols;
        private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Relationship sample control</summary>
        public Relationship()
        {
            InitializeComponent();

            _symbols = new List<Symbol>();
            _symbols.Add(LayoutRoot.Resources["PointSymbol"] as Symbol);
            _symbols.Add(LayoutRoot.Resources["LineSymbol"] as Symbol);
            _symbols.Add(LayoutRoot.Resources["FillSymbol"] as Symbol);

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Start map interaction
        private void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                MyMapView.ExtentChanged -= MyMapView_ExtentChanged;
                btnDraw.IsEnabled = true;
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Accepts two user shapes and adds them to the graphics layer
        private async Task AcceptShapeAsync()
        {
            // Shape One
            Geometry shapeOne = await MyMapView.Editor.RequestShapeAsync(
                (DrawShape)comboShapeOne.SelectedValue, _symbols[comboShapeOne.SelectedIndex]);

			_graphicsOverlay.Graphics.Add(new Graphic(shapeOne, _symbols[comboShapeOne.SelectedIndex]));

            // Shape Two
            Geometry shapeTwo = await MyMapView.Editor.RequestShapeAsync(
                (DrawShape)comboShapeTwo.SelectedValue, _symbols[comboShapeTwo.SelectedIndex]);

			_graphicsOverlay.Graphics.Add(new Graphic(shapeTwo, _symbols[comboShapeTwo.SelectedIndex]));

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

				_graphicsOverlay.Graphics.Clear();
                await AcceptShapeAsync();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                btnDraw.IsEnabled = true;
            }
        }
    }
}
