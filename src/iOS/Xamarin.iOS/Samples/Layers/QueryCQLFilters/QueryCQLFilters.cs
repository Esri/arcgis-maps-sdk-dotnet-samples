// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
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
    [Register("QueryCQLFilters")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Query with CQL filters",
        category: "Layers",
        description: "Query data from an OGC API feature service using CQL filters.",
        instructions: "Enter a CQL query. Press the \"Apply query\" button to see the query applied to the OGC API features shown on the map.",
        tags: new[] { "CQL", "OGC", "OGC API", "browse", "catalog", "common query language", "feature table", "filter", "query", "service", "web", "Featured" })]
    public class QueryCQLFilters : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIActivityIndicatorView _activityIndicator;
        private UISwitch _dateSwitch;
        private UITextField _maxFeaturesEntry;
        private UIButton _clauseButton;
        private UIBarButtonItem _applyButton;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _startDateButton;
        private UIBarButtonItem _endDateButton;

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

        private string _whereClause;

        private TaskCompletionSource<DateTime> _dateTimeTCS;

        public QueryCQLFilters()
        {
            Title = "Query with CQL filters";
        }

        private async Task Initialize()
        {
            // Populate the UI.
            _maxFeaturesEntry.Text = "3000";
            _startDateButton.Title = ((DateTime)_startTime).ToShortDateString();
            _endDateButton.Title = ((DateTime)_endTime).ToShortDateString();

            // Create the map with topographic basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                _activityIndicator.StartAnimating();

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

                // Choose a renderer for the layer based on the table.
                ogcFeatureLayer.Renderer = GetRendererForTable(_featureTable) ?? ogcFeatureLayer.Renderer;

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
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
            finally
            {
                // Update the UI.
                _activityIndicator.StopAnimating();
            }
        }

        private Renderer GetRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 4));

                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Green, null));

                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.5));
            }

            return null;
        }

        public QueryParameters CreateQueryParameters()
        {
            // Create new query parameters.
            var queryParameters = new QueryParameters();

            // Assign the where clause if one is provided.
            if (!string.IsNullOrEmpty(_whereClause)) queryParameters.WhereClause = _whereClause;

            // Set the MaxFeatures property to MaxFeaturesBox content.
            if (int.TryParse(_maxFeaturesEntry.Text, out int parsedMaxFeatures))
                queryParameters.MaxFeatures = parsedMaxFeatures;

            // Set user date times if provided.
            if (_dateSwitch.On)
            {
                DateTime startDate = (_startTime is DateTime userStart) ? userStart : DateTime.MinValue;
                DateTime endDate = (_endTime is DateTime userEnd) ? userEnd : new DateTime(9999, 12, 31);

                // Use the newly created startDate and endDate to create the TimeExtent.
                queryParameters.TimeExtent = new Esri.ArcGISRuntime.TimeExtent(startDate, endDate);
            }

            return queryParameters;
        }

        private void ResetClicked(object sender, EventArgs e)
        {
            _whereClause = null;
            _clauseButton.SetTitle("Select Clause", UIControlState.Normal);
            _maxFeaturesEntry.Text = "3000";
            _dateSwitch.On = true;
            _startTime = new DateTime(2011, 6, 13);
            _endTime = new DateTime(2012, 1, 7);
            
            _startDateButton.Title = ((DateTime)_startTime).ToShortDateString();
            _endDateButton.Title = ((DateTime)_endTime).ToShortDateString();
        }

        private async void ApplyClicked(object sender, EventArgs e)
        {
            if (_featureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                return;
            }

            try
            {
                _activityIndicator.StartAnimating();

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
                new UIAlertView("Query Completed", $"Query returned {result.Count()} features.", (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
            finally
            {
                // Update the UI.
                _activityIndicator.StopAnimating();
            }
        }

        

        private async void StartDateClick(object sender, EventArgs e)
        {
            try
            {
                DateTime promptDate = new DateTime(2011, 6, 13);
                if (_startTime is DateTime validStart) promptDate = validStart;

                DateTime? newTime = await PromptForDate(promptDate);
                if (newTime is DateTime time)
                {
                    _startTime = time;
                    _startDateButton.Title = time.ToShortDateString();
                }
                else
                {
                    _startTime = null;
                    _startDateButton.Title = "Start date";
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
                DateTime promptDate = new DateTime(2011, 6, 13);
                if (_endTime is DateTime validEnd) promptDate = validEnd;

                DateTime? newTime = await PromptForDate(promptDate);
                if (newTime is DateTime time)
                {
                    _endTime = time;
                    _endDateButton.Title = time.ToShortDateString();
                }
                else
                {
                    _endTime = null;
                    _endDateButton.Title = "End date";
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task<DateTime?> PromptForDate(DateTime promptDate)
        {
            return null;
            
        }

        private void ClauseSelected(UIAlertAction obj)
        {
            throw new NotImplementedException();
        }

        

        private void DateSwitched(object sender, EventArgs e)
        {
            _startDateButton.Enabled = _endDateButton.Enabled = _dateSwitch.On;
        }

        private void ClauseClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HidesWhenStopped = true,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f)
            };

            UILabel clauseLabel = new UILabel { Text = "Where clause:", TranslatesAutoresizingMaskIntoConstraints = false };
            _clauseButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _clauseButton.SetTitle("Select Clause", UIControlState.Normal);
            _clauseButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            var clauseStack = GetRowStackView(new UIView[] { clauseLabel, _clauseButton });

            UILabel maxLabel = new UILabel { Text = "Maximum features:", TranslatesAutoresizingMaskIntoConstraints = false };
            _maxFeaturesEntry = new UITextField { TranslatesAutoresizingMaskIntoConstraints = false, Text = "3000", KeyboardType = UIKeyboardType.NumberPad };
            var maxFeaturesStack = GetRowStackView(new UIView[] { maxLabel, _maxFeaturesEntry });

            UILabel dateLabel = new UILabel { Text = "Time extent:", TranslatesAutoresizingMaskIntoConstraints = false };
            _dateSwitch = new UISwitch { TranslatesAutoresizingMaskIntoConstraints = false, On = true };
            var dateSwitchStack = GetRowStackView(new UIView[] { dateLabel, _dateSwitch });

            _startDateButton = new UIBarButtonItem { Title = "Start Date" };
            _endDateButton = new UIBarButtonItem { Title = "End Date" };

            var dateToolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };
            dateToolbar.Items = new[]
            {
                _startDateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _endDateButton
            };

            _applyButton = new UIBarButtonItem { Title = "Apply" };
            _resetButton = new UIBarButtonItem { Title = "Reset" };

            var applyToolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };
            applyToolbar.Items = new[]
            {
                _applyButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, _activityIndicator, dateSwitchStack, maxFeaturesStack, clauseStack, dateToolbar, applyToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(clauseStack.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                clauseStack.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                clauseStack.BottomAnchor.ConstraintEqualTo(maxFeaturesStack.TopAnchor),
                clauseStack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                clauseStack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                maxFeaturesStack.TopAnchor.ConstraintEqualTo(clauseStack.BottomAnchor),
                maxFeaturesStack.BottomAnchor.ConstraintEqualTo(dateSwitchStack.TopAnchor),
                maxFeaturesStack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                maxFeaturesStack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                dateSwitchStack.TopAnchor.ConstraintEqualTo(maxFeaturesStack.BottomAnchor),
                dateSwitchStack.BottomAnchor.ConstraintEqualTo(dateToolbar.TopAnchor),
                dateSwitchStack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                dateSwitchStack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                dateToolbar.TopAnchor.ConstraintEqualTo(dateSwitchStack.BottomAnchor),
                dateToolbar.BottomAnchor.ConstraintEqualTo(applyToolbar.TopAnchor),
                dateToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                dateToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                applyToolbar.TopAnchor.ConstraintEqualTo(dateToolbar.BottomAnchor),
                applyToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                applyToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                applyToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _activityIndicator.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _activityIndicator.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _activityIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _activityIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        private UIStackView GetRowStackView(UIView[] views)
        {
            UIStackView row = new UIStackView(views);
            row.TranslatesAutoresizingMaskIntoConstraints = false;
            row.Spacing = 8;
            row.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(5, 5, 5, 5);
            row.Axis = UILayoutConstraintAxis.Horizontal;
            row.Distribution = UIStackViewDistribution.EqualCentering;
            row.WidthAnchor.ConstraintEqualTo(350).Active = true;
            row.LayoutMarginsRelativeArrangement = true;
            return row;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _ = Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _clauseButton.TouchUpInside += ClauseClick;
            _dateSwitch.ValueChanged += DateSwitched;
            _startDateButton.Clicked += StartDateClick;
            _endDateButton.Clicked += EndDateClick;
            _applyButton.Clicked += ApplyClicked;
            _resetButton.Clicked += ResetClicked;
        }

        

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _clauseButton.TouchUpInside -= ClauseClick;
            _dateSwitch.ValueChanged -= DateSwitched;
            _startDateButton.Clicked -= StartDateClick;
            _endDateButton.Clicked -= EndDateClick;
            _applyButton.Clicked -= ApplyClicked;
            _resetButton.Clicked -= ResetClicked;
        }
    }
}