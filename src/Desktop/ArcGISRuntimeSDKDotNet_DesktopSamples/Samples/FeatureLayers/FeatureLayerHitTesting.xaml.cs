using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstates feature layer hit testing.  When the user clicks on the map, the applicaiton uses FeatureLayer.HitTest to retrieve the feature at the mouse point.  The HitTest method returns the row ID of the first feature found at the given point.  This sample then uses the FeatureTable.QueryAsync method to retrieve a Feature for the HitTested row ID and desiplays its attributes in the UI.
    /// </summary>
    /// <title>Hit Testing</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerHitTesting : UserControl, INotifyPropertyChanged
    {
        /// <summary>PropertyChanged event for INotifyPropertyChanged</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private Feature _resultFeature;

        /// <summary>HitTest result feature</summary>
        public Feature ResultFeature 
        {
            get { return _resultFeature; }
            set
            {
                _resultFeature = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ResultFeature"));
            }
        }

        /// <summary>Construct FeatureLayerHitTesting sample control</summary>
        public FeatureLayerHitTesting()
        {
            InitializeComponent();

            DataContext = this;
        }

        /// <summary>
        /// On each mouse click:
        /// - HitTest the feature layer
        /// - Query the feature table for the returned row
        /// - Set the result feature for the UI
        /// </summary>
		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
				var rows = await cities.HitTestAsync(MyMapView, e.Position);
                if (rows != null && rows.Length > 0)
                {
                    var features = await cities.FeatureTable.QueryAsync(rows);
                    ResultFeature = features.FirstOrDefault();
                }
                else
                    ResultFeature = null;
            }
            catch (Exception ex)
            {
                ResultFeature = null;
                MessageBox.Show("HitTest Error: " + ex.Message, "Feature Layer Hit Testing Sample");
            }
        }
    }
}
