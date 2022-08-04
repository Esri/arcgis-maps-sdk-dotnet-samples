using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Forms.Resources
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResponsiveFormContainer : Frame
    {
        public ResponsiveFormContainer() : base()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, Device.Idiom == TargetIdiom.Phone ? "Phone" : "DesktopTablet");
        }
    }
}