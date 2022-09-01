namespace ArcGISRuntimeMaui.Resources
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