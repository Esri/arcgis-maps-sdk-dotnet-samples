using System.Windows;
using System.Windows.Controls;

namespace IntegratedWindowsAuth
{    
    // A simple UI for entering network credential information (username, password, and domain)
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // A handler for both the "Login" and "Cancel" buttons on the page
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            // Set the dialog result to indicate whether or not "Cancel" was clicked
            this.DialogResult = !(sender as Button).IsCancel;

            // Close the window
            this.Close();
        }
    }
}
