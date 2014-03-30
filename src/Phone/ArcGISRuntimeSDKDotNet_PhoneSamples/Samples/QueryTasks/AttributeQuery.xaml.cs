using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public partial class AttributeQuery : PhoneApplicationPage
    {
        public AttributeQuery()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Page_Loaded;

            // Wait for layers to initialize, then zoom to desired initial extent.  Avoid using InitialExtent
            // to work around issue with calling ZoomTo after InitialExtent has been set.
            await Task.WhenAll(mapView1.Map.Layers.Select(l => l.InitializeAsync()));
            await mapView1.SetViewAsync(new Envelope(-15000000, 2000000, -7000000, 8000000, SpatialReferences.WebMercator), 
                TimeSpan.FromTicks(0));
        }

        // Get list of states when list picker loads
        private async void QueryListPicker_Loaded(object sender, RoutedEventArgs e)
        {
            await initializeComboBox();
        }

        // When a new state is selected, display the attributes for it
        private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await displaySelectedState();
        }

        // Show attribute display UI when the show attributes button is toggled on
        private void ShowAttributesButton_Checked(object sender, RoutedEventArgs e)
        {
            AttributeDisplay.Visibility = Visibility.Visible;
            ShowAttributesButton.Content = "Hide Attributes";
        }

        // Hide attribute display UI when the show attributes button is toggled off
        private void ShowAttributesButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AttributeDisplay.Visibility = Visibility.Collapsed;
            ShowAttributesButton.Content = "Show Attributes";
        }

        private async Task initializeComboBox()
        {
            if (QueryListPicker.ItemsSource != null) // already initialized
                return;

            // Construct the query to return all features (where clause of "1=1") and only the state name
            Query query = new Query("1=1") { ReturnGeometry = false };
            query.OutFields.Add("STATE_NAME");

            QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));
            try
            {
                // Do the query
                var result = await queryTask.ExecuteAsync(query);

                // Make sure results were received
                if (result != null && result.FeatureSet != null && result.FeatureSet.Features != null)
                {
                    // Add the states to the list picker, ordered by state name
                    QueryListPicker.ItemsSource = result.FeatureSet.Features.OrderBy(
                        x => x.Attributes["STATE_NAME"]);

                    // Enable the list picker and hide the busy indicator
                    QueryListPicker.IsEnabled = true;
                    progress.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task displaySelectedState()
        {
            if (QueryListPicker.SelectedItem == null)
                return;

            // Get the selected state name
            var graphic = ((Graphic)QueryListPicker.SelectedItem);
            var stateName = (string)graphic.Attributes["STATE_NAME"];

            // Construct the query
            Query query = new Query(stateName)
            {
                OutFields = OutFields.All,
                ReturnGeometry = true,
                OutSpatialReference = mapView1.SpatialReference
            };
            QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

            try
            {
                // show busy indicator before issuing query request
                progress.Visibility = Visibility.Visible;

                // Do the query
                var result = await queryTask.ExecuteAsync(query);

                // Clear the previous results
                GraphicsLayer graphicsLayer = (GraphicsLayer)mapView1.Map.Layers["MyGraphicsLayer"];
                graphicsLayer.Graphics.Clear();

                // Get the results
                var results = result.FeatureSet;
                if (results != null && results.Features.Count > 0)
                {
                    // Add the first result to the map
                    graphic = results.Features[0];
                    graphicsLayer.Graphics.Add(graphic);

                    // Zoom to the result
                    var selectedFeatureExtent = graphic.Geometry.Extent;
                    Envelope displayExtent = selectedFeatureExtent.Expand(1.3);
                    mapView1.SetView(displayExtent);

                    // Update the attribute display with the result
                    FieldsDisplay.ItemsSource = ValuesDisplay.ItemsSource = graphic.Attributes;

                    // Enable the button to show/hide attributes
                    ShowAttributesButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}