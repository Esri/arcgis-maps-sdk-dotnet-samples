using Esri.ArcGISRuntime.Layers;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Mapping</category>
	public partial class HandleErrors : PhoneApplicationPage
    {
        
        public HandleErrors()
        {
            InitializeComponent();

            // Initialize string builder for construction of initialization error message
            HandleLayersLoaded();
        }

        async Task HandleLayersLoaded()
        {
            var loadresult = await mapView1.LayersLoadedAsync();
            foreach (var res in loadresult)
            {
                if (res.LoadError != null)
                {
                    MessageBox.Show(string.Format("Layer {0} failed to load. {1} ", res.Layer.ID, res.LoadError.Message.ToString()));
                    
                }
            }
        }

     
    }
}