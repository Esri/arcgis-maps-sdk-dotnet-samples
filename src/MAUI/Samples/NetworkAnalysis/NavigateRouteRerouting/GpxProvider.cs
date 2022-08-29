// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Location = Esri.ArcGISRuntime.Location.Location;
using Timer = System.Timers.Timer;

namespace ArcGISRuntime.Samples.NavigateRouteRerouting
{
    public class GpxProvider : LocationDataSource
    {
        private Timer _driveTimer;
        private double _timerValue = 500;

        private List<MapPoint> _gpsPoints;
        private int _nodeIndex;

        private double _course;
        private double _speed;

        public GpxProvider(string gpxFilePath)
        {
            _gpsPoints = new List<MapPoint>();

            // Read the GPX data.
            ReadLocations(gpxFilePath);

            // Create a timer for updating the location.
            _driveTimer = new Timer(_timerValue)
            {
                Enabled = false
            };
            _driveTimer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Check that there are remaining nodes.
            if (_nodeIndex > _gpsPoints.Count - 1)
            {
                _driveTimer.Enabled = false;
                return;
            }

            // Check if there is a previous node.
            if (_nodeIndex > 0)
            {
                // Get the distance between the current node and the previous node.
                GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(_gpsPoints[_nodeIndex], _gpsPoints[_nodeIndex - 1], LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic);

                // Get the course in degrees.
                _course = distance.Azimuth2;

                // Calculate the speed in m/s by using the distance and the timer interval.
                _speed = distance.Distance / (_timerValue / 1000);
            }

            // Set the new location.
            UpdateLocation(new Location(_gpsPoints[_nodeIndex], 5, _speed, _course, false));

            // Increment the node index.
            _nodeIndex++;
        }

        protected override Task OnStartAsync()
        {
            _driveTimer.Enabled = true;
            return Task.FromResult(true);
        }

        protected override Task OnStopAsync()
        {
            _driveTimer.Enabled = false;
            return Task.FromResult(true);
        }

        private void ReadLocations(string gpx)
        {
            // Load the GPX data.
            XmlDocument gpxDoc = new XmlDocument();
            gpxDoc.Load(gpx);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(gpxDoc.NameTable);

            // Get the schema information for GPX.
            namespaceManager.AddNamespace("x", "http://www.topografix.com/GPX/1/1");

            // Get the tracking points from the GPX data.
            XmlNodeList xmlNodes;
            xmlNodes = gpxDoc.SelectNodes("//x:trkpt", namespaceManager);

            foreach (XmlNode locationNode in xmlNodes)
            {
                // Get the location from the node.
                string longitude = locationNode.Attributes["lon"].Value;
                string latitude = locationNode.Attributes["lat"].Value;
                double.TryParse(longitude, out double x);
                double.TryParse(latitude, out double y);
                _gpsPoints.Add(new MapPoint(x, y, SpatialReferences.Wgs84));
            }
        }
    }
}