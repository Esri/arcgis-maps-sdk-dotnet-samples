using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
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
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class DriveTimes : PhoneApplicationPage
    {
        public DriveTimes()
        {
            InitializeComponent();

            // Initialize the graphics collections
            TapPoints = new ObservableCollection<Graphic>();
            DriveTimePolygons = new ObservableCollection<Graphic>();

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

        // On tap, either get related records for the tapped well or find nearby wells if no well was tapped
        private async void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
            if (BusyVisibility == Visibility.Visible)
            {
                MessageBox.Show("Please wait until the current operation is complete.");
                return;
            }

            // Show busy UI
            BusyVisibility = Visibility.Visible;

            // Clear previous results
            TapPoints.Clear();
            DriveTimePolygons.Clear();

            // Create graphic and add to tap points
            var g = new Graphic() { Geometry = e.Location };
            TapPoints.Add(g);

            string error = null;

            // Initialize the Geoprocessing task with the drive time calculation service endpoint
            Geoprocessor task = new Geoprocessor(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons"));

            // Initialize input parameters
            var parameter = new GPInputParameter();
            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Location", e.Location));
            parameter.GPParameters.Add(new GPString("Drive_Times", "1 2 3"));

            try
            {
                // Run the operation
                var result = await task.ExecuteAsync(parameter);

                // Check that layers were returned from the operation
                var outputLayers = result.OutParameters.OfType<GPFeatureRecordSetLayer>();
                if (outputLayers.Count() > 0)
                {
                    // Get the first layer returned - this will be the drive time areas
                    var outputLayer = outputLayers.First();
                    if (outputLayer.FeatureSet != null && outputLayer.FeatureSet.Features != null)
                    {
                        // Instead of adding drive time polygons one-by-one, update the collection all at once to
                        // allow the map to render the new features in one rendering pass.
                        DriveTimePolygons = new ObservableCollection<Graphic>(outputLayer.FeatureSet.Features);
                    }
                    else
                    {
                        error = "No results returned";
                    }
                }
                else
                {
                    error = "No results returned";
                }
            }
            catch (Exception ex)
            {
                error = "Drive time calculation failed: " + ex.Message;
            }

            // If operation did not succeed, notify user
            if (error != null)
                MessageBox.Show(error);

            // Hide busy UI
            BusyVisibility = Visibility.Collapsed;
        }

        #region Bindable Properties - TapPoints, DriveTimePolygons, and BusyVisibility

        #region TapPoints
        /// <summary>
        /// Identifies the <see cref="TapPoints"/> dependency property
        /// </summary>
        private static readonly DependencyProperty TapPointsProperty = DependencyProperty.Register(
            "TapPoints", typeof(ObservableCollection<Graphic>), typeof(DriveTimes), null);

        /// <summary>
        /// Gets the set of points on the map where the user has tapped
        /// </summary>
        public ObservableCollection<Graphic> TapPoints
        {
            get { return GetValue(TapPointsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(TapPointsProperty, value); }
        }
        #endregion

        #region DriveTimePolygons
        /// <summary>
        /// Identifies the <see cref="DriveTimePolygons"/> dependency property
        /// </summary>
        private static readonly DependencyProperty DriveTimePolygonsProperty = DependencyProperty.Register(
            "DriveTimePolygons", typeof(ObservableCollection<Graphic>), typeof(DriveTimes), null);

        /// <summary>
        /// Gets the set of drive time areas that have been calculated around the tap points
        /// </summary>
        public ObservableCollection<Graphic> DriveTimePolygons
        {
            get { return GetValue(DriveTimePolygonsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(DriveTimePolygonsProperty, value); }
        }
        #endregion

        #region BusyVisibility
        /// <summary>
        /// Identifies the <see cref="BusyVisibility"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BusyVisibilityProperty = DependencyProperty.Register(
            "BusyVisibility", typeof(Visibility), typeof(DriveTimes), 
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