using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Demonstrates how to geocode an address using the OnlineLocatorTask.
	/// </summary>
	/// <title>Geocode an Address</title>
	/// <category>Geocode Tasks</category>
	public sealed partial class GeocodeAddress : Page
	{
		private const string OnlineLocatorUrl = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";

		private GraphicsOverlay _addressOverlay;
		private LocatorServiceInfo _locatorServiceInfo;
		private OnlineLocatorTask _locatorTask;
		private Graphic MapTipGraphic = null;

		public GeocodeAddress()
		{
			InitializeComponent();
			_addressOverlay = MyMapView.GraphicsOverlays[0];
			_locatorTask = new OnlineLocatorTask(new Uri(OnlineLocatorUrl));
			_locatorTask.AutoNormalize = true;

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

		private async void GeocodeButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				progress.Visibility = Visibility.Visible;
				_addressOverlay.Graphics.Clear();

				if (_locatorServiceInfo == null)
					_locatorServiceInfo = await _locatorTask.GetInfoAsync();

				var candidateResults = await _locatorTask.GeocodeAsync(
					GetInputAddressFromUI(), new List<string> { "Addr_type", "Score", "X", "Y" }, MyMapView.SpatialReference, CancellationToken.None);

				if (candidateResults == null || candidateResults.Count == 0)
					throw new Exception("No candidates found.");

				foreach (var candidate in candidateResults)
					AddGraphicFromLocatorCandidate(candidate);

				var extent = GeometryEngine.Union(_addressOverlay.Graphics.Select(g => g.Geometry)).Extent.Expand(1.1);
				await MyMapView.SetViewAsync(extent);
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
			catch (System.Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}
		private Dictionary<string, string> GetInputAddressFromUI()
		{
			Dictionary<string, string> address = new Dictionary<string, string>();

			if (btnSingleLine.IsChecked == true)
			{
				address[_locatorServiceInfo.SingleLineAddressField.FieldName] = txtSingleLine.Text;
			}
			else
			{
				if (!string.IsNullOrEmpty(InputAddress.Text))
				{
					string fieldName = "Address";
					address.Add(fieldName, InputAddress.Text);
				}
				if (!string.IsNullOrEmpty(City.Text))
				{
					string fieldName = "City";
					address.Add(fieldName, City.Text);
				}
				if (!string.IsNullOrEmpty(State.Text))
				{
					string fieldName = "Region";
					address.Add(fieldName, State.Text);
				}
				if (!string.IsNullOrEmpty(Zip.Text))
				{
					string fieldName = "Postal";
					address.Add(fieldName, Zip.Text);
				}
			}

			return address;
		}

		private void AddGraphicFromLocatorCandidate(LocatorGeocodeResult candidate)
		{
			var graphic = new Graphic(new MapPoint(candidate.Location.X, candidate.Location.Y, MyMapView.SpatialReference));
			graphic.Attributes["Address"] = candidate.Address;
			graphic.Attributes["Score"] = candidate.Score;
			graphic.Attributes["MatchType"] = candidate.Attributes["Addr_type"];

			double x = 0.0, y = 0.0;
			double.TryParse(candidate.Attributes["X"], out x);
			double.TryParse(candidate.Attributes["Y"], out y);
			graphic.Attributes["LocationDisplay"] = string.Format("{0:0.000}, {1:0.000}", x, y);

			graphic.IsVisible = (candidate.Score >= 90);
			_addressOverlay.Graphics.Add(graphic);
		}

		private void ZoomToExtent()
		{
			Envelope extent = null;
			foreach (Graphic g in _addressOverlay.Graphics)
			{
				Envelope tempEnv = GetDisplayExtent(g.Geometry as MapPoint, MyMapView.ActualHeight, MyMapView.ActualWidth);
				if (extent == null)
					extent = tempEnv;
				else
					extent = extent.Union(GetDisplayExtent(g.Geometry as MapPoint, MyMapView.ActualHeight, MyMapView.ActualWidth));
			}
			if (extent != null)
				MyMapView.SetView(extent);

		}

		private Envelope GetDisplayExtent(MapPoint point, double mapHeight, double mapWidth)
		{
			double halfWidth = 0.29858214173896908 * mapWidth / 2;
			double halfHeight = 0.29858214173896908 * mapHeight / 2;
			Envelope newExtent = new Envelope(point.X - halfWidth, point.Y - halfHeight,
				point.X + halfWidth, point.Y + halfHeight);
			return newExtent;

		}

		private void RenderMapTip()
		{
			MapPoint anchor = MapTipGraphic.Geometry as MapPoint;
			if (MyMapView.SpatialReference != null)
			{
				if (MapTipGraphic != null)
				{
					maptip.DataContext = MapTipGraphic.Attributes;
				}
				//Convert anchor point to the spatial reference of the map
				var mp = GeometryEngine.Project(anchor, MyMapView.SpatialReference) as MapPoint;
				//Convert anchor point to screen MapPoint
				var screen = MyMapView.LocationToScreen(mp);

				if (screen.X >= 0 && screen.Y >= 0 &&
					screen.X < MyMapView.ActualWidth && screen.Y < MyMapView.ActualHeight)
				{
					//Update location of map
					MapTipTranslate.X = screen.X;
					MapTipTranslate.Y = screen.Y - maptip.ActualHeight;
					maptip.Visibility = Windows.UI.Xaml.Visibility.Visible;
				}
				else //Anchor is outside the display so close map tip
				{
					maptip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				}
			}
		}

		private void maptip_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			RenderMapTip();
		}

		private async void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			Graphic hitGraphic = await _addressOverlay.HitTestAsync(MyMapView, e.Position);
			if (hitGraphic != null)
			{
				if (maptip.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
				{
					MapTipGraphic = hitGraphic;
					RenderMapTip();
				}
				else
				{
					maptip.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
					maptip.DataContext = null;
					MapTipGraphic = null;
				}
			}
		}
	}
}
