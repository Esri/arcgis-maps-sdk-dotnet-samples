// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.QueryCQLFilters
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Query with CQL filters",
        category: "Layers",
        description: "Query data from an OGC API feature service using CQL filters.",
        instructions: "Enter a CQL query. Press the \"Apply query\" button to see the query applied to the OGC API features shown on the map.",
        tags: new[] { "CQL", "OGC", "OGC API", "browse", "catalog", "common query language", "feature table", "filter", "query", "service", "web", "Featured" })]
    public class QueryCQLFilters : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Spinner _whereClauseSpinner;
        private EditText _maxFeatures;
        private Switch _dateSwitch;
        private Button _applyButton;
        private Button _resetButton;
        private Button _startDateButton;
        private Button _endDateButton;
        private ProgressBar _progressBar;

        private List<string> _whereClauses { get; } = new List<string>
        {
            // Empty query.
            "",

            // Sample Query 1: Query for features with an F_CODE attribute property of "AP010".
            "F_CODE = 'AP010'", // cql-text query
            "{ \"eq\" : [ { \"property\" : \"F_CODE\" }, \"AP010\" ] }", // cql-json query

            // Sample Query 2: Query for features with an F_CODE attribute property similar to "AQ".
            "F_CODE LIKE 'AQ%'", // cql-text query

            // Sample Query 3: use cql-json to combine "before" and "eq" operators with the logical "and" operator.
           "{\"and\":[{\"eq\":[{ \"property\" : \"F_CODE\" }, \"AP010\"]},{ \"before\":[{ \"property\" : \"ZI001_SDV\"},\"2013-01-01\"]}]}"
        };

        // Hold a reference to the OGC feature collection table.
        private OgcFeatureCollectionTable _featureTable;

        // Constants for the service URL and collection id.
        private const string ServiceUrl = "https://demo.ldproxy.net/daraa";

        // Note that the service defines the collection id which can be accessed via OgcFeatureCollectionInfo.CollectionId.
        private const string CollectionId = "TransportationGroundCrv";

        private DateTime? _startTime = new DateTime(2011, 6, 13);
        private DateTime? _endTime = new DateTime(2012, 1, 7);

        private TaskCompletionSource<DateTime> _dateTimeTCS;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query with CQL filters";

            CreateLayout();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Populate the UI.
            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, _whereClauses);
            _whereClauseSpinner.Adapter = adapter;

            _maxFeatures.Text = "3000";
            _startDateButton.Text = ((DateTime)_startTime).ToShortDateString();
            _endDateButton.Text = ((DateTime)_endTime).ToShortDateString();

            // Create the map with topographic basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;

                // Create the feature table from URI and collection id.
                _featureTable = new OgcFeatureCollectionTable(new Uri(ServiceUrl), CollectionId);

                // Set the feature request mode to manual (only manual is currently supported).
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Populate the OGC feature collection table.
                QueryParameters queryParamaters = new QueryParameters();
                queryParamaters.MaxFeatures = 3000;
                await _featureTable.PopulateFromServiceAsync(queryParamaters, false, null);

                // Create a feature layer from the OGC feature collection
                // table to visualize the OAFeat features.
                FeatureLayer ogcFeatureLayer = new FeatureLayer(_featureTable);

                // Set a renderer for the layer.
                ogcFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.5));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(ogcFeatureLayer);

                // Zoom to the extent of the feature layer/table.
                Envelope tableExtent = _featureTable.Extent;
                if (tableExtent != null && !tableExtent.IsEmpty)
                {
                    await _myMapView.SetViewpointGeometryAsync(tableExtent, 20);
                }
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                // Update the UI.
                _progressBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        public QueryParameters CreateQueryParameters()
        {
            // Create new query parameters.
            var queryParameters = new QueryParameters();

            // Assign the where clause if one is provided.
            if (_whereClauseSpinner.SelectedItem?.ToString() is string selectedClause)
            {
                queryParameters.WhereClause = selectedClause;
            }

            // Set the MaxFeatures property to MaxFeaturesBox content.
            if (int.TryParse(_maxFeatures.Text, out int parsedMaxFeatures))
            {
                queryParameters.MaxFeatures = parsedMaxFeatures;
            }

            // Set user date times if provided.
            if (_dateSwitch.Checked)
            {
                DateTime startDate = (_startTime is DateTime userStart) ? userStart : DateTime.MinValue;
                DateTime endDate = (_endTime is DateTime userEnd) ? userEnd : new DateTime(9999, 12, 31);

                // Use the newly created startDate and endDate to create the TimeExtent.
                queryParameters.TimeExtent = new Esri.ArcGISRuntime.TimeExtent(startDate, endDate);
            }

            return queryParameters;
        }

        private void CreateLayout()
        {
            // Load the layout from the xml resource.
            SetContentView(Resource.Layout.QueryCQLFilters);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _whereClauseSpinner = FindViewById<Spinner>(Resource.Id.whereClauseSpinner);
            _maxFeatures = FindViewById<EditText>(Resource.Id.maxFeatures);

            _dateSwitch = FindViewById<Switch>(Resource.Id.dateSwitch);

            _startDateButton = FindViewById<Button>(Resource.Id.startDateButton);
            _endDateButton = FindViewById<Button>(Resource.Id.endDateButton);
            _applyButton = FindViewById<Button>(Resource.Id.applyButton);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            _dateSwitch.CheckedChange += DateSwitchChanged;

            _startDateButton.Click += StartDateClick;
            _endDateButton.Click += EndDateClick;

            _applyButton.Click += ApplyQuery;
            _resetButton.Click += ResetQuery;
        }

        private void ResetQuery(object sender, EventArgs e)
        {
            _whereClauseSpinner.SetSelection(0);
            _maxFeatures.Text = "3000";
            _dateSwitch.Checked = true;
            _startTime = new DateTime(2011, 6, 13);
            _endTime = new DateTime(2012, 1, 7);
            _startDateButton.Text = ((DateTime)_startTime).ToShortDateString();
            _endDateButton.Text = ((DateTime)_endTime).ToShortDateString();
        }

        private async void ApplyQuery(object sender, EventArgs e)
        {
            if (_featureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                return;
            }

            try
            {
                _progressBar.Visibility = Android.Views.ViewStates.Visible;

                // Set queryParameters to the user's input
                var queryParameters = CreateQueryParameters();

                // Populate the table with the query, clearing the exist content of the table.
                // Setting outFields to null requests all fields.
                var result = await _featureTable.PopulateFromServiceAsync(queryParameters, true, outFields: null);

                // Zoom to the extent of the returned features.
                Envelope tableExtent = GeometryEngine.CombineExtents(result.Select(feature => feature.Geometry));
                if (tableExtent != null && !tableExtent.IsEmpty)
                {
                    await _myMapView.SetViewpointGeometryAsync(tableExtent, 20);
                }

                // Report the number of returned features by the query.
                new AlertDialog.Builder(this).SetMessage($"Query returned {result.Count()} features.").SetTitle("Query Completed").SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null).Show();
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
            finally
            {
                // Update the UI.
                _progressBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private async void StartDateClick(object sender, EventArgs e)
        {
            try
            {
                // Prompt the user with the default time or currently set time.
                DateTime promptDate = new DateTime(2011, 6, 13);
                if (_startTime is DateTime validStart)
                {
                    promptDate = validStart;
                }

                // Await the user selecting a date.
                DateTime? newTime = await PromptForDate(promptDate);
                if (newTime is DateTime time)
                {
                    _startTime = time;
                    _startDateButton.Text = time.ToShortDateString();
                }
                else
                {
                    _startTime = null;
                    _startDateButton.Text = "Start date";
                }
            }
            catch (Exception)
            {
            }
        }

        private async void EndDateClick(object sender, EventArgs e)
        {
            try
            {
                // Prompt the user with the default time or currently set time.
                DateTime promptDate = new DateTime(2011, 6, 13);
                if (_endTime is DateTime validEnd)
                {
                    promptDate = validEnd;
                }

                // Await the user selecting a date.
                DateTime? newTime = await PromptForDate(promptDate);
                if (newTime is DateTime time)
                {
                    _endTime = time;
                    _endDateButton.Text = time.ToShortDateString();
                }
                else
                {
                    _endTime = null;
                    _endDateButton.Text = "End date";
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task<DateTime?> PromptForDate(DateTime initialTime)
        {
            try
            {
                // Create the task completion source.
                _dateTimeTCS = new TaskCompletionSource<DateTime>();

                // Prompt the user for a datetime.
                DatePickerDialog dialog = new DatePickerDialog(this, DatePromptCallback, initialTime.Year, initialTime.Month, initialTime.Day);
                dialog.CancelEvent += (s, e) => { _dateTimeTCS.SetCanceled(); };
                dialog.Show();

                var result = await _dateTimeTCS.Task;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void DatePromptCallback(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            try
            {
                // Set the result of the task completion source using the date the user selected via the control.
                _dateTimeTCS.SetResult(e.Date);
            }
            catch (Exception)
            {
                _dateTimeTCS.TrySetCanceled();
            }
        }

        private void DateSwitchChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            _startDateButton.Enabled = _endDateButton.Enabled = e.IsChecked;
        }
    }
}