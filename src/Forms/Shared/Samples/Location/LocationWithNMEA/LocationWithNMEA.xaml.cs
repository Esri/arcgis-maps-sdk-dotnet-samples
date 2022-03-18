﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.LocationWithNMEA
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display device location with NMEA data sources",
        category: "Location",
        description: "Parse NMEA sentences and use the results to show device location on the map.",
        instructions: "Tap \"Start\" to parse the NMEA sentences into a simulated location data source, and initiate the location display. Tap \"Recenter\" to recenter the location display. Tap \"Reset\" to reset the location display.",
        tags: new[] { "GNSS", "GPS", "NMEA", "RTK", "dongle", "history", "navigation", "real-time", "trace" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d5bad9f4fee9483791e405880fb466da")]
    public partial class LocationWithNMEA : ContentPage, IDisposable
    {
        private NmeaLocationDataSource _nmeaSource;

        private NMEAStreamSimulator _simulatedNMEADataSource;

        public LocationWithNMEA()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);
            MyMapView.SetViewpoint(new Viewpoint(new MapPoint(-117.191, 34.0306, SpatialReferences.Wgs84), 100000));

            // Create the data simulation using stored mock data.
            _simulatedNMEADataSource = new NMEAStreamSimulator(6.0);
            _simulatedNMEADataSource.NmeaMessageChanged += UpdateNmeaMessageLabel;

            // Create the NMEA data source.
            _nmeaSource = new NmeaLocationDataSource(SpatialReferences.Wgs84);

            // Android and UWP:
            // To create a NmeaLocationDataSource for a bluetooth device, use the `FromBluetooth` constructor. https://developers.arcgis.com/net/api-reference/api/android/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.NmeaLocationDataSource.FromBluetooth.html
            // To create a NmeaLocationDataSource from a serial port, use the `FromSerialPort` constructor. https://developers.arcgis.com/net/api-reference/api/android/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.NmeaLocationDataSource.FromSerialPort.html

            // iOS:
            // When using an NMEA device on iOS, use the `NmeaLocationDataSource.FromAccessory` constructor. https://developers.arcgis.com/net/api-reference/api/ios/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Location.NmeaLocationDataSource.FromAccessory.html

            // Set the location data source to use the stream from the simulator.
            Stream messageStream = _simulatedNMEADataSource.MessageStream;
            _nmeaSource.NmeaDataStream = messageStream;

            // Create an event handler to update the UI when the location changes.
            _nmeaSource.SatellitesChanged += SatellitesChanged;
            _nmeaSource.LocationChanged += LocationChanged;

            // Start the location data source.
            try
            {
                await MyMapView.Map.LoadAsync();
                MyMapView.LocationDisplay.DataSource = _nmeaSource;
                await _nmeaSource.StartAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.Message.GetType().Name, ex.Message, "OK");
            }
        }

        private void UpdateNmeaMessageLabel(object sender, NmeaMessageEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NmeaMessageLabel.Text = e.NmeaMessage;
            });
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

            Device.BeginInvokeOnMainThread(() =>
            {
                // Show the status information in the UI.
                CountLabel.Text = $"Satellite count: {infos.Count}";
                SatellitesLabel.Text = $"Satellites: {string.Join(", ", uniqueSatelliteIds)}";
                SystemLabel.Text = $"System: {infos.First().System}";
            });
        }

        private void LocationChanged(object sender, Location loc)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Show the status information in the UI.
                AccuracyLabel.Text = $"Accuracy: Horizontal {string.Format("{0:0.00}", loc.HorizontalAccuracy)} meters, Vertical  {string.Format("{0:0.00}", loc.VerticalAccuracy)} meters";

                // Recenter on the new location.
                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            });
        }

        private void StartClick(object sender, EventArgs e)
        {
            _simulatedNMEADataSource.Start();
            AccuracyLabel.Text = string.Empty;
        }

        private void RecenterClick(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
        }

        private void ResetClick(object sender, EventArgs e)
        {
            _simulatedNMEADataSource.Stop();
            _simulatedNMEADataSource.Reset();

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
            _simulatedNMEADataSource?.Dispose();
        }
    }

    /*
     * This class uses mock data (an edited recording of a real NMEA data stream) to simulate live NMEA data and create a stream.
     * For NMEA location data sources created from a Bluetooth device or serial input, you may not need to create your own stream.
     * For any other case, you can write the data to a memory stream like below.
     */

    public class NMEAStreamSimulator : IDisposable
    {
        private Timer _timer;

        public Stream MessageStream;

        private int _lineCounter = 0;
        private const int DefaultInterval = 1000;

        private string[] _nmeaStrings;

        public event EventHandler<NmeaMessageEventArgs> NmeaMessageChanged;

        public NMEAStreamSimulator(double speed = 1.0)
        {
            _timer = new Timer(DefaultInterval / speed);

            // Populate an array with all of the mock NMEA data.
            string nmeaMockDataPath = DataManager.GetDataFolder("d5bad9f4fee9483791e405880fb466da", "Redlands.nmea");
            _nmeaStrings = File.ReadAllText(nmeaMockDataPath).Split('\n');

            // Create a data stream for the `NmeaLocationDataSource` to use.
            MessageStream = new MemoryStream();

            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Reset()
        {
            _timer.Stop();
            _lineCounter = 0;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Get the next NMEA string and append return and newline characters to it.
            string nmeaString = $"{_nmeaStrings[_lineCounter]}\r\n";

            NmeaMessageChanged?.Invoke(this, new NmeaMessageEventArgs { NmeaMessage = nmeaString });

            // Check if the start of a new location message.
            if (nmeaString.StartsWith("$GPGGA"))
            {
                // Flush any existing NMEA data from the stream.
                MessageStream.Flush();
            }

            // Write the string to the stream.
            byte[] data = System.Text.Encoding.UTF8.GetBytes(nmeaString);
            MessageStream.Write(data, 0, data.Length);

            // Increment the line counter.
            _lineCounter = (_lineCounter + 1) % _nmeaStrings.Length;
        }

        public void Dispose()
        {
            // Dispose of the timer and stream.
            _timer.Stop();
            _timer.Dispose();
            MessageStream.Dispose();
        }
    }

    public class NmeaMessageEventArgs : EventArgs
    {
        public string NmeaMessage { get; set; }
    }
}