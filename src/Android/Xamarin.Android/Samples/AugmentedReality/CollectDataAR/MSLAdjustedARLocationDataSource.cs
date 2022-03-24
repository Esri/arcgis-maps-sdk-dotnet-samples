// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace ArcGISRuntimeXamarin.Samples.CollectDataAR
{
    /// <summary>
    /// Custom location data source that allows you to apply an altitude offset in addition to
    /// returning altitude values relative to mean sea level, rather than the WGS84 ellipsoid.
    /// </summary>
    public class MSLAdjustedARLocationDataSource : LocationDataSource
    {
        public enum AltitudeAdjustmentMode
        {
            GpsRawEllipsoid,
            NmeaParsedMsl
        }
        private AltitudeAdjustmentMode _currentMode = AltitudeAdjustmentMode.GpsRawEllipsoid;

        // Enable configuration of the altitude mode, adding or removing NMEA listener as needed.
        public AltitudeAdjustmentMode AltitudeMode {
            get => _currentMode;
            set
            {
                _currentMode = value;

                if (_currentMode == AltitudeAdjustmentMode.NmeaParsedMsl)
                {
                    GetLocationManager().AddNmeaListener(_listener);
                }
                else
                {
                    GetLocationManager().RemoveNmeaListener(_listener);
                }
            }
        }

        // Object to handle NMEA messages from the onboard GNSS device.
        private readonly NmeaListener _listener = new NmeaListener();

        // Allow setting an altitude offset.
        private double _altitudeOffset;
        public double AltitudeOffset
        {
            get => _altitudeOffset;
            set
            {
                _altitudeOffset = value;

                // Raise a location changed event if possible.
                if (_lastLocation != null)
                {
                    BaseSource_LocationChanged(_baseSource, _lastLocation);
                }
            }
        }

        // Track the last location so that a location changed
        // event can be raised when the altitude offset is changed.
        private Location _lastLocation;

        public IntPtr Handle => throw new NotImplementedException();

        // Track the last elevation received from the GNSS.
        private double _lastNmeaElevation;

        // Use the underlying system location data source.
        private readonly SystemLocationDataSource _baseSource;

        private readonly Context _context;

        public MSLAdjustedARLocationDataSource(Context context)
        {
            _context = context;

            // Create and listen for updates from a new system location data source.
            _baseSource = new SystemLocationDataSource();
            _baseSource.HeadingChanged += BaseSource_HeadingChanged;
            _baseSource.LocationChanged += BaseSource_LocationChanged;

            // Listen for altitude change events from the onboard GNSS.
            _listener.NmeaAltitudeChanged += (o, e) =>
            {
                _lastNmeaElevation = e.Altitude;
            };
        }

        private void BaseSource_LocationChanged(object sender, Location e)
        {
            // Store the last location to enable raising change events.
            _lastLocation = e;

            // Intercept location change events from the base source and either
            // apply an altitude offset, or return the offset altitude from the latest NMEA message.
            MapPoint newPosition = null;
            switch (AltitudeMode)
            {
                case AltitudeAdjustmentMode.GpsRawEllipsoid:
                    newPosition = new MapPoint(e.Position.X, e.Position.Y, e.Position.Z + AltitudeOffset, e.Position.SpatialReference);
                    break;
                case AltitudeAdjustmentMode.NmeaParsedMsl:
                    newPosition = new MapPoint(e.Position.X, e.Position.Y, _lastNmeaElevation + AltitudeOffset, e.Position.SpatialReference);
                    break;
            }

            Location newLocation = new Location(newPosition, e.HorizontalAccuracy, e.Velocity, e.Course, e.IsLastKnown);

            UpdateLocation(newLocation);
        }

        private void BaseSource_HeadingChanged(object sender, double e)
        {
            UpdateHeading(e);
        }

        protected override Task OnStartAsync() => _baseSource.StartAsync();

        protected override Task OnStopAsync() => _baseSource.StopAsync();

        private LocationManager _locationManager;

        private LocationManager GetLocationManager()
        {
            if (_locationManager == null)
            {
                _locationManager = (LocationManager)_context.GetSystemService("location");
            }
            return _locationManager;
        }

        private class NmeaListener : Java.Lang.Object, IOnNmeaMessageListener
        {
            private long _lastTimestamp;
            private double _lastElevation;

            public event EventHandler<AltitudeEventArgs> NmeaAltitudeChanged;

            public void OnNmeaMessage(string message, long timestamp)
            {
                if (message.StartsWith("$GPGGA") || message.StartsWith("$GNGNS") || message.StartsWith("$GNGGA"))
                {
                    var parts = message.Split(',');

                    if (parts.Length < 10)
                    {
                        return; // not enough
                    }

                    string mslAltitude = parts[9];

                    if (string.IsNullOrEmpty(mslAltitude)) { return; }


                    if (double.TryParse(mslAltitude, NumberStyles.Float, CultureInfo.InvariantCulture, out double altitudeParsed))
                    {
                        if (timestamp > _lastTimestamp)
                        {
                            _lastElevation = altitudeParsed;
                            _lastTimestamp = timestamp;
                            NmeaAltitudeChanged?.Invoke(this, new AltitudeEventArgs { Altitude = _lastElevation });
                        }
                    }
                }
            }

            public class AltitudeEventArgs
            {
                public double Altitude { get; set; }
            }
        }
    }
}
