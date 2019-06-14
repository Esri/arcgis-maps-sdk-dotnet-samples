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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ArcGISRuntimeXamarin.Samples.ShowLocationHistory
{
    class FakeLocationDataSource : LocationDataSource
    {
        // Center around which the fake locations are centered.
        private readonly MapPoint _circleRouteCenter = new MapPoint(-117.195801, 34.056007, SpatialReferences.Wgs84);

        // List of points around the circle for use in generating fake locations.
        private readonly List<MapPoint> _artificialMapPoints;

        // Update the location about once per second.
        private readonly Timer _timer;

        // Randomizer. Provide a seed if you want predictable behavior.
        private readonly Random _randomizer = new Random();

        // Index keeps track of where on the fake track you are.
        private int _locationIndex = 0;

        public FakeLocationDataSource()
        {
            // Generate the points that will be used.

            // The location will walk around a circle around the point.
            Polygon outerCircle = (Polygon)GeometryEngine.BufferGeodetic(_circleRouteCenter, 1000, LinearUnits.Feet);

            // Get a list of points on the circle from the buffered point.
            _artificialMapPoints = outerCircle.Parts[0].Points.ToList();

            // Create the timer and configure it to repeat every second.
            _timer = new Timer(1000);
            _timer.AutoReset = true;

            // Listen for timer elapsed events.
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_randomizer.Next(0, 10) > 8)
            {
                // Don't send an update about 20% of the time.
                return;
            }

            // Get the next point along the circle.
            MapPoint selectedMapPoint = _artificialMapPoints[_locationIndex % _artificialMapPoints.Count];

            // Calculate a random offset to move the point.
            double horizontalOffset = _randomizer.NextDouble() * .001;
            double verticalOffset = _randomizer.NextDouble() * .001;

            // Create a new random point.
            MapPoint randomMapPoint = new MapPoint(selectedMapPoint.X + horizontalOffset, selectedMapPoint.Y + verticalOffset, selectedMapPoint.SpatialReference);

            // Update the location; this will trigger a `LocationChanged` event.
            this.UpdateLocation(new Location(randomMapPoint, 5, 0, 0, false));

            // Increment the location index.
            _locationIndex++;
        }

        protected override Task OnStartAsync()
        {
            _timer.Start();
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync()
        {
            _timer.Stop();
            return Task.CompletedTask;
        }
    }
}