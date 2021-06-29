// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

#if __IOS__
using Xamarin.Auth;
using Xamarin.Forms.Platform.iOS;
using UIKit;
#endif

#if __ANDROID__
using Android.App;
using Application = Xamarin.Forms.Application;
using Xamarin.Auth;
using System.IO;
#endif

namespace Forms.Helpers
{
    internal static class ArcGISLoginPrompt
    {
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // - The Client ID for an app registered with the server (the ID below is for a public app created by the ArcGIS Runtime team).
        public const string AppClientId = @"6wMAmbUEX1rvsOb4";

        // - An optional client secret for the app (only needed for the OAuthClientCredentials authorization type).
        private const string ClientSecret = "";

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"forms-samples-app://auth";

        public static async Task<bool> EnsureAGOLCredentialAsync()
        {
            bool loggedIn = false;

            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo
                {
                    // Use the OAuth implicit grant flow
                    GenerateTokenOptions = new GenerateTokenOptions
                    {
                        TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode
                    },

                    // Indicate the url (portal) to authenticate with (ArcGIS Online)
                    ServiceUri = new Uri(ArcGISOnlineUrl)
                };

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (OperationCanceledException)
            {
                // OAuth login was canceled, no need to display error to user.
            }
            catch (Exception ex)
            {
                // Login failure
                //MessageBox.Show("Login failed: " + ex.Message);
            }

            return loggedIn;
        }

        public static void SetChallengeHandler()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo(new Uri(ArcGISOnlineUrl))
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode,
                OAuthClientInfo = new OAuthClientInfo(AppClientId, new Uri(OAuthRedirectUrl))
            };

            // If a client secret has been configured, set the authentication type to OAuth client credentials.
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                // Use OAuthClientCredentials if you need a refresh token (and have specified a valid client secret).
                portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthClientCredentials;
                portalServerInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(portalServerInfo);

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(PromptCredentialAsync);

            // Set the OAuthAuthorizeHandler component (this class) for Android or iOS platforms.
#if __ANDROID__ || __IOS__
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();
#endif
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted.
        public static async Task<Credential> PromptCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            // OAuth login was canceled, no need to display error to user.
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }

            return credential;
        }
    }

    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        #region IOAuthAuthorizationHandler implementation

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be canceled.
            if (_taskCompletionSource != null)
            {
                // Try to cancel any existing authentication task.
                _taskCompletionSource.TrySetCanceled();
            }

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();
#if __ANDROID__ || __IOS__

#if __ANDROID__
            // Get the current Android Activity.
            Activity activity = (Activity)ArcGISRuntime.Droid.MainActivity.Instance;
#endif
#if __IOS__
            // Get the current iOS ViewController.
            UIViewController viewController = null;
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            });
#endif
            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            OAuth2Authenticator authenticator = new OAuth2Authenticator(
                clientId: ArcGISLoginPrompt.AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false,
                // Allow the user to cancel the OAuth attempt.
                AllowCancel = true
            };

            // Define a handler for the OAuth2Authenticator.Completed event.
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
#if __IOS__
                    // Dismiss the OAuth UI when complete.
                    viewController.DismissViewController(true, null);
#endif

                    // Check if the user is authenticated.
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account.
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource.
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                    else
                    {
                        throw new Exception("Unable to authenticate user.");
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication.
                    authenticator.OnCancelled();
                }
                finally
                {
                    // Dismiss the OAuth login.
#if __ANDROID__
                    activity.FinishActivity(99);
#endif
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login.
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
#if __ANDROID__
                        activity.FinishActivity(99);
#endif
                    }
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password.
#if __ANDROID__
            var intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);
#endif
#if __IOS__
            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController.PresentViewController(authenticator.GetUI(), true, null);
            });
#endif

#endif // (If Android or iOS)
            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion IOAuthAuthorizationHandler implementation
    }
}