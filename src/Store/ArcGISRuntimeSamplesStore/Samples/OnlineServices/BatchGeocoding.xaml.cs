using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;


namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// This sample shows how to perform batch geocoding using the ArcGIS Online World Geocoding service with an ArcGIS Online Organizational account.
	/// </summary>
	/// <title>Batch Geocoding</title>
	/// <category>ArcGIS Online Services</category>
	public sealed partial class BatchGeocoding : Page
	{
		private const string GEOCODE_SERVICE_URL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/geocodeAddresses";

		private GraphicsOverlay _graphicsOverlay;

		public ObservableCollection<SourceAddress> SourceAddresses { get; set; }
		
		public BatchGeocoding()
		{
			InitializeComponent();

			// Security Setup
			IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorizeHandler();
			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(PortalSecurity.Challenge);

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

			// Allow 5 source addresses by default
			SourceAddresses = new ObservableCollection<SourceAddress>();
			SourceAddresses.Add(new SourceAddress() { Address = "380 New York St., Redlands, CA, 92373" });
			SourceAddresses.Add(new SourceAddress() { Address = "1 World Way, Los Angeles, CA, 90045" });
			SourceAddresses.Add(new SourceAddress() { Address = string.Empty });
			SourceAddresses.Add(new SourceAddress() { Address = string.Empty });
			SourceAddresses.Add(new SourceAddress() { Address = string.Empty });
			listAddresses.ItemsSource = SourceAddresses;
		}

		// Batch Geocode
		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				progress.Visibility = Visibility.Visible;
				_graphicsOverlay.Graphics.Clear();
				MyMapView.Overlays.Items.Clear();

				string records = string.Join(",", SourceAddresses.Where(s => !string.IsNullOrWhiteSpace(s.Address))
					.Select((s, idx) => string.Format("{{ \"attributes\": {{ \"OBJECTID\": {0}, \"SingleLine\": \"{1}\" }} }}", idx, s.Address))
					.ToArray());
				string addresses = string.Format("{{ \"records\": [ {0} ] }}", records);

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["f"] = "json";
				parameters["outSR"] = MyMapView.SpatialReference.Wkid.ToString();
				parameters["addresses"] = addresses;

				ArcGISHttpClient httpClient = new ArcGISHttpClient();
				var response = await httpClient.GetOrPostAsync(GEOCODE_SERVICE_URL, parameters);

				var jsonResults = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

				var mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults));

				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GeocodeResults));
				var results = serializer.ReadObject(mStream) as GeocodeResults;

				foreach (var candidate in results.locations)
				{
					var location = candidate.location;
					MapPoint point = new MapPoint(Convert.ToDouble(location.x), Convert.ToDouble(location.y), MyMapView.SpatialReference);
					_graphicsOverlay.Graphics.Add(new Graphic(point));

					// Create a new templated overlay for the geocoded address
					var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
					overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
					overlay.DataContext = candidate.attributes;
					MapView.SetViewOverlayAnchor(overlay, point);
					MyMapView.Overlays.Items.Add(overlay);
				}

				await MyMapView.SetViewAsync(GeometryEngine.Union(_graphicsOverlay.Graphics.Select(g => g.Geometry)).Extent.Expand(1.5));
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

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			Uri targetUri = new Uri(@"https://developers.arcgis.com/en/features/geocoding/");
			MyWebView.Navigate(targetUri);
		}
	}

	/// <summary>Source Address class with change notification for UI</summary>
	public class SourceAddress : INotifyPropertyChanged
	{
		private string _address;
		public string Address
		{
			get { return _address; }
			set { _address = value; RaisePropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[DataContract]
	public class GeocodeResults
	{
		[DataMember(Name = "locations")]
		public List<Locations> locations { get; set; }

	}

	[DataContract]
	public class Locations
	{
		[DataMember]
		public string address { get; set; }

		[DataMember]
		public Location location { get; set; }

		[DataMember]
		public int score { get; set; }

		[DataMember]
		public Attributes attributes { get; set; }
	}

	[DataContract]
	public class Location
	{
		[DataMember]
		public double x { get; set; }

		[DataMember]
		public double y { get; set; }
	}

	[DataContract]
	public class Attributes
	{
		[DataMember]
		public string Match_addr { get; set; }

		[DataMember]
		public string City { get; set; }

		[DataMember]
		public string Region { get; set; }

		[DataMember]
		public string Postal { get; set; }

		[DataMember]
		public string X { get; set; }

		[DataMember]
		public string Y { get; set; }
	}

	public class StringFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			value = System.Convert.ToDecimal(value);

			if (value == null)
				return null;
 
			if (parameter == null)
				return value;
 
			return string.Format((string)parameter, value);
		}
 
		public object ConvertBack(object value, Type targetType, object parameter, 
			string language)
		{
			throw new NotImplementedException();
		}
	}
}



