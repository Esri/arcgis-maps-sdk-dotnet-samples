using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Sample shows how to to test the spatial relationship of two geometries.
    /// </summary>
    /// <title>Relationship</title>
	/// <category>Geometry</category>
	public partial class Relationship : UserControl
    {
        private List<Symbol> _symbols;

        /// <summary>Construct Relationship sample control</summary>
        public Relationship()
        {
            InitializeComponent();

            _symbols = new List<Symbol>();
            _symbols.Add(layoutGrid.Resources["PointSymbol"] as Symbol);
            _symbols.Add(layoutGrid.Resources["LineSymbol"] as Symbol);
            _symbols.Add(layoutGrid.Resources["FillSymbol"] as Symbol);

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
                MessageBox.Show("Error: " + ex.Message, "Relationship Sample");
            }
        }

        // Accepts two user shapes and adds them to the graphics layer
        private async Task AcceptShapeAsync()
        {
            // Shape One
            DrawShape drawShape1 = (DrawShape)comboShapeOne.SelectedItem;
            Geometry shapeOne = null;
            if (drawShape1 == DrawShape.Point)
                shapeOne = await mapView.Editor.RequestPointAsync();
            else
                shapeOne = await mapView.Editor.RequestShapeAsync(drawShape1, _symbols[comboShapeOne.SelectedIndex]);

            graphicsLayer.Graphics.Add(new Graphic(shapeOne, _symbols[comboShapeOne.SelectedIndex]));

            // Shape Two
            Geometry shapeTwo = await mapView.Editor.RequestShapeAsync(
                (DrawShape)comboShapeTwo.SelectedItem, _symbols[comboShapeTwo.SelectedIndex]);

            graphicsLayer.Graphics.Add(new Graphic(shapeTwo, _symbols[comboShapeTwo.SelectedIndex]));

            Dictionary<string, bool> relations = new Dictionary<string, bool>();
            relations["Contains"] = GeometryEngine.Contains(shapeOne, shapeTwo);
            relations["Crosses"] = GeometryEngine.Crosses(shapeOne, shapeTwo);
            relations["Disjoint"] = GeometryEngine.Disjoint(shapeOne, shapeTwo);
            relations["Equals"] = GeometryEngine.Equals(shapeOne, shapeTwo);
            relations["Intersects"] = GeometryEngine.Intersects(shapeOne, shapeTwo);
            relations["Overlaps"] = GeometryEngine.Overlaps(shapeOne, shapeTwo);
            relations["Touches"] = GeometryEngine.Touches(shapeOne, shapeTwo);
            relations["Within"] = GeometryEngine.Within(shapeOne, shapeTwo);

            resultsPanel.Visibility = Visibility.Visible;
            resultsListView.ItemsSource = relations;
        }

        private async void StartDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDraw.IsEnabled = false;
                resultsPanel.Visibility = Visibility.Collapsed;

                graphicsLayer.Graphics.Clear();
                await AcceptShapeAsync();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Relationship Sample");
            }
            finally
            {
                btnDraw.IsEnabled = true;
            }
        }
    }
}
