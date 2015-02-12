using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates use of the location display, and also how to create a custom location provider
    /// </summary>
    /// <title>Location Display</title>
    /// <category>Mapping</category>
    public sealed partial class LocationDisplay : Page
    {
        public LocationDisplay()
        {
            this.InitializeComponent();

            autoPanModeSelector.ItemsSource = Enum.GetValues(typeof(AutoPanMode));
        }

        private void LocationProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (providerSelector != null)
            {
                if (providerSelector.SelectedIndex == 0)
                    MyMapView.LocationDisplay.LocationProvider = new SystemLocationProvider();
                else if (providerSelector.SelectedIndex == 1)
                    MyMapView.LocationDisplay.LocationProvider = new RandomProvider();
            }
        }

        private async void resetDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // If the LocationDisplay is enabled and a Location currently exists, reset the map
                // to zero rotation and center on the Location. Otherwise, set the MapView to center on 0,0.
                if (MyMapView.LocationDisplay != null &&
                    MyMapView.LocationDisplay.IsEnabled &&
                    MyMapView.LocationDisplay.CurrentLocation != null &&
                    MyMapView.LocationDisplay.CurrentLocation.Location.Extent != null)
                {
                    // Get the current AutoPanMode setting as it is automatically disabled when calling MyMapView.SetView().
                    var PanMode = MyMapView.LocationDisplay.AutoPanMode;

                    MyMapView.SetRotation(0);
                    await MyMapView.SetViewAsync(MyMapView.LocationDisplay.CurrentLocation.Location);

                    // Reset the AutoPanMode 
                    MyMapView.LocationDisplay.AutoPanMode = PanMode;
                }
                else
                {
                    var viewpoint = new Viewpoint(MyMapView.Map.Layers[0].FullExtent) { Rotation = 0.0 };
                    await MyMapView.SetViewAsync(viewpoint);
                }
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Sample Error: " + ex.Message).ShowAsync();
            }
        }
    }

    /// <summary>
    /// This is serves as a custom location provider - in this case creating a randomly roaming location
    /// for simulating movement, accuracy, speed and heading changes.
    /// </summary>
    public class RandomProvider : ILocationProvider
    {
        private static Random randomizer = new Random();
        private DispatcherTimer timer;
        LocationInfo oldPosition;

        public RandomProvider()
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += timer_Tick;
            // Default start location
            StartLatitude = 34.057104;
            StartLongitude = -117.196816;
        }

        // Called when the position timer triggers, and calculates the next position based on current speed and heading,
        // and adds a little randomization to current heading, speed and accuracy.       
        private void timer_Tick(object sender, object e)
        {
            if (oldPosition == null)
            {
                oldPosition = new LocationInfo()
                {
                    Location = new MapPoint(StartLongitude, StartLatitude, new SpatialReference(4326)),
                    Speed = 0,
                    Course = 0,
                    HorizontalAccuracy = 20,
                };
            }
            var now = DateTime.Now;
            TimeSpan timeParsed = timer.Interval;
            double acceleration = randomizer.NextDouble() * 5 - 2.5;
            double deltaSpeed = acceleration * timeParsed.TotalSeconds;
            double newSpeed = Math.Max(0, deltaSpeed + oldPosition.Speed);
            double deltaCourse = randomizer.NextDouble() * 30 - 15;
            double newCourse = deltaCourse + oldPosition.Course;
            while (newCourse < 0) newCourse += 360;
            while (newCourse >= 360) newCourse -= 360;
            double distanceTravelled = (newSpeed + oldPosition.Speed) * .5 * timeParsed.TotalSeconds;
            double accuracy = Math.Min(500, Math.Max(20, oldPosition.HorizontalAccuracy + (randomizer.NextDouble() * 100 - 50)));
            var pos = GetPointFromHeadingGeodesic(new Point(oldPosition.Location.X, oldPosition.Location.Y), distanceTravelled, newCourse - 180);
            var newPosition = new LocationInfo()
            {
                Location = new MapPoint(pos.X, pos.Y, new SpatialReference(4326)),
                Speed = newSpeed,
                Course = newCourse,
                HorizontalAccuracy = accuracy,
            };
            oldPosition = newPosition;
            if (LocationChanged != null)
                LocationChanged(this, oldPosition);
        }


        // Gets a point on the globe based on a location, a heading and a distance.       
        private static Point GetPointFromHeadingGeodesic(Point start, double distance, double heading)
        {
            double brng = (180 + heading) / 180 * Math.PI;
            double lon1 = start.X / 180 * Math.PI;
            double lat1 = start.Y / 180 * Math.PI;
            double dR = distance / 6378137; //Angular distance in radians
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dR) + Math.Cos(lat1) * Math.Sin(dR) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(dR) * Math.Cos(lat1), Math.Cos(dR) - Math.Sin(lat1) * Math.Sin(lat2));
            double lon = lon2 / Math.PI * 180;
            double lat = lat2 / Math.PI * 180;
            while (lon < -180) lon += 360;
            while (lat < -90) lat += 180;
            while (lon > 180) lon -= 360;
            while (lat > 90) lat -= 180;
            return new Point(lon, lat);
        }

        public double StartLatitude { get; set; }

        public double StartLongitude { get; set; }

        public event EventHandler<LocationInfo> LocationChanged;

        public Task StartAsync()
        {
            timer.Start();
            return Task.FromResult<bool>(true);
        }

        public Task StopAsync()
        {
            timer.Stop();
            return Task.FromResult<bool>(true);
        }
    }
}
