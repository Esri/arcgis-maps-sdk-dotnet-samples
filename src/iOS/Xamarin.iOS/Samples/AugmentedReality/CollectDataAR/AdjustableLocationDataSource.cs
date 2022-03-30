// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;

namespace ArcGISRuntimeXamarin.Samples.CollectDataAR
{
    /// <summary>
    /// Wraps the built-in location data source to enable altitude adjustment.
    /// </summary>
    public class AdjustableLocationDataSource : LocationDataSource
    {
        // Track the altitude offset and raise location changed event when it is updated.
        private double _altitudeOffset = 0;
        public double AltitudeOffset
        {
            get => _altitudeOffset;
            set
            {
                _altitudeOffset = value;

                if (_lastLocation != null)
                {
                    _baseSource_LocationChanged(_baseSource, _lastLocation);
                }
            }
        }

        // Track the last location provided by the system.
        private Location _lastLocation;

        // The system's location data source.
        private SystemLocationDataSource _baseSource;

        public AdjustableLocationDataSource()
        {
            _baseSource = new SystemLocationDataSource();
            _baseSource.HeadingChanged += _baseSource_HeadingChanged;
            _baseSource.LocationChanged += _baseSource_LocationChanged;
        }

        private void _baseSource_LocationChanged(object sender, Location e)
        {
            // Store the last location; used to raise location changed event when only the offset is changed.
            _lastLocation = e;

            // Create the offset map point.
            MapPoint newPosition = new MapPoint(e.Position.X, e.Position.Y, e.Position.Z + AltitudeOffset, e.Position.SpatialReference);

            // Create a new location from the map point.
            Location newLocation = new Location(newPosition, e.HorizontalAccuracy, e.Velocity, e.Course, e.IsLastKnown);

            // Call the base UpdateLocation implementation.
            UpdateLocation(newLocation);
        }

        private void _baseSource_HeadingChanged(object sender, double e)
        {
            UpdateHeading(e);
        }

        protected override Task OnStartAsync() => _baseSource.StartAsync();

        protected override Task OnStopAsync() => _baseSource.StopAsync();
    }
}
