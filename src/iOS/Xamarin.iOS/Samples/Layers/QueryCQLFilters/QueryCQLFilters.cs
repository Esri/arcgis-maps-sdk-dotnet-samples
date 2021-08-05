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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
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
        private UIDatePicker _startDatePicker;
        private UIDatePicker _endDatePicker;
        private UIToolbar _applyToolbar;

        private List<string> _whereClauses { get; } = new List<string>
        {
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

        private string _whereClause;

        public QueryCQLFilters()
        {
            Title = "Query with CQL filters";
        }

        private async Task Initialize()
        {
            try
            {
                // Populate the UI.
                _maxFeaturesEntry.Text = "3000";
                _startDatePicker.Date = (NSDate)new DateTime(2011, 6, 13).ToUniversalTime();
                _endDatePicker.Date = (NSDate)new DateTime(2012, 1, 7).ToUniversalTime();

                // Create the map with topographic basemap.
                _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

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
                DateTime startDate = (_startDatePicker.Date != null) ? (DateTime)_startDatePicker.Date : DateTime.MinValue;
                DateTime endDate = (_endDatePicker.Date != null) ? (DateTime)_endDatePicker.Date : new DateTime(9999, 12, 31);

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
            _startDatePicker.Date = (NSDate)new DateTime(2011, 6, 13).ToUniversalTime();
            _endDatePicker.Date = (NSDate)new DateTime(2012, 1, 7).ToUniversalTime();
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

        private void DateSwitched(object sender, EventArgs e)
        {
            _startDatePicker.Enabled = _endDatePicker.Enabled = _dateSwitch.On;
        }

        private void ClauseClick(object sender, EventArgs ev)
        {
            // Start the UI for the user choosing the clause.
            UIAlertController prompt = UIAlertController.Create(null, "Choose clause", UIAlertControllerStyle.ActionSheet);

            foreach (string clause in _whereClauses)
            {
                prompt.AddAction(UIAlertAction.Create(clause, UIAlertActionStyle.Default, ClauseSelected));
            }

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.SourceView = _applyToolbar;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            PresentViewController(prompt, true, null);
        }

        private void ClauseSelected(UIAlertAction obj)
        {
            _whereClause = obj.Title;
            _clauseButton.SetTitle(obj.Title, UIControlState.Normal);
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
            _maxFeaturesEntry = new UITextField { TranslatesAutoresizingMaskIntoConstraints = false, Text = "3000" };
            var maxFeaturesStack = GetRowStackView(new UIView[] { maxLabel, _maxFeaturesEntry });

            UILabel dateLabel = new UILabel { Text = "Time extent:", TranslatesAutoresizingMaskIntoConstraints = false };
            _dateSwitch = new UISwitch { TranslatesAutoresizingMaskIntoConstraints = false, On = true };
            var dateSwitchStack = GetRowStackView(new UIView[] { dateLabel, _dateSwitch });

            _startDatePicker = new UIDatePicker { PreferredDatePickerStyle = UIDatePickerStyle.Compact, Mode = UIDatePickerMode.Date };
            _endDatePicker = new UIDatePicker { PreferredDatePickerStyle = UIDatePickerStyle.Compact, Mode = UIDatePickerMode.Date };
            var datesStack = GetRowStackView(new UIView[] { _startDatePicker, _endDatePicker });

            _applyButton = new UIBarButtonItem { Title = "Apply" };
            _resetButton = new UIBarButtonItem { Title = "Reset" };

            _applyToolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };
            _applyToolbar.Items = new[]
            {
                _applyButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, _activityIndicator, dateSwitchStack, maxFeaturesStack, clauseStack, datesStack, _applyToolbar);

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
                dateSwitchStack.BottomAnchor.ConstraintEqualTo(datesStack.TopAnchor),
                dateSwitchStack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                dateSwitchStack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                datesStack.TopAnchor.ConstraintEqualTo(dateSwitchStack.BottomAnchor),
                datesStack.BottomAnchor.ConstraintEqualTo(_applyToolbar.TopAnchor),
                datesStack.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                datesStack.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                datesStack.HeightAnchor.ConstraintEqualTo(45),

                _applyToolbar.TopAnchor.ConstraintEqualTo(datesStack.BottomAnchor),
                _applyToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _applyToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _applyToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

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
            _applyButton.Clicked += ApplyClicked;
            _resetButton.Clicked += ResetClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _clauseButton.TouchUpInside -= ClauseClick;
            _dateSwitch.ValueChanged -= DateSwitched;
            _applyButton.Clicked -= ApplyClicked;
            _resetButton.Clicked -= ResetClicked;
        }
    }
}