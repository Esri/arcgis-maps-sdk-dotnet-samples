using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
            InitializeComponent();
            var host = new ElementHost();
            host.Dock = DockStyle.Fill;
            //host.Child = new ArcGIS.WPF.Samples.ShowCallout.ShowCallout();
            //host.Child = new ArcGIS.WPF.Samples.CreateAndEditGeometries.CreateAndEditGeometries();
            host.Child = new ArcGIS.WPF.Samples.GetElevationAtPoint.GetElevationAtPoint();
            this.Controls.Add(host);
        }
    }
}
