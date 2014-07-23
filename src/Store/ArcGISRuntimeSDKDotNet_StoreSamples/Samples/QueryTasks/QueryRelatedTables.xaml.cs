using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to use a relationship query to display information from a related table for selected features.
    /// </summary>
    /// <title>Query Related Tables</title>
    /// <category>Query Tasks</category>
	public sealed partial class QueryRelatedTables : Windows.UI.Xaml.Controls.Page
    {
        private  GraphicsLayer _wellsLayer;

        public QueryRelatedTables()
        {
            this.InitializeComponent();

            _wellsLayer = mapView.Map.Layers["WellsLayer"] as GraphicsLayer;
                
            mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-10854000, 4502000, -10829000, 4524000, SpatialReferences.WebMercator));
        }

        // Select a set of wells near the click point
        private async void mapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                _wellsLayer.Graphics.Clear();
                wellsGrid.ItemsSource = relationshipsGrid.ItemsSource = null;

                QueryTask queryTask =
                    new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0"));

                Query query = new Query("1=1")
                {
                    Geometry = Expand(mapView.Extent, e.Location, 0.01),
                    ReturnGeometry = true,
                    OutSpatialReference = mapView.SpatialReference,
                    OutFields = OutFields.All
                };

                var result = await queryTask.ExecuteAsync(query);
                if (result.FeatureSet.Features != null && result.FeatureSet.Features.Count > 0)
                {
                    _wellsLayer.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
                    wellsGrid.ItemsSource = result.FeatureSet.Features;
                    resultsPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Query for rows related to the selected well in the wells list view
        private async void WellsGrid_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    QueryTask queryTask =
                       new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0"));

                    //Relationship query
                    var objectIds = e.AddedItems.OfType<Graphic>()
                        .Select(g => Convert.ToInt64(g.Attributes["OBJECTID"]));

                    RelationshipParameter parameters = new RelationshipParameter(new List<long>(objectIds), 3)
                    {
                        OutSpatialReference = mapView.SpatialReference
                    };

                    parameters.OutFields.AddRange(new string[] { "OBJECTID, API_NUMBER, ELEVATION, FORMATION, TOP" });

                    var result = await queryTask.ExecuteRelationshipQueryAsync(parameters);
                    relationshipsGrid.ItemsSource = result.RelatedRecordGroups.FirstOrDefault().Value;
                }
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private Envelope Expand(Envelope mapExtent, MapPoint point, double pct)
        {
            return new Envelope(
                point.X - mapExtent.Width * (pct / 2), point.Y - mapExtent.Height * (pct / 2),
                point.X + mapExtent.Width * (pct / 2), point.Y + mapExtent.Height * (pct / 2),
				mapExtent.SpatialReference);
        }
    }
}
