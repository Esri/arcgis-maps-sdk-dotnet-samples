using Esri.ArcGISRuntime.Mapping;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using System.Runtime.CompilerServices;

namespace ArcGISRuntimeXamarin.Samples.OAuth
{
	public partial class OAuthPage : ContentPage, INotifyPropertyChanged
	{
        // Constants for OAuth-related values ...
        // URL of the portal to authenticate with
        public const string PortalUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add item ID for a web map on the portal (secured with OAuth)
        public const string WebMapId = "";

        // TODO: Add Client ID for an app registered with the server
        public const string AppClientId = "";

        // TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
        public const string ClientSecret = "";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        public const string OAuthRedirectUrl = "";

        // URL used by the server for authorization
        public const string AuthorizeUrl = "https://www.arcgis.com/sharing/oauth2/authorize";

        private Map _map;
        public Map MyMap
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        public OAuthPage()
		{
			InitializeComponent();
            this.BindingContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }    
}
