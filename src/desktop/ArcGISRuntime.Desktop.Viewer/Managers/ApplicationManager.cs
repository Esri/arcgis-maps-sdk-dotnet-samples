using ArcGISRuntime.Samples.Models;

namespace ArcGISRuntime.Desktop.Viewer.Managers
{
    public class ApplicationManager
    {
        private string _applicationLocation;

        #region Constructor and unique instance management

        // Private constructor
        private ApplicationManager() { }

        // Static initialization of the unique instance 
        private static readonly ApplicationManager SingleInstance = new ApplicationManager();

        /// <summary>
        /// Gets the single <see cref="MapViewController"/> instance.
        /// </summary>
        public static ApplicationManager Current
        {
            get { return SingleInstance; }
        }

        public void Initialize(Language language)
        {
            SelectedLanguage = language;
        }

        #endregion // Constructor and unique instance management

        public Language SelectedLanguage { get; private set; }
    }
}
