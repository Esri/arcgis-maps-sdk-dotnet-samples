using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how you can show popups for KmlPlacemark when you click on it.
    /// </summary>
    /// <title>KML Popup</title>
    /// <category>Layers</category>
    /// <subcategory>Kml Layers</subcategory>
    public partial class KMLPopups : UserControl
    {
        public KMLPopups()
        {
            InitializeComponent();
        }

        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
            MyMapView.Overlays.Items.Clear();

            IEnumerable<KmlFeature> features = await (MyMapView.Map.Layers["kmlLayer"] as KmlLayer).HitTestAsync(MyMapView, e.Position);

            if (features.Count() > 0)
            {
                if (!string.IsNullOrWhiteSpace(features.FirstOrDefault().BalloonStyle.FormattedText))
                {
                    //Create WebBrowser to show the formatted text
                    var browser = new System.Windows.Controls.WebBrowser();
                    browser.NavigateToString(features.FirstOrDefault().BalloonStyle.FormattedText);

                    //Get the KmlPlacemark position
                    var featurePosition = (features.FirstOrDefault() as KmlPlacemark).Extent;
                    
                    //Create ContentControl
                    var cControl = new ContentControl() 
                    {
                        Content = browser,
                        MaxHeight = 500,
                        MaxWidth = 450
                    };

                    //Add the ContentControl to MapView.Overlays
                    MapView.SetViewOverlayAnchor(cControl, featurePosition.GetCenter());
                    MyMapView.Overlays.Items.Add(cControl);
                }
            }
          
        }
    }
}
