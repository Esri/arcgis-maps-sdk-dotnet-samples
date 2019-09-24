// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Location = Esri.ArcGISRuntime.Location.Location;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    public class MSLAdjustedARLocationDataSource : LocationDataSource
    {
        public enum AltitudeAdjustmentMode
        {
            GpsRawEllipsoid,
            NmeaParsedMsl
        }

        private AltitudeAdjustmentMode _currentMode = AltitudeAdjustmentMode.GpsRawEllipsoid;

        private NmeaListener _listener = new NmeaListener();

        public AltitudeAdjustmentMode AltitudeMode {
            get => _currentMode;
            set
            {
                _currentMode = value;

                if (_currentMode == AltitudeAdjustmentMode.NmeaParsedMsl)
                {
                    getLocationManager().AddNmeaListener(_listener);
                }
            }
        }

        public double AltitudeOffset { get; set; } = 0;

        public IntPtr Handle => throw new NotImplementedException();

        private double _lastNmeaElevation = 0;

        private SystemLocationDataSource _baseSource;
        private Context _context;
        public MSLAdjustedARLocationDataSource(Context context)
        {
            _context = context;
            _baseSource = new SystemLocationDataSource();
            _baseSource.HeadingChanged += _baseSource_HeadingChanged;
            _baseSource.LocationChanged += _baseSource_LocationChanged;

            _listener.NmeaAltitudeChanged += (o, e) =>
            {
                _lastNmeaElevation = e.Altitude;
            };
        }

        private void _baseSource_LocationChanged(object sender, Location e)
        {
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

        private void _baseSource_HeadingChanged(object sender, double e)
        {
            UpdateHeading(e);
        }

        protected override Task OnStartAsync()
        {
            return _baseSource.StartAsync();
        }

        protected override Task OnStopAsync()
        {
            return _baseSource.StopAsync();
        }

        private LocationManager _locationManager;

        private LocationManager getLocationManager()
        {
            if (_locationManager == null)
            {
                _locationManager = (LocationManager)_context.GetSystemService("location");
            }
            return _locationManager;
        }

        private class NmeaListener : Java.Lang.Object, IOnNmeaMessageListener
        {
            private long _lastTimestamp = 0;
            private double _lastElevation = 0;

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

                    if (String.IsNullOrEmpty(mslAltitude)) { return; }

                    Double altitudeParsed;

                    if (Double.TryParse(mslAltitude, out altitudeParsed))
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
