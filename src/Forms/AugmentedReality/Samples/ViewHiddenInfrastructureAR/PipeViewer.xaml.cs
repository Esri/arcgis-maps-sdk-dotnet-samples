using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    public partial class PipeViewer : ContentPage
    {
        // Pipe graphics that have been passed in by the PipePlacer class.
        public static IEnumerable<Graphic> PipeGraphics;
        public PipeViewer()
        {
            InitializeComponent();
        }
    }
}