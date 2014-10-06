using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geoprocessing Tasks</category>
	public sealed partial class MessageInABottle : Page
    {
        public MessageInABottle()
        {
            InitializeComponent();
            InitializePMS();
        }

        private async void InitializePMS()
        {
            try
            {
                var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/i_pushpin.png"));
                var imageSource = await imageFile.OpenReadAsync();
                var pms = LayoutRoot.Resources["StartMarkerSymbol"] as PictureMarkerSymbol;
                await pms.SetSourceAsync(imageSource);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

       private async void mapView1_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {            
            var mapPoint = e.Location;
            var l = mapView1.Map.Layers["InputLayer"] as GraphicsLayer;
            l.Graphics.Clear();
            l.Graphics.Add(new Graphic() { Geometry = mapPoint });
            string error = null;
            Geoprocessor geoprocessorTask = new Geoprocessor(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_Currents_World/GPServer/MessageInABottle"));

            var parameter = new GPInputParameter()
            {
                OutSpatialReference = mapView1.SpatialReference
            };
            var toGeographic = GeometryEngine.Project(mapPoint, new SpatialReference(4326)) as MapPoint;

            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Point", toGeographic));
            parameter.GPParameters.Add(new GPDouble("Days", Convert.ToDouble(DaysTextBox.Text)));

            try
            {
                var result = await geoprocessorTask.ExecuteAsync(parameter);
                var r = mapView1.Map.Layers["ResultLayer"] as GraphicsLayer;
                r.Graphics.Clear();
                foreach (GPParameter gpParameter in result.OutParameters)
                {
                    if (gpParameter is GPFeatureRecordSetLayer)
                    {
                        GPFeatureRecordSetLayer gpLayer = gpParameter as GPFeatureRecordSetLayer;
                        r.Graphics.AddRange(gpLayer.FeatureSet.Features.OfType<Graphic>());
                    }
                }
            }
            catch (Exception ex)
            {
                error = "Geoprocessor service failed: " + ex.Message;
            }
            if (error != null)
                await new MessageDialog(error).ShowAsync();
        }

        
    }
}
