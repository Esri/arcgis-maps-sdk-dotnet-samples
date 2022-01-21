// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Location;
using ArcGISRuntime.Samples.Managers;
using System.Collections.Generic;
using System;
using System.Timers;
using System.IO;

namespace ArcGISRuntimeXamarin.Samples.LocationWithNMEA
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display device location with NMEA data sources",
        "Location",
        "Parse NMEA sentences and use the results to show device location on the map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d5bad9f4fee9483791e405880fb466da")]
    public partial class LocationWithNMEA : ContentPage
    {
        private NmeaLocationDataSource _nmeaSource;

        private SimulatedNMEADataSource _simulatedNMEADataSource;

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

            // Create the simulated data.
            string nmeaPath = DataManager.GetDataFolder("d5bad9f4fee9483791e405880fb466da", "Redlands.nmea");
            _simulatedNMEADataSource = new SimulatedNMEADataSource(nmeaPath);

            // Create the NMEA data source.
            _nmeaSource = new NmeaLocationDataSource(SpatialReferences.Wgs84);
            _nmeaSource.NmeaDataStream = _simulatedNMEADataSource.DataStream;
            _nmeaSource.SatellitesChanged += SatellitesChanged;

            MyMapView.LocationDisplay.DataSource = _nmeaSource;

            await _nmeaSource.StartAsync();
        }

        private void SatellitesChanged(object sender, IReadOnlyList<NmeaSatelliteInfo> infos)
        {
            if (infos.Count == 0) return;
            SortedSet<int> uniqueSatelliteIds = new SortedSet<int>();
            foreach (var info in infos)
            {
                uniqueSatelliteIds.Add(info.Id);
            }

            //Dispatcher.BeginInvoke((Action)delegate ()
            //{
            //    // Show the status information in the UI.
            //    InfoLabel.Content = $"Satellite count: {infos.Count} Satellites: {string.Join(", ", uniqueSatelliteIds)} System: {infos.First().System} ";

            //});
        }

        private void StartClick(object sender, EventArgs e)
        {
            _simulatedNMEADataSource.Start();
        }

        private void RecenterClick(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
        }

        private void ResetClick(object sender, EventArgs e)
        {
            _simulatedNMEADataSource.Stop();
            _simulatedNMEADataSource.Reset();
        }
    }

    public class SimulatedNMEADataSource : IDisposable
    {
        private Timer _timer;

        public Stream DataStream;

        private int _count = 0;
        private const int DefaultInterval = 100;

        private string[] _nmeaStrings;

        public SimulatedNMEADataSource(string nmeaSourceFilePath, double speed = 1.0)
        {
            _timer = new Timer((int)(DefaultInterval * speed));

            _nmeaStrings = File.ReadAllText(nmeaSourceFilePath).Split('\n');

            DataStream = new MemoryStream();

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
            _count = 0;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            string nmeaString = $"{_nmeaStrings[_count]}\n";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(nmeaString);
            DataStream.Write(data, 0, data.Length);

            _count = (_count + 1) % _nmeaStrings.Length;
        }

        public void Dispose()
        {
            _timer.Dispose();
            DataStream.Dispose();
        }
    }
}