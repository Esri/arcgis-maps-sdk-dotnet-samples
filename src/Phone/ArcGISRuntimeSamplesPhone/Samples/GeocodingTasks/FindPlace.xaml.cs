using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Linq;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to use the FindAsync method of an OnlineLocatorTask object to find places by name.
	/// </summary>
	/// <title>Find a Place</title>
	/// <category>Geocode Tasks</category>
	public sealed partial class FindPlace : Page
	{
		private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

		private GraphicsOverlay _addressOverlay;
		private OnlineLocatorTask _locatorTask;

		/// <summary>Construct find place sample control</summary>
		public FindPlace()
		{
			InitializeComponent();

			_addressOverlay = MyMapView.GraphicsOverlays[0]; ;

			_locatorTask = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
			_locatorTask.AutoNormalize = true;

			listResults.ItemsSource = _addressOverlay.Graphics;

			SetSimpleRendererSymbols();
		}

		// Setup the pin graphic and graphics overlay renderer
		private async void SetSimpleRendererSymbols()
		{
			try
			{
				var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
				await markerSymbol.SetSourceAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesPhone/Assets/RedStickpin.png"));
				var renderer = new SimpleRenderer() { Symbol = markerSymbol };

				_addressOverlay.Renderer = renderer;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Selection Error: " + ex.Message, "Find Place Sample").ShowAsync();
			}
		}

		// Find matching places, create graphics and add them to the UI
		private async void FindButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				progress.Visibility = Visibility.Visible;
				listResults.Visibility = Visibility.Collapsed;
				_addressOverlay.Graphics.Clear();

				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				var param = new OnlineLocatorFindParameters(SearchTextBox.Text)
				{

					SearchExtent = viewpointExtent,
					Location = viewpointExtent.GetCenter(),
					MaxLocations = 5,
					OutSpatialReference = MyMapView.SpatialReference,
					OutFields = new string[] { "Place_addr" }
				};

				var candidateResults = await _locatorTask.FindAsync(param, CancellationToken.None);

				if (candidateResults == null || candidateResults.Count == 0)
					throw new Exception("No candidates found in the current map extent.");

				foreach (var candidate in candidateResults)
					AddGraphicFromLocatorCandidate(candidate);

				var extent = GeometryEngine.Union(_addressOverlay.Graphics.Select(g => g.Geometry)).Extent.Expand(1.1);
				await MyMapView.SetViewAsync(extent);

				listResults.Visibility = Visibility.Visible;
			}
			catch (AggregateException ex)
			{
				var innermostExceptions = ex.Flatten().InnerExceptions;
				if (innermostExceptions != null && innermostExceptions.Count > 0)
				{
					var _x = new MessageDialog(string.Join(" > ", innermostExceptions.Select(i => i.Message).ToArray()), "Sample Error").ShowAsync();
				}
				else
				{
					var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
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

		private void AddGraphicFromLocatorCandidate(LocatorFindResult candidate)
		{
			var location = GeometryEngine.Project(candidate.Feature.Geometry, SpatialReferences.Wgs84) as MapPoint;

			var graphic = new Graphic(location);
			graphic.Attributes["Name"] = candidate.Name;
			graphic.Attributes["Address"] = candidate.Feature.Attributes["Place_addr"];
			graphic.Attributes["LocationDisplay"] = string.Format("{0:0.000}, {1:0.000}", location.X, location.Y);

			_addressOverlay.Graphics.Add(graphic);
		}

		private void listResults_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
		{
			_addressOverlay.ClearSelection();

			if (e.AddedItems != null)
			{
				foreach (var graphic in e.AddedItems.OfType<Graphic>())
				{
					graphic.IsSelected = true;
				}
			}
		}
	}
}
