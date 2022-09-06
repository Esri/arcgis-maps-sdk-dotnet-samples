namespace ArcGISRuntimeMaui.Resources
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResponsiveFormContainer : Frame
    {
        public ResponsiveFormContainer() : base()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, DeviceInfo.Idiom == DeviceIdiom.Phone ? "Phone" : "DesktopTablet");
        }
    }
}