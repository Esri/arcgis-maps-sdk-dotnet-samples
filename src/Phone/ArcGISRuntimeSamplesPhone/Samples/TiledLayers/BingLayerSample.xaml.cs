using Esri.ArcGISRuntime.Layers;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples.TiledLayers
{
	/// <summary>
	/// This sample demonstrates adding a Bing Maps layer to a Map in code.
	/// A Bing Maps key must be provided to view Bing Maps tiled map services.
	/// </summary>
	/// <title>Bing Maps Layer</title>
	/// <category>Tiled Layers</category>
	public sealed partial class BingLayerSample : Page
	{
		public BingLayerSample()
		{
			InitializeComponent();
		}

		private void RadioButton_Click(object sender, RoutedEventArgs e)
		{
			string layerNameTag = (string)((RadioButton)sender).Tag;

			foreach (Layer layer in MyMap.Layers)
				if (layer is BingLayer)
					layer.IsVisible = false;

			MyMap.Layers[layerNameTag].IsVisible = true;
		}

		private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if ((sender as TextBox).Text.Length >= 64)
				LoadMapButton.IsEnabled = true;
			else
				LoadMapButton.IsEnabled = false;
		}

		private async void LoadMapButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

				HttpClient http = new System.Net.Http.HttpClient();
				HttpResponseMessage response = await http.GetAsync(uri);
				var bingAuthStream = await response.Content.ReadAsStreamAsync();

				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BingAuthentication));
				BingAuthentication bingAuthentication = serializer.ReadObject(bingAuthStream) as BingAuthentication;
				string authenticationResult = bingAuthentication.AuthenticationResultCode.ToString();
				if (authenticationResult == "ValidCredentials")
				{
					foreach (BingLayer.LayerType layerType in (BingLayer.LayerType[])Enum.GetValues(typeof(BingLayer.LayerType)))
					{
						BingLayer bingLayer = new BingLayer()
						{
							ID = layerType.ToString(),
							MapStyle = layerType,
							Key = BingKeyTextBox.Text,
							IsVisible = false
						};

						MyMap.Layers.Add(bingLayer);
					}

					MyMap.Layers[0].IsVisible = true;

					InvalidBingKeyTextBlock.Visibility = Visibility.Collapsed;
					LayerStyleGrid.Visibility = Visibility.Visible;
					InfoText.Visibility = Visibility.Collapsed;
				}
				else
					InvalidBingKeyTextBlock.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				InvalidBingKeyTextBlock.Visibility = Visibility.Visible;
			}
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Launcher.LaunchUriAsync(new Uri("https://www.bingmapsportal.com"));
		}

		[DataContract]
		public class BingAuthentication
		{
			[DataMember(Name = "authenticationResultCode")]
			public string AuthenticationResultCode { get; set; }
		}
	}
}
