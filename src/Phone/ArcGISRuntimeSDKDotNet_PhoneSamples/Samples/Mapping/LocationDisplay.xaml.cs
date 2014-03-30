using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Microsoft.Phone.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Location = Esri.ArcGISRuntime.Location;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Mapping</category>
	public partial class LocationDisplay : PhoneApplicationPage
    {
        Location.LocationDisplay m_locationDisplay;

        public LocationDisplay()
        {
            InitializeComponent();
            m_locationDisplay = this.Resources["locationDisplay"] as Location.LocationDisplay;
        }

        // Update the location provider when a different one is selcted
        private async void LocationProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Make sure to stop the previous provider
            await stopRandomLocationProvider();

            // Update the map's location provider based on the selected index in the ListPicker
            ListPicker providerPicker = (ListPicker)sender;
            if (providerPicker.SelectedIndex == 0)
                m_locationDisplay.LocationProvider = new SystemLocationProvider();
            else if (providerPicker.SelectedIndex == 1)
                m_locationDisplay.LocationProvider = new RandomProvider();
        }

        // Updates the AutoPanMode when a different mode is selected
        private void NavMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the location display settings.  Store in a member variable so it only needs to be done once
            if (m_locationDisplay == null)
                m_locationDisplay = (Esri.ArcGISRuntime.Location.LocationDisplay)this.Resources["locationDisplay"];

            // Update the AutoPanMode with the selected one in the ListPicker
            m_locationDisplay.AutoPanMode = (AutoPanMode)((ListPicker)sender).SelectedItem;
        }

        // Toggles the settings UI on and off
        private void SettingsButton_Click(object sender, EventArgs e)
        {
            DisplaySettings.Visibility = DisplaySettings.Visibility == Visibility.Visible ? 
                Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowSettingsButton_Click(object sender, EventArgs e)
        {
            DisplaySettings.Visibility = HideSettingsButton.Visibility = Visibility.Visible;
            ShowSettingsButton.Visibility = Visibility.Collapsed;
        }

        private void HideSettingsButton_Click(object sender, EventArgs e)
        {
            DisplaySettings.Visibility = HideSettingsButton.Visibility = Visibility.Collapsed;
            ShowSettingsButton.Visibility = Visibility.Visible;
        }

        // Stop the random location provider when the page is navigated away from 
        protected override async void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            await stopRandomLocationProvider();
        }

        private async Task stopRandomLocationProvider()
        {
            if (m_locationDisplay.LocationProvider is RandomProvider)
                await ((RandomProvider)m_locationDisplay.LocationProvider).StopAsync();
        }
    }

    /// <summary>
    /// Location provider implementation that randomly generates location data based on the last position
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
                    Location = new MapPoint(StartLongitude, StartLatitude) { SpatialReference = new SpatialReference(4326) },
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

        public System.Threading.Tasks.Task StartAsync()
        {
            timer.Start();
            return Task.FromResult<bool>(true);
        }

        public System.Threading.Tasks.Task StopAsync()
        {
            timer.Stop();
            return Task.FromResult<bool>(true);
        }

        public event EventHandler<LocationInfo> LocationChanged;
    }
}