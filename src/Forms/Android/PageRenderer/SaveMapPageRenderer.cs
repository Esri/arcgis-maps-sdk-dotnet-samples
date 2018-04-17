using Android.App;
using ArcGISRuntime.Samples.AuthorEditSaveMap;
using ArcGISRuntime.AndroidPageRenderer;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SaveMapPage), typeof(SaveMapPageRenderer))]
namespace ArcGISRuntime.AndroidPageRenderer
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
        // IOAuthAuthorizeHandler.AuthorizeAsync implementation
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization is in progress
            if (_taskCompletionSource != null)
            {
                // Allow only one authorization process at a time
                throw new Exception();
            }

            // Create a task completion source
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Get the current Android Activity
            var activity = Context as Activity;

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: Samples.AuthorEditSaveMap.AuthorEditSaveMap.AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri);

            // Allow the user to cancel the OAuth attempt
            authenticator.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Check if the user is authenticated
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.SetException(ex);
                }
                finally
                {
                    // Dismiss the OAuth login
                    activity.FinishActivity(99);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.SetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        activity.FinishActivity(99);
                    }
                }
            };

            // Present the OAuth UI so the user can enter user name and password
            var intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }
        #endregion
    }
}