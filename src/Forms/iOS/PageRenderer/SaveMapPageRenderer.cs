using ArcGISRuntime.Samples.AuthorEditSaveMap;
using ArcGISRuntime.iOSPageRenderer;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SaveMapPage), typeof(SaveMapPageRenderer))]
namespace ArcGISRuntime.iOSPageRenderer
{
    public class SaveMapPageRenderer : PageRenderer, IOAuthAuthorizeHandler
    {
        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
        
        // ctor
        public SaveMapPageRenderer()
        {
            // Set the OAuth authorization handler to this class (Implements IOAuthAuthorize interface)
            AuthenticationManager.Current.OAuthAuthorizeHandler = this;
        }

        #region IOAuthAuthorizationHandler implementation
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be cancelled
            if (_taskCompletionSource != null)
            {
                // Try to cancel any existing authentication task
                _taskCompletionSource.TrySetCanceled();
            }

            // Create a task completion source
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            var authenticator = new OAuth2Authenticator(
                clientId: Samples.AuthorEditSaveMap.AuthorEditSaveMap.AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false
            };

            // Allow the user to cancel the OAuth attempt
            authenticator.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete
                    DismissViewController(true, null);

                    // Throw an exception if the user could not be authenticated
                    if (!authArgs.IsAuthenticated)
                    {
                        throw new Exception("Unable to authenticate user.");
                    }

                    // If authorization was successful, get the user's account
                    Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                    // Set the result (Credential) for the TaskCompletionSource
                    _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication
                    authenticator.OnCancelled();
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    _taskCompletionSource.TrySetException(new Exception(errArgs.Message));
                }

                // Cancel authentication
                authenticator.OnCancelled();
            };

            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password
            InvokeOnMainThread(() =>
            {
                PresentViewController(authenticator.GetUI(), true, null);
            });

            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }
        #endregion
    }
}