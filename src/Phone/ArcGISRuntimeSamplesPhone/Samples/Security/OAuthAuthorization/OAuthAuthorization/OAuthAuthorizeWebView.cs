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

namespace OAuthAuthorization
{
	class OAuthAuthorizeWebView : IOAuthAuthorizeHandler
	{
		private string _callbackUrl;
		private TaskCompletionSource<IDictionary<string, string>> _tcs;
		private Popup _popup;

		public async Task<IDictionary<string, string>> AuthorizeAsync(string serviceUri, string authorizeUri, string callbackUri)
		{
			CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
			if (dispatcher == null)
				throw new Exception("No access to UI thread");

			if (_tcs != null || _popup != null)
				throw new Exception(); // only one authorization process at a time

			_callbackUrl = callbackUri;
			_tcs = new TaskCompletionSource<IDictionary<string, string>>();

			// Set an embedded WebView that displays the authorize page
			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				grid.Height = Window.Current.Bounds.Height;
				grid.Width = Window.Current.Bounds.Width;

				var webBrowser = new WebView { };
				webBrowser.NavigationStarting += webBrowser_NavigationStarting;

				webBrowser.Navigate(new Uri(authorizeUri));
				grid.Children.Add(webBrowser);

				// Display the webBrowser in a popup (default behavior, may be customized by an application)
				_popup = new Popup
				{
					Child = grid,
					IsOpen = true
				};
				_popup.Closed += OnPopupClosed;

			});

			return await _tcs.Task;
		}
		
		// Check if the web browser is redirected to the callback url
		void webBrowser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			var webBrowser = sender as WebView;
			Uri uri = args.Uri;
			if (webBrowser == null || uri == null || _tcs == null)
				return;

			if (!String.IsNullOrEmpty(uri.AbsoluteUri) && uri.AbsoluteUri.StartsWith(_callbackUrl))
			{
				// The web browser is redirected to the callbackUrl ==> close the window, decode the parameters returned as 
				// fragments or query, and return these parameters as result of the Task
				var tcs = _tcs;
				_tcs = null;
				if (_popup != null)
					_popup.IsOpen = false;
				tcs.SetResult(DecodeParameters(uri));
			}
		}

		void OnPopupClosed(object sender, object e)
		{
			if (_tcs != null && !_tcs.Task.IsCompleted)
				_tcs.SetException(new OperationCanceledException()); // user closed the window
			_tcs = null;
			_popup = null;
		}

		/// <summary>
		/// Decodes the parameters returned when the user agent is redirected to the callback url
		/// The parameters can be returned as fragments (e.g. access_token for Browser based app) or as query parameter (e.g. code for Server based app)
		/// </summary>
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
