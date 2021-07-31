// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntime;

namespace ArcGISRuntimeXamarin.Samples.QueryCQLFilters
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Query with CQL filters",
        category: "Layers",
        description: "Query data from an OGC API feature service using CQL filters.",
        instructions: "Enter a CQL query. Press the \"Apply query\" button to see the query applied to the OGC API features shown on the map.",
        tags: new[] { "CQL", "OGC", "OGC API", "browse", "catalog", "common query language", "feature table", "filter", "query", "service", "web" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class QueryCQLFilters : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Spinner _spinner;
        private EditText _maxFeatures;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query with CQL filters";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource. (This sample has the same interface as the navigation sample without rerouting)
            SetContentView(Resource.Layout.QueryCQLFilters);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _spinner = FindViewById<Spinner>(Resource.Id.whereClauseSpinner);
            _maxFeatures = FindViewById<EditText>(Resource.Id.maxFeatures);
        }
    }
}
