using Esri.ArcGISRuntime.Layers;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Mapping</category>
	public sealed partial class HandleErrors : Page
    {
        public HandleErrors()
        {
            this.InitializeComponent();
            var _ = HandleLayersLoaded();
        }

        private async Task HandleLayersLoaded()
        {
            var loadresult = await mapView1.LayersLoadedAsync();
            foreach (var res in loadresult)
            {
                if (res.LoadError != null)
                {
                    var msg = new MessageDialog(string.Format("Layer {0} failed to load. {1} ", res.Layer.ID, res.LoadError.Message.ToString()));
                    await msg.ShowAsync();
                }
            }
        }
    }
}
