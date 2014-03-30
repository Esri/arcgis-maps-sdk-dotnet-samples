using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>
	public sealed partial class QueryRelatedRecords : PhoneApplicationPage
    {
        MapView m_mapView;
        public QueryRelatedRecords()
        {
            InitializeComponent();
        }

        // On tap, either get related records for the tapped well or find nearby wells if no well was tapped
        private async void mapView1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Get the map
            if (m_mapView == null)
                m_mapView = (MapView)sender;

            // Check whether any wells were tapped
            var selectedWells = await WellsLayer.HitTestAsync(m_mapView, e.GetPosition(m_mapView), 1);
            if (selectedWells != null && selectedWells.Count() > 0) // A well was tapped
            {
                #region UI logic - show selected graphic, update status text, clear results
                // Show the tapped graphic as selected
                var selectedGraphic = selectedWells.First();
                selectGraphic(selectedGraphic);

                updateStatus("Searching for related records...", true);

                ObjectIdColumn.ItemsSource = null;
                RelatedRecordsGrid.Visibility = Visibility.Collapsed;
                #endregion

                // Execute the query for related records
                var result = await doRelationshipQuery(Convert.ToInt32(selectedGraphic.Attributes["OBJECTID"]));

                #region Handle results - show related records, update status text
                if (result == null) // Error
                {
                    updateStatus("Error retrieving related records", false);
                }
                else if (result.RelatedRecordGroups.Count > 0)
                {
                    // Get related well tops
                    IReadOnlyList<Graphic> relatedWellTops = result.RelatedRecordGroups.First().Value;

                    // Show related records
                    ObjectIdColumn.ItemsSource = relatedWellTops;
                    RelatedRecordsGrid.Visibility = Visibility.Visible;

                    // Update status text with number of related records found
                    var statusText = relatedWellTops.Count.ToString();
                    if (relatedWellTops.Count > 1)
                        statusText += " related records found";
                    else
                        statusText += " related record found";
                    updateStatus(statusText, false);
                }
                else // None found
                {
                    updateStatus("No related records found", false);
                }
                #endregion
            }
            else // No wells were tapped - find nearby wells
            {
                #region Update UI - clear results and update status text
                clearResults();
                updateStatus("Searching for nearby wells...", true);
                #endregion

                // Convert tap point to map point and buffer 300 meters
                var mp = m_mapView.ScreenToLocation(e.GetPosition(m_mapView));
                var buffer = GeometryEngine.Buffer(mp, 300);

                // Query for wells intersecting the buffer
                var result = await doQuery(buffer);

                #region Handle results - show wells on map and update status text
                if (result == null) // Error
                {
                    updateStatus("Error finding nearby wells", false);
                }
                else if (result.FeatureSet != null
                    && result.FeatureSet.Features != null
                    && result.FeatureSet.Features.Count > 0)
                {
                    IReadOnlyList<Graphic> wells = result.FeatureSet.Features;

                    // Update status text based on number of wells found
                    var statusText = wells.Count.ToString();
                    if (wells.Count > 1)
                        statusText += " wells found.  Tap one to view related records.";
                    else
                        statusText += " well found.  Tap it to view related records.";
                    updateStatus(statusText, false);

                    // Show the wells on the map and zoom to them
                    WellsLayer.Graphics.AddRange(wells);
                    zoomToGraphicsLayer(WellsLayer);
                }
                else // no nearby wells found
                {
                    updateStatus("No wells found.  Tap the map to search again.", false);
                }
                #endregion
            }
        }

        #region Get Related Records
        // Query for well tops related to wells with the passed-in object ID
        private async Task<RelationshipResult> doRelationshipQuery(int oid)
        {

            QueryTask queryTask =
               new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0"));

            // Initialize relationship parameters
            RelationshipParameter parameters = new RelationshipParameter(new long[] { oid }, 3)
            {
                OutSpatialReference = m_mapView.SpatialReference,
                OutFields = new OutFields(new string[] { "OBJECTID, API_NUMBER, FORMATION, TOP" })
            };

            try
            {
                // Do the relationship query and return the result
                return await queryTask.ExecuteRelationshipQueryAsync(parameters);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Find Wells
        // Searches for wells that intersect the passed-in geometry
        private async Task<QueryResult> doQuery(Geometry geometry)
        {
            QueryTask queryTask =
                new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0"));

            // Construct query parameters using the passed-in geometry
            Query query = new Query(geometry)
            {
                ReturnGeometry = true,
                OutSpatialReference = m_mapView.SpatialReference,
                OutFields = new OutFields(new string[] { "OBJECTID" })
            };
            try
            {
                // Execute the query and return the result
                return await queryTask.ExecuteAsync(query);
            }
            catch            
            {
                return null;
            }
        }
        #endregion

        #region UI Utility Methods - updateStatus, clearResults, selectGraphic
        private void updateStatus(string statusText, bool busy)
        {
            StatusText.Text = statusText;
            ProgressBar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void clearResults()
        {
            SelectionLayer.Graphics.Clear();
            WellsLayer.Graphics.Clear();

            RelatedRecordsGrid.Visibility = Visibility.Collapsed;
            ObjectIdColumn.ItemsSource = null;
        }

        private void selectGraphic(Graphic g)
        {
            SelectionLayer.Graphics.Clear();
            SelectionLayer.Graphics.Add(g);
        }
        #endregion

        #region GraphicsLayer Utility Methods - zoom and get extent
        // Zooms to the passed-in graphics layer
        private void zoomToGraphicsLayer(GraphicsLayer layer)
        {
            if (layer.Graphics.Count == 1) // Just pan if only one
            {
                m_mapView.SetView((MapPoint)layer.Graphics.First().Geometry);
            }
            else if (layer.Graphics.Count > 1)
            {
                // Get envelope and zoom if more than one
                Envelope extent = getGraphicsLayerExtent(layer);
                if (extent != null)
                    m_mapView.SetView(extent.Expand(2));
            }
        }

        // Gets the extent of the passed-in graphics layer as an Envelope
        private Envelope getGraphicsLayerExtent(GraphicsLayer layer)
        {
            Envelope env = null;

            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;

            foreach (Graphic g in layer.Graphics)
            {
                if (!(g.Geometry is MapPoint))
                    continue;

                var point = (MapPoint)g.Geometry;
                if (point.X < xmin)
                    xmin = point.X;                
                if (point.X > xmax)
                    xmax = point.X;
                if (point.Y < ymin)
                    ymin = point.Y;                
                if (point.Y > ymax)
                    ymax = point.Y;
            }

            if (xmin < double.MaxValue && ymin < double.MaxValue
            && xmax > double.MinValue && ymax > double.MinValue)
                env = new Envelope(xmin, ymin, xmax, ymax);

            return env;
        }
        #endregion

        private GraphicsLayer SelectionLayer { get { return (GraphicsLayer)m_mapView.Map.Layers["SelectionLayer"]; } }

        private GraphicsLayer WellsLayer { get { return (GraphicsLayer)m_mapView.Map.Layers["WellsLayer"]; } }
    }
}