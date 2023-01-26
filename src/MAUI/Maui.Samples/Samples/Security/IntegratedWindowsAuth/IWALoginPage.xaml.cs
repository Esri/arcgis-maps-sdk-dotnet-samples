namespace ArcGIS.Samples.IntegratedWindowsAuth
{
    public partial class IWALoginPage : ContentPage
    {
        // Event to provide login information when the user dismisses the view.
        public event EventHandler<IWALoginEventArgs> OnLoginInfoEntered;

        // Event to report that the login was canceled.
        public event EventHandler OnCanceled;

        public IWALoginPage()
        {
            InitializeComponent();
        }

        // Text to display at the top of the login controls.
        public string TitleText
        {
            set
            {
                LoginLabel.Text = value;
            }
        }

        private void LoginButtonClicked(object sender, EventArgs e)
        {
            // Get the values entered in the text fields.
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string domain = DomainEntry.Text;

            // Make sure the user entered all values.
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(domain))
            {
                Application.Current.MainPage.DisplayAlert("Login", "Please enter credentials.", "OK");
                return;
            }

            // Fire the OnLoginInfoEntered event and provide the login values.
            if (OnLoginInfoEntered != null)
            {
                // Create a new LoginEventArgs to contain the user's values.
                var loginEventArgs = new IWALoginEventArgs(username.Trim(), password.Trim(), domain.Trim());

                // Raise the event.
                OnLoginInfoEntered(sender, loginEventArgs);
            }
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            // Fire the OnCanceled event to let the calling code no the login was canceled.
            if (OnCanceled != null)
            {
                OnCanceled(this, null);
            }
        }
    }

}

// Custom EventArgs implementation to hold login information (username and password).
public class IWALoginEventArgs : EventArgs
{
    // Username property.
    public string Username { get; set; }

    // Password property.
    public string Password { get; set; }

    // Domain property.
    public string Domain { get; set; }

    // Store login values passed into the constructor.
    public IWALoginEventArgs(string username, string password, string domain)
    {
        Username = username;
        Password = password;
        Domain = domain;
    }

}