using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates performing identify operations.
    /// </summary>
    /// <title>Identify</title>
    /// <category>Query Tasks</category>
	public sealed partial class Identify : Page
    {
        public Identify()
        {
            this.InitializeComponent();

            mapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000));
        }

        private async void mapView_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                resultsGrid.DataContext = null;

                GraphicsLayer graphicsLayer = mapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
                graphicsLayer.Graphics.Clear();
                graphicsLayer.Graphics.Add(new Graphic(e.Location));

                IdentifyParameters identifyParams = new IdentifyParameters(e.Location, mapView.Extent, 2, (int)mapView.ActualHeight, (int)mapView.ActualWidth)
                {
                    LayerOption = LayerOption.Visible,
                    SpatialReference = mapView.SpatialReference,
                };

                IdentifyTask identifyTask = new IdentifyTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer"));

                var result = await identifyTask.ExecuteAsync(identifyParams);

                resultsGrid.DataContext = result.Results;
                if (result != null && result.Results != null && result.Results.Count > 0)
                    titleComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private void TitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (titleComboBox.SelectedIndex >= 0)
            {
                var items = titleComboBox.ItemsSource as IReadOnlyList<IdentifyItem>;
                var item = items[titleComboBox.SelectedIndex];
                attributesList.ItemsSource = item.Feature.Attributes
                    .Select(attr => new Tuple<string, string>(attr.Key, attr.Value.ToString()));
            }
        }
    }
}