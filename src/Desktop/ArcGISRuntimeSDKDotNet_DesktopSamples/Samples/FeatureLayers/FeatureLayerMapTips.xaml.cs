using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample shows how to display a map tip for a feature layer.  In this example, the MouseMove event of the MapView is handled with code that performs a FeatureLayer HitTest / Query combination which returns a single feature for display in the mapTip element defined in the XAML.
    /// </summary>
    /// <title>Map Tips</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerMapTips : UserControl
    {
        /// <summary>Construct Map Tips sample</summary>
        public FeatureLayerMapTips()
        {
            InitializeComponent();
        }

        private async void mapView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                System.Windows.Point screenPoint = e.GetPosition(mapView);
                var rows = await earthquakes.HitTestAsync(mapView, screenPoint);
                if (rows != null && rows.Length > 0)
                {
                    var features = await earthquakes.FeatureTable.QueryAsync(rows);
                    var feature = features.FirstOrDefault();

                    maptipTransform.X = screenPoint.X + 4;
                    maptipTransform.Y = screenPoint.Y - mapTip.ActualHeight;
                    mapTip.DataContext = feature;
                    mapTip.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    mapTip.Visibility = System.Windows.Visibility.Hidden;
            }
            catch
            {
                mapTip.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
