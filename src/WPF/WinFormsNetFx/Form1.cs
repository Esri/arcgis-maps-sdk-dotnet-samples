using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WinFormsNetFx
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
            InitializeComponent();
            var host = new ElementHost();
            host.Dock = DockStyle.Fill;
            host.Child = new ArcGIS.WPF.Samples.CreateAndEditGeometries.CreateAndEditGeometries();
            this.Controls.Add(host);
        }
    }
}
