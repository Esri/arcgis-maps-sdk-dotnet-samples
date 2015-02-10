using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample uses a geoprocessing task to estimate the number of people within a polygon you draw on the map. Click 'Summarize Population', then freehand a line on the map surrounding an area to analyze. When you release the mouse button, the polygon will auto-complete and you'll see how many people are estimated to live within the polygon boundaries.
	/// </summary>
	/// <title>Population for an Area</title>
	/// <category>Geoprocessing Tasks</category>
	public partial class PopulationForArea : Windows.UI.Xaml.Controls.Page
	{
		private const string PopulationSummaryServiceUrl =
			"http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Population_World/GPServer/PopulationSummary";

		private GraphicsOverlay _areaOverlay;

		/// <summary>Construct Population for Area sample control</summary>
		public PopulationForArea()
		{
			InitializeComponent();

			_areaOverlay = MyMapView.GraphicsOverlays["areaOverlay"];
		}

		// Accept user boundary line and run the Geoprocessing Task to summarize population
		private async void SummarizePopulationButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				txtResult.Visibility = Visibility.Collapsed;
				_areaOverlay.Graphics.Clear();

				var boundary = await MyMapView.Editor.RequestShapeAsync(DrawShape.Freehand) as Polyline;
				var polygon = new Polygon(boundary.Parts, MyMapView.SpatialReference);
				polygon = GeometryEngine.Simplify(polygon) as Polygon;
				_areaOverlay.Graphics.Add(new Graphic() { Geometry = polygon });

				progress.Visibility = Visibility.Visible;

				Geoprocessor geoprocessorTask = new Geoprocessor(new Uri(PopulationSummaryServiceUrl));

				var parameter = new GPInputParameter() { OutSpatialReference = MyMapView.SpatialReference };
				parameter.GPParameters.Add(new GPFeatureRecordSetLayer("inputPoly", polygon));

				var result = await geoprocessorTask.ExecuteAsync(parameter);

				GPRecordSet rs = result.OutParameters.OfType<GPRecordSet>().FirstOrDefault();
				if (rs != null && rs.FeatureSet.Features != null)
				{
					int population = Convert.ToInt32(rs.FeatureSet.Features[0].Attributes["SUM"]);
					txtResult.Visibility = Visibility.Visible;
					txtResult.Text = string.Format("Area Population: {0:N0}", population);
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}
	}
}
