using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public sealed partial class BufferAndQuery : PhoneApplicationPage
    {
        MapView m_mapView;
        public BufferAndQuery()
        {
            InitializeComponent();

            // Initialize the graphics collections
            TapPoints = new ObservableCollection<Graphic>();
            Buffers = new ObservableCollection<Graphic>();
            Parcels = new ObservableCollection<Graphic>();

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

        // On tap, either get related records for the tapped well or find nearby wells if no well was tapped
        private async void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
            // Show busy UI
            BusyVisibility = Visibility.Visible;

            // Get the map
            if (m_mapView == null)
                m_mapView = (MapView)sender;
                      

            // Create graphic and add to tap points
            var g = new Graphic() { Geometry = e.Location };
            TapPoints.Add(g);

            // Buffer graphic by 100 meters, create graphic with buffer, add to buffers
            var buffer = GeometryEngine.Buffer(g.Geometry, 100);
            Buffers.Add(new Graphic() { Geometry = buffer });

            // Find intersecting parcels and show them on the map
            var result = await doQuery(buffer);
            if (result != null && result.FeatureSet != null && result.FeatureSet.Features.Count > 0)
            {
                // Instead of adding parcels one-by-one, update the Parcels collection all at once to
                // allow the map to render the new features in one rendering pass.
                Parcels = new ObservableCollection<Graphic>(Parcels.Union(result.FeatureSet.Features));
            }

            // Hide busy UI
            BusyVisibility = Visibility.Collapsed;
        }

        // Searches for parcels that intersect the passed-in geometry
        private async Task<QueryResult> doQuery(Geometry geometry)
        {
            QueryTask queryTask =
                new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2"));

            // Construct query parameters using the passed-in geometry
            Query query = new Query(geometry)
            {
                ReturnGeometry = true,
                OutSpatialReference = m_mapView.SpatialReference
            };
            try
            {
                // Execute the query and return the result
                return await queryTask.ExecuteAsync(query);
            }
            catch            
            {
                return null;
            }
        }

        #region Bindable Properties - TapPoints, Buffers, Parcels, and BusyVisibility

        #region TapPoints
        /// <summary>
        /// Identifies the <see cref="TapPoints"/> dependency property
        /// </summary>
        private static readonly DependencyProperty TapPointsProperty = DependencyProperty.Register(
            "TapPoints", typeof(ObservableCollection<Graphic>), typeof(BufferAndQuery), null);

        /// <summary>
        /// Gets the set of points on the map where the user has tapped
        /// </summary>
        public ObservableCollection<Graphic> TapPoints
        {
            get { return GetValue(TapPointsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(TapPointsProperty, value); }
        }
        #endregion

        #region Buffers
        /// <summary>
        /// Identifies the <see cref="Buffers"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BuffersProperty = DependencyProperty.Register(
            "Buffers", typeof(ObservableCollection<Graphic>), typeof(BufferAndQuery), null);

        /// <summary>
        /// Gets the set of buffers that have been calculated around the tap points
        /// </summary>
        public ObservableCollection<Graphic> Buffers
        {
            get { return GetValue(BuffersProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(BuffersProperty, value); }
        }
        #endregion

        #region Parcels
        /// <summary>
        /// Identifies the <see cref="Parcels"/> dependency property
        /// </summary>
        private static readonly DependencyProperty ParcelsProperty = DependencyProperty.Register(
            "Parcels", typeof(ObservableCollection<Graphic>), typeof(BufferAndQuery), null);

        /// <summary>
        /// Gets the set of parcels that intersect the buffers
        /// </summary>
        public ObservableCollection<Graphic> Parcels
        {
            get { return GetValue(ParcelsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(ParcelsProperty, value); }
        }
        #endregion

        #region BusyVisibility
        /// <summary>
        /// Identifies the <see cref="BusyVisibility"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BusyVisibilityProperty = DependencyProperty.Register(
            "BusyVisibility", typeof(Visibility), typeof(BufferAndQuery), 
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets whether an operation is currently in progress, expressed as a 
        /// <see cref="System.Windows.Visibility">Visibility</see>
        /// </summary>
        public Visibility BusyVisibility
        {
            get { return (Visibility)GetValue(BusyVisibilityProperty); }
            private set { SetValue(BusyVisibilityProperty, value); }
        }
        #endregion

        #endregion
    }
}