// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ConnectedTrace
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Connected trace",
        "Network Analysis",
        "Find all features connected to a given set of starting point(s) and barrier(s) in your network using the Connected trace type.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("ConnectedTrace.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class ConnectedTrace : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;

        private RadioButton _startButton;
        private RadioButton _barrierButton;
        private Button _traceButton;
        private Button _resetButton;
        private TextView _status;

        // real variables

        private readonly int[] LayerIds = new int[] { 4, 3, 5, 0 };

        private UtilityNetwork _utilityNetwork;
        private UtilityTraceParameters _parameters;

        private TaskCompletionSource<int> _terminalCompletionSource = null;

        private SimpleMarkerSymbol _startingPointSymbol;
        private SimpleMarkerSymbol _barrierPointSymbol;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Connected trace";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            _myMapView.Map = new Map(Basemap.CreateStreetsNightVector());
        }

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            _status.Text = "CLICKED";
            await WaitForTerminalAsync(null);
        }

        private async Task<UtilityTerminal> WaitForTerminalAsync(IEnumerable<UtilityTerminal> terminals)
        {
            string[] terminalNames = { "red", "green", "blue", "black" };

            //string[] terminalNames = terminals.ToList().Select(t => t.Name).ToArray();

            // Load the terminals into the UI.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select a terminal for this junction.");
            builder.SetItems(terminalNames, Choose_Click);
            builder.SetCancelable(false);

            builder.Show();

            // Return the selected terminal.
            _terminalCompletionSource = new TaskCompletionSource<int>();

            //return terminalNames[await _terminalCompletionSource.Task];
            _status.Text = terminalNames[await _terminalCompletionSource.Task];
            return null;
        }

        private void Choose_Click(object sender, DialogClickEventArgs e)
        {
            _terminalCompletionSource.TrySetResult(e.Which);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            SetContentView(Resource.Layout.ConnectedTrace);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);

            _startButton = FindViewById<RadioButton>(Resource.Id.addStart);
            _barrierButton = FindViewById<RadioButton>(Resource.Id.addBarrier);
            _traceButton = FindViewById<Button>(Resource.Id.traceButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _status = FindViewById<TextView>(Resource.Id.statusLabel);

            _myMapView.GeoViewTapped += GeoViewTapped;
        }
    }
}