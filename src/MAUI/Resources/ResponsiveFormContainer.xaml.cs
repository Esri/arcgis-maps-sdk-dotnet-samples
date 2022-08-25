using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISRuntimeMaui.Resources
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ResponsiveFormContainer : Frame
	{
        public ResponsiveFormContainer(): base()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, Device.Idiom == TargetIdiom.Phone ? "Phone" : "DesktopTablet");
        }
	}
}