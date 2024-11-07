using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WindowsFormsNetFx
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "<YOUR_KEY>";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
            InitializeComponent();
            var host = new ElementHost();
            host.Dock = DockStyle.Fill;
            host.Child = new ArcGIS.WPF.Samples.ShowCallout.ShowCallout();
            //host.Child = new ArcGIS.WPF.Samples.CreateAndEditGeometries.CreateAndEditGeometries();
            //host.Child = new ArcGIS.WPF.Samples.GetElevationAtPoint.GetElevationAtPoint();
            this.Controls.Add(host);
        }
    }
}
