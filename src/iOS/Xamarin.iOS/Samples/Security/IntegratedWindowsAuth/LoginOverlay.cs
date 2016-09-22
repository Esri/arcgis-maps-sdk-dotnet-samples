using CoreGraphics;
using System;
using UIKit;

namespace Security.IWA
{
    // View containing login controls (username, password, and domain)
    public class LoginOverlay : UIView
    {
        // Event to provide login information when the user dismisses the view
        public event EventHandler<LoginEventArgs> OnLoginInfoEntered;

        // Event to report that the login was canceled
        public event EventHandler OnCanceled;

        // Store the username, password, and domain controls so the values can be read
        private UITextField _usernameTextField;
        private UITextField _passwordTextField;
        private UITextField _domainTextField;

        public LoginOverlay(CGRect frame, nfloat transparency, UIColor color) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = Frame.Width - 60;
            nfloat buttonWidth = 60;

            // Get the total height and width of the control set (four rows of controls, three sets of space)
            nfloat totalHeight = (4 * controlHeight) + (3 * rowSpace);
            nfloat totalWidth = textViewWidth;

            // Find the center x and y of the view
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout
            nfloat controlX = centerX - (totalWidth / 2);
            nfloat controlY = centerY - (totalHeight / 2);

            // Username text input
            _usernameTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight));
            _usernameTextField.Placeholder = "Username";
            _usernameTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _usernameTextField.BackgroundColor = UIColor.LightGray;

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Password text input
            _passwordTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight));
            _passwordTextField.SecureTextEntry = true;
            _passwordTextField.Placeholder = "Password";
            _passwordTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _passwordTextField.BackgroundColor = UIColor.LightGray;

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Domain text input
            _domainTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight));
            _domainTextField.Placeholder = "Domain";
            _domainTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _domainTextField.BackgroundColor = UIColor.LightGray;

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Button to submit the login information
            UIButton loginButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            loginButton.SetTitle("Login", UIControlState.Normal);
            loginButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            loginButton.TouchUpInside += LoginButtonClick;

            // Adjust the X position for the next control
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the login
            UIButton cancelButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls
            AddSubviews(_usernameTextField, _passwordTextField, _domainTextField, loginButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields
            var username = _usernameTextField.Text.Trim();
            var password = _passwordTextField.Text.Trim();
            var domain = _domainTextField.Text.Trim();

            // Make sure the user entered all values
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(domain))
            {
                new UIAlertView("Login", "Please enter a username, password, and domain", null, "OK", null).Show();
                return;
            }

            // Fire the OnLoginInfoEntered event and provide the login values
            if (OnLoginInfoEntered != null)
            {
                // Create a new LoginEventArgs to contain the user's values
                var loginEventArgs = new LoginEventArgs(username, password, domain);

                // Raise the event
                OnLoginInfoEntered(sender, loginEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold login information (username, password, and domain)
    public class LoginEventArgs : EventArgs
    {
        // Username property
        public string Username { get; set; }

        // Password property
        public string Password { get; set; }

        // Domain property
        public string Domain { get; set; }

        // Store login values passed into the constructor
        public LoginEventArgs(string username, string password, string domain)
        {
            Username = username;
            Password = password;
            Domain = domain;
        }
    }
}