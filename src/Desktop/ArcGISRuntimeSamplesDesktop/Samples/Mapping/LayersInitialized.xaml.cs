using System;
using System.ComponentModel;
using System.Windows;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates implementing logic which is dependent on the layer collection being initialized.
    /// </summary>
	/// <category>Layers</category>
	public partial class LayersInitialized : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region Layers Initialized Text Property

        private string _layersInitialized = "Not Initialized";

        public string LayersInitializedProperty
        {
            get
            {
                return _layersInitialized;
            }
            set
            {
                _layersInitialized = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LayersInitializedProperty"));
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LayersInitialized"/> class.
        /// </summary>
        public LayersInitialized()
        {
            DataContext = this;
            InitializeComponent();
            HandleLayersInitialized();
        }

        private async void HandleLayersInitialized()
        {
			try
			{
				var loadresult = await MyMapView.LayersLoadedAsync();
				LayersInitializedProperty = "Initialized!";
				foreach (var res in loadresult)
				{
					if (res.LoadError != null)
					{
						MessageBox.Show(string.Format("Layer {0} failed to load. {1} ", res.Layer.ID, res.LoadError.Message.ToString()));
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured : " + ex.ToString(), "Sample error");
			}
        }
    }
}
