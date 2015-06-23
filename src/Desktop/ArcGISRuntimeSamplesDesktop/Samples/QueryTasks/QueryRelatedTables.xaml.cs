using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates how to use a relationship query to display information from a related table for selected features. Clicking on the map highlight a set of wells near the clicked point. Select a well from the list to display related features (tops). The object id of the well is used in the relationship query to return 0 to many related records displayed in the list view. Note that the code requires a relationshipId. Feature layers can have more than one relationship and each relationship is identified by a unique identifier. You can use the Services Directory to find the relationship id.
	/// </summary>
	/// <title>Query Related Tables</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class QueryRelatedTables : UserControl
	{
		private GraphicsOverlay _wellsOverlay;
	 
		/// <summary>Construct Spatial Query sample control</summary>
		public QueryRelatedTables()
		{
			InitializeComponent();

			_wellsOverlay = MyMapView.GraphicsOverlays["wellsOverlay"];               
		}

		// Select a set of wells near the click point
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				_wellsOverlay.Graphics.Clear();
				wellsGrid.ItemsSource = relationshipsGrid.ItemsSource = null;

				QueryTask queryTask =
					new QueryTask(new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Petroleum/KSPetro/MapServer/0"));
				
				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				Query query = new Query("1=1")
				{
					Geometry = Expand(viewpointExtent, e.Location, 0.01),
					ReturnGeometry = true,
					OutSpatialReference = MyMapView.SpatialReference,
					OutFields = OutFields.All
				};

				var result = await queryTask.ExecuteAsync(query);
				if (result.FeatureSet.Features != null && result.FeatureSet.Features.Count > 0)
				{
					_wellsOverlay.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
					wellsGrid.ItemsSource = result.FeatureSet.Features;
					resultsPanel.Visibility = Visibility.Visible;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Query Related Tables");
			}
		}

		// Query for rows related to the selected well in the wells list view
		private async void WellsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

					RelationshipParameters parameters = new RelationshipParameters(new List<long>(objectIds), 3)
					{
						OutSpatialReference = MyMapView.SpatialReference
					};

					parameters.OutFields.AddRange(new string[] { "OBJECTID, API_NUMBER, ELEVATION, FORMATION, TOP" });

					var result = await queryTask.ExecuteRelationshipQueryAsync(parameters);
					relationshipsGrid.ItemsSource = result.RelatedRecordGroups.FirstOrDefault().Value;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Query Related Tables");
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
