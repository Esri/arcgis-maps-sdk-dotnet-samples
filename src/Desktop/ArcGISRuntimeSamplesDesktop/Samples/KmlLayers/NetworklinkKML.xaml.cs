using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using System.Windows.Media;
using System.Windows;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates how to display KML NetworkLink features and bind them to a TreeView.
	/// </summary>
	/// <title>NetworkLink with TreeView</title>
	/// <category>Layers</category>
	/// <subcategory>Kml Layers</subcategory>
	public partial class NetworkLinkKML : UserControl
	{
		public NetworkLinkKML()
		{
			InitializeComponent();
		}

		private async void MyMapView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
		{
			try
			{
				//Add kml layer to the treeView
				if (e.Layer is KmlLayer)
				{
					ObservableCollection<KmlFeature> kmlFeatureList = new ObservableCollection<KmlFeature>();
					kmlFeatureList.Add((e.Layer as KmlLayer).RootFeature);

					treeView.ItemsSource = kmlFeatureList;

					await MyMapView.SetViewAsync(new MapPoint(570546.04, 6867036.46), 1000000);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sample Error");
			}
		}
	}
}
