using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public partial class Identify : PhoneApplicationPage
    {
        MapView m_mapView;
        public Identify()
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
            await mapView1.SetViewAsync(new Envelope(-12800000, 2000000, -7800000, 8000000, SpatialReferences.WebMercator));
        }

        // Perform identify when the map is tapped
        private async void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
            if (m_mapView == null)
                m_mapView = (MapView)sender;
            // Clear any previously displayed results
            clearResults();

            // Get the point that was tapped and show it on the map
            
            GraphicsLayer identifyPointLayer = m_mapView.Map.Layers["IdentifyPointLayer"] as GraphicsLayer;
            identifyPointLayer.Graphics.Add(new Graphic() { Geometry = e.Location });

            // Show activity
            progress.Visibility = Visibility.Visible;

            // Perform the identify operation
            List<DataItem> results = await doIdentifyAsync(e.Location);

            // Hide the activity indicator
            progress.Visibility = Visibility.Collapsed;

            // Show the results
            ResultsListPicker.ItemsSource = results;
            if (results.Count > 0)
            {
                ResultsListPicker.Visibility = Visibility.Visible;
                ShowAttributesButton.Visibility = Visibility.Visible;
            }
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

        // When a different feature is selected in the list picker, display it on the map and show its attributes
        private void ResultsListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected feature
            DataItem selectedItem = ResultsListPicker.SelectedItem as DataItem;
            if (selectedItem != null)
            {
                // Update the data display with the new feature
                FieldsDisplay.ItemsSource = ValuesDisplay.ItemsSource = selectedItem.Attributes;

                // Get the layer for displaying the selected feature and clear it
                GraphicsLayer polygonLayer = m_mapView.Map.Layers["PolygonLayer"] as GraphicsLayer;
                polygonLayer.Graphics.Clear();

                // Add the newly selected feature to the map and zoom to it
                polygonLayer.Graphics.Add(new Graphic() { Geometry = selectedItem.Geometry });
                m_mapView.SetView(selectedItem.Geometry.Extent.Expand(2.5));
            }
        }

        // Performs the identify operation and returns the results as DataItems
        private async Task<List<DataItem>> doIdentifyAsync(MapPoint point)
        {
            // Initialize paraemters for the identify operation
            IdentifyParameter identifyParams = new IdentifyParameter(point, m_mapView.Extent, 2, 
                (int)m_mapView.ActualHeight, (int)m_mapView.ActualWidth)
            {
                SpatialReference = m_mapView.SpatialReference,
                ReturnGeometry = true
            };

            // Initialize the identify task with the service to identify features from
            IdentifyTask identifyTask = new IdentifyTask(new Uri(
                "http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer"));
            var dataItems = new List<DataItem>();

            try
            {
                // Do the identify operation
                var result = await identifyTask.ExecuteAsync(identifyParams);

                // Create DataItems that encapsulate each result
                if (result != null && result.Results != null && result.Results.Count > 0)
                {
                    foreach (var r in result.Results)
                    {
                        dataItems.Add(new DataItem()
                        {
                            Title = string.Format("{0} ({1})", r.Value, r.LayerName),
                            Attributes = r.Feature.Attributes,
                            Geometry = r.Feature.Geometry
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // Return the results as DataItems
            return dataItems;
        }

        // Clears the previous results from the map and attribute display area
        private void clearResults()
        {
            // Hide the list picker for selecting the feature to view and the button to toggle attributes
            // on and off
            ResultsListPicker.Visibility = Visibility.Collapsed;
            ShowAttributesButton.Visibility = Visibility.Collapsed;
            ShowAttributesButton.IsChecked = false;

            // Clear the results from the attribute display area
            ResultsListPicker.ItemsSource = null;
            FieldsDisplay.ItemsSource = null;
            ValuesDisplay.ItemsSource = null;

            // Clear the input point from the map
            GraphicsLayer identifyPointLayer = m_mapView.Map.Layers["IdentifyPointLayer"] as GraphicsLayer;
            identifyPointLayer.Graphics.Clear();

            // Clear the currently selected feature from the map
            GraphicsLayer polygonLayer = m_mapView.Map.Layers["PolygonLayer"] as GraphicsLayer;
            polygonLayer.Graphics.Clear();
        }
    }

    /// <summary>
    /// Encapsulates a identify result to allow easy binding
    /// </summary>
    public class DataItem : INotifyPropertyChanged
    {
        private string m_title;
        /// <summary>
        /// Gets or sets the title of the item
        /// </summary>
        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title != value)
                {
                    m_title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private IDictionary<string, object> m_attributes;
        /// <summary>
        /// Gets or sets the attributes of the item
        /// </summary>
        public IDictionary<string, object> Attributes
        {
            get { return m_attributes; }
            set
            {
                if (m_attributes != value)
                {
                    m_attributes = value;
                    OnPropertyChanged("Attributes");
                }
            }
        }

        private Geometry m_geometry;
        /// <summary>
        /// Gets or sets the geometry of the item
        /// </summary>
        public Geometry Geometry
        {
            get { return m_geometry; }
            set
            {
                if (m_geometry != value)
                {
                    m_geometry = value;
                    OnPropertyChanged("Geometry");
                }
            }
        }

        /// <summary>
        /// Raised when one of the DataItem's properties is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // Fires the PropertyChanged event
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}