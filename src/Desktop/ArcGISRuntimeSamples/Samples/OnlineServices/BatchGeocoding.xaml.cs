using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to perform batch geocoding using the ArcGIS Online World Geocoding service with an ArcGIS Online Organizational account.
    /// </summary>
    /// <title>Batch Geocoding</title>
    /// <category>ArcGIS Online Services</category>
    public partial class BatchGeocoding : UserControl
    {
        private const string GEOCODE_SERVICE_URL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/geocodeAddresses";

        public ObservableCollection<SourceAddress> SourceAddresses { get; set; }
		private GraphicsOverlay _graphicsOverlay;

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

				JavaScriptSerializer serializer = new JavaScriptSerializer();

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
				var results = serializer.Deserialize<Dictionary<string, object>>(jsonResults);

				var candidates = results["locations"] as ArrayList;
				foreach (var candidate in candidates.OfType<Dictionary<string, object>>())
				{
					var location = candidate["location"] as Dictionary<string, object>;
					MapPoint point = new MapPoint(Convert.ToDouble(location["x"]), Convert.ToDouble(location["y"]), MyMapView.SpatialReference);
					_graphicsOverlay.Graphics.Add(new Graphic(point));

					// Create a new templated overlay for the geocoded address
					var overlay = new ContentControl() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
					overlay.Template = layoutGrid.Resources["MapTipTemplate"] as ControlTemplate;
					overlay.DataContext = candidate["attributes"] as Dictionary<string, object>;
					MapView.SetViewOverlayAnchor(overlay, point);
					MyMapView.Overlays.Items.Add(overlay);
				}

				await MyMapView.SetViewAsync(GeometryEngine.Union(_graphicsOverlay.Graphics.Select(g => g.Geometry)).Extent.Expand(1.5));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sample Error");
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
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
}
