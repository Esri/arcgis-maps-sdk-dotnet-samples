// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ArcGIS.WinUI.Samples.LocationWithNMEA
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display device location with NMEA data sources",
        category: "Location",
        description: "Parse NMEA sentences and use the results to show device location on the map.",
        instructions: "Tap \"Start\" to parse the NMEA sentences into a simulated location data source, and initiate the location display. Tap \"Recenter\" to recenter the location display. Tap \"Reset\" to reset the location display.",
        tags: new[] { "GNSS", "GPS", "NMEA", "RTK", "dongle", "history", "navigation", "real-time", "trace" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("d5bad9f4fee9483791e405880fb466da")]
    public partial class LocationWithNMEA : IDisposable
    {
        private NmeaLocationDataSource _nmeaSource;

        public LocationWithNMEA()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);
            MyMapView.SetViewpoint(new Viewpoint(new MapPoint(-117.191, 34.0306, SpatialReferences.Wgs84), 100000));

            // Create the data simulation using stored mock data.
            string nmeaMockDataPath = DataManager.GetDataFolder("d5bad9f4fee9483791e405880fb466da", "Redlands.nmea");

            // Create the NMEA data source.
            _nmeaSource = NmeaLocationDataSource.FromStreamCreator((datasource) => Task.FromResult<Stream>(new SimulatedNmeaStream(nmeaMockDataPath)));
            // To create a NmeaLocationDataSource for a bluetooth device, use the `FromBluetooth` constructor. https://developers.arcgis.com/net/api-reference/api/uwp/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.NmeaLocationDataSource.FromBluetooth.html
            // To create a NmeaLocationDataSource from a serial port, use the `FromSerialPort` constructor. https://developers.arcgis.com/net/api-reference/api/uwp/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.NmeaLocationDataSource.FromSerialPort.html

            // Create an event handler to update the UI when the location changes.
            _nmeaSource.SatellitesChanged += SatellitesChanged;
            _nmeaSource.LocationChanged += LocationChanged;
            _nmeaSource.SentenceReceived += UpdateNmeaMessageLabel;

            MyMapView.LocationDisplay.DataSource = _nmeaSource;
        }

        private void UpdateNmeaMessageLabel(object sender, string message)
        {
            if (message.StartsWith("$GPRMC")) // Display the latest RMC message
            {
                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => NmeaMessageLabel.Text = message);
            }
        }

        private void SatellitesChanged(object sender, IReadOnlyList<NmeaSatelliteInfo> infos)
        {
            // Check for any satellite info.
            if (infos.Count == 0) return;

            // Get all of the satellite Id's from the satellite infos.
            SortedSet<int> uniqueSatelliteIds = new SortedSet<int>();
            foreach (var info in infos)
            {
                uniqueSatelliteIds.Add(info.Id);
            }

            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                // Show the status information in the UI.
                CountLabel.Text = $"Satellite count: {infos.Count}";
                SatellitesLabel.Text = $"Satellites: {string.Join(", ", uniqueSatelliteIds)}";
                SystemLabel.Text = $"System: {infos.First().System}";
            });
        }

        private void LocationChanged(object sender, Location loc)
        {
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                // Show the status information in the UI.
                AccuracyLabel.Text = $"Accuracy: H {string.Format("{0:0.00}", loc.HorizontalAccuracy)} meters, V {string.Format("{0:0.00}", loc.VerticalAccuracy)} meters";

                // Recenter on the new location.
                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            });
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            _nmeaSource.StartAsync();
            AccuracyLabel.Text = string.Empty;
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            _nmeaSource.StopAsync();

            // Reset the labels.
            AccuracyLabel.Text = "Simulation reset.";
            CountLabel.Text = string.Empty;
            SatellitesLabel.Text = string.Empty;
            SystemLabel.Text = string.Empty;
            NmeaMessageLabel.Text = string.Empty;
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }

    /*
     * This class uses mock data (an edited recording of a real NMEA data stream) to simulate live NMEA data and create a stream.
     * For NMEA location data sources created from a Bluetooth device or serial input, you may not need to create your own stream.
     * For any other case, you can simulate the datastream from a file like below
     */

    public class SimulatedNmeaStream : Stream
    {
        // The default interval in milliseconds between bursts of NMEA data.
        private const int DefaultInterval = 1000;

        private readonly System.Timers.Timer _timer;

        private readonly StreamReader _sr;

        private int _pendingBursts = 0;

        public SimulatedNmeaStream(string file)
        {
            // Populate an array with all of the mock NMEA data.
            _sr = new StreamReader(file);
            _timer = new System.Timers.Timer(DefaultInterval);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) => Interlocked.Increment(ref _pendingBursts);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of the timer and stream.
                _timer.Stop();
                _timer.Dispose();
                _sr.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Implementation of a custom System.IO.Stream 

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pendingBursts == 0)
                //Nothing in the buffer to read
                return 0;

            // Read all the pending bursts of data until we fill up the buffer
            var start = _sr.BaseStream.Position;
            StringBuilder sb = new StringBuilder();
            while (sb.Length < count && !_sr.EndOfStream && _pendingBursts > 0)
            {
                string line = _sr.ReadLine();
                // In this sample we pause the burst of messages for each GGA message to simulate the break in the nmea stream on a receiver
                if (line.StartsWith("$GPGGA,") && Interlocked.Decrement(ref _pendingBursts) == 0)
                {
                    break;
                }
                sb.AppendLine(line);
            }

            if (sb.Length == 0)
                return 0;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            count = Math.Min(count, data.Length);
            Array.Copy(data, 0, buffer, offset, count);
            // move the stream position forward only the amount of data we read
            _sr.BaseStream.Seek(start + count, SeekOrigin.Begin);
            return count;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => -1;

        public override long Position { get => _sr.BaseStream.Position; set => throw new NotImplementedException(); }

        public override void Flush() => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        #endregion
    }
}