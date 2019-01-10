// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ListRelatedFeatures
{
    [Register("ListRelatedFeatures")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List related features",
        "Data",
        "This sample demonstrates how to query features related to an identified feature.",
        "Click on a feature to identify it. Related features will be listed in the window above the map.")]
    public class ListRelatedFeatures : UIViewController
    {
        // Create and hold references to the UI controls.
        private MapView _myMapView;
        private UITableView _tableView;

        // URL to the web map.
        private readonly Uri _mapUri =
            new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=dcc7466a91294c0ab8f7a094430ab437");

        // Reference to the feature layer.
        private FeatureLayer _myFeatureLayer;

        // Hold a source for the UITableView that shows the related features.
        private LayerListSource _layerListSource;

        public ListRelatedFeatures()
        {
            Title = "List related features";
        }

        private async void Initialize()
        {
            try
            {
                // Create the portal item from the URL to the webmap.
                PortalItem alaskaPortalItem = await PortalItem.CreateAsync(_mapUri);

                // Create the map from the portal item.
                Map myMap = new Map(alaskaPortalItem);

                // Add the map to the mapview.
                _myMapView.Map = myMap;

                // Wait for the map to load.
                await myMap.LoadAsync();

                // Get the feature layer from the map.
                _myFeatureLayer = (FeatureLayer) myMap.OperationalLayers.First();

                // Update the selection color.
                _myMapView.SelectionProperties.Color = Color.Yellow;

                // Listen for GeoViewTapped events.
                _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing feature selection and results list.
            _myFeatureLayer.ClearSelection();
            _tableView.Source = null;
            _tableView.ReloadData();

            try
            {
                // Identify the tapped feature.
                IdentifyLayerResult results = await _myMapView.IdentifyLayerAsync(_myFeatureLayer, e.Position, 10, false);

                // Return if there are no results.
                if (results.GeoElements.Count < 1)
                {
                    return;
                }

                // Get the first result.
                ArcGISFeature myFeature = (ArcGISFeature) results.GeoElements.First();

                // Select the feature.
                _myFeatureLayer.SelectFeature(myFeature);

                // Get the feature table for the feature.
                ArcGISFeatureTable myFeatureTable = (ArcGISFeatureTable) myFeature.FeatureTable;

                // Query related features.
                IReadOnlyList<RelatedFeatureQueryResult> relatedFeaturesResult = await myFeatureTable.QueryRelatedFeaturesAsync(myFeature);

                // Create a list to hold the formatted results of the query.
                List<string> queryResultsForUi = new List<string>();

                // For each query result.
                foreach (RelatedFeatureQueryResult result in relatedFeaturesResult)
                {
                    // And then for each feature in the result.
                    foreach (Feature resultFeature in result)
                    {
                        // Get a reference to the feature's table.
                        ArcGISFeatureTable relatedTable = (ArcGISFeatureTable) resultFeature.FeatureTable;

                        // Get the display field name - this is the name of the field that is intended for display.
                        string displayFieldName = relatedTable.LayerInfo.DisplayFieldName;

                        // Get the name of the feature's table.
                        string tableName = relatedTable.TableName;

                        // Get the display name for the feature.
                        string featureDisplayName = resultFeature.Attributes[displayFieldName].ToString();

                        // Create a formatted result string.
                        string formattedResult = $"{tableName} - {featureDisplayName}";

                        // Add the result to the list.
                        queryResultsForUi.Add(formattedResult);
                    }
                }

                // Create the source for the display list.
                _layerListSource = new LayerListSource(queryResultsForUi);

                // Assign the source to the display view.
                _tableView.Source = _layerListSource;

                // Force the table view to refresh its data.
                _tableView.ReloadData();
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }


        public override void LoadView()
        {
            _myMapView = new MapView();
            _tableView = new UITableView();
            _tableView.RowHeight = 30;

            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            _tableView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView, _tableView);
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Reset constraints.
            _tableView.RemoveFromSuperview();
            _myMapView.RemoveFromSuperview();
            View.AddSubviews(_myMapView, _tableView);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                applyLandscapeLayout();
            }
            else
            {
                applyPortraitLayout();
            }
        }

        private void applyPortraitLayout()
        {
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _tableView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _tableView.HeightAnchor.ConstraintEqualTo(120).Active = true;

            _myMapView.TopAnchor.ConstraintEqualTo(_tableView.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        private void applyLandscapeLayout()
        {
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.WidthAnchor.ConstraintEqualTo(View.Frame.Height).Active = true;

            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(_tableView.TrailingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }

    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the UI for the list of related features.
    /// </summary>
    public class LayerListSource : UITableViewSource
    {
        private readonly List<string> _viewModelList = new List<string>();

        // Used when re-using cells to ensure that a cell of the right type is used.
        private const string CellId = "TableCell";

        public LayerListSource(List<string> items)
        {
            // Set the items.
            if (items != null)
            {
                _viewModelList = items;
            }
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index.
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance). If there are no cells, create a new one.
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
            }

            // Set the text on the cell.
            cell.TextLabel.Text = _viewModelList[indexPath.Row];

            // Ensure that the label fits.
            cell.TextLabel.AdjustsFontSizeToFitWidth = true;

            // Return the cell.
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _viewModelList.Count;
        }
    }
}