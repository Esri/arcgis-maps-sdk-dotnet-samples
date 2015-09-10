// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Phone/Store OAuthAuthorize handler which encapsulates the redirection of the user to the OAuth authorization URI by using a WebBrowser.
	/// </summary>
	public class OAuthAuthorizeHandler : IOAuthAuthorizeHandler
	{
		private TaskCompletionSource<IDictionary<string, string>> _tcs;
		private string _callbackUrl;
		private Popup _popup;

		/// <summary>
		/// Redirects the user to the authorization URI by using a WebBrowser.
		/// </summary>
		/// <param name="serviceUri">The service URI.</param>
		/// <param name="authorizeUri">The authorize URI.</param>
		/// <param name="callbackUri">The callback URI.</param>
		/// <returns>Dictionary of parameters returned by the authorization URI (code, access_token, refresh_token, ...)</returns>
		public async Task<IDictionary<string, string>> AuthorizeAsync(string serviceUri, string authorizeUri, string callbackUri)
		{
			CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
			if (dispatcher == null)
				throw new Exception("No access to UI thread");

			if (_tcs != null || _popup != null)
				throw new Exception(); // only one authorization process at a time

			_callbackUrl = callbackUri;
			_tcs = new TaskCompletionSource<IDictionary<string, string>>();


			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				grid.Height = Window.Current.Bounds.Height;
				grid.Width = Window.Current.Bounds.Width;

				var webView = new WebView();
				webView.NavigationStarting += WebViewOnNavigationStarting;

				webView.Navigate(new Uri(authorizeUri));
				grid.Children.Add(webView);

				_popup = new Popup
				{
					Child = grid,
					IsOpen = true
				};

				_popup.Closed += OnPopupClosed;


			});

			return await _tcs.Task;
		}

		private void OnPopupClosed(object sender, object e)
		{
			if (_tcs != null && !_tcs.Task.IsCompleted)
				_tcs.SetException(new OperationCanceledException()); // user closed the window
			_tcs = null;
			_popup = null;
		}


		private void WebViewOnNavigationStarting(WebView webView, WebViewNavigationStartingEventArgs args)
		{
			const string portalApprovalMarker = "/oauth2/approval";
			Uri uri = args.Uri;
			if (webView == null || uri == null || _tcs == null || string.IsNullOrEmpty(uri.AbsoluteUri))
				return;

			bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
				_callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker);
			if (isRedirected)
			{
				var tcs = _tcs;
				_tcs = null;
				if (_popup != null)
					_popup.IsOpen = false;
				tcs.SetResult(DecodeParameters(uri));
			}
		}

		/// <summary>
		/// Decodes the parameters returned when the user agent is redirected to the callback url
		/// The parameters can be returned as fragments (e.g. access_token for Browser based app) or as query parameter (e.g. code for Server based app)
		/// </summary>
		/// <param name="uri">The URI.</param>
		private static IDictionary<string, string> DecodeParameters(Uri uri)
		{
			string answer = !string.IsNullOrEmpty(uri.Fragment)
								? uri.Fragment.Substring(1)
								: (!string.IsNullOrEmpty(uri.Query) ? uri.Query.Substring(1) : string.Empty);

			// decode parameters from format key1=value1&key2=value2&...
			return answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=')).ToDictionary(pair => pair[0], pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : null);
		}
	}
}
