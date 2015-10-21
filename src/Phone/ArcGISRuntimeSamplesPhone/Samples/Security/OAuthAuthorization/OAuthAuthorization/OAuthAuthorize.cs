using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;

namespace OAuthAuthorization
{
	class OAuthAuthorize : IOAuthAuthorizeHandler, IWebAuthenticationContinuable
	{
		private TaskCompletionSource<IDictionary<string, string>> _tcs;
		public async Task<IDictionary<string, string>> AuthorizeAsync(string serviceUri, string authorizeUri, string callbackUrl)
		{
			_tcs = new TaskCompletionSource<IDictionary<string, string>>();

			CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
			if (dispatcher == null)
				throw new Exception("No access to UI thread");

			var requestUri = new Uri(authorizeUri);
			var callbackUri = new Uri(callbackUrl);
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				// Add entry in ContinuationManager to check for WebAuthenticationBrokerContinuation
				ValueSet continuationValues = new ValueSet();
				continuationValues.Add("IdentityManager.OAuthAuthorize", serviceUri);
				WebAuthenticationBroker.AuthenticateAndContinue(requestUri, callbackUri,
					continuationValues, WebAuthenticationOptions.None);
			});

			return await _tcs.Task;
		}

		private static IDictionary<string, string> DecodeParameters(Uri uri)
		{
			string answer = !string.IsNullOrEmpty(uri.Fragment)
								? uri.Fragment.Substring(1)
								: (!string.IsNullOrEmpty(uri.Query) ? uri.Query.Substring(1) : string.Empty);

			return answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=')).ToDictionary(pair => pair[0], pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : null);
		}

		public void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
		{
			try
			{
				WebAuthenticationResult result = args.WebAuthenticationResult;
				var tcs = _tcs;
				_tcs = null;
				tcs.SetResult(DecodeParameters(new Uri(result.ResponseData)));
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}
	}
}

