// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Desktop OAuthAuthorize handler which encapsulates the redirection of the user to the OAuth authorization URI by using a WebBrowser.
	/// </summary>
	public class OAuthAuthorizeHandler : IOAuthAuthorizeHandler
	{
		private Window _window;
		private TaskCompletionSource<IDictionary<string, string>> _tcs;
		private string _callbackUrl;

		/// <summary>
		/// Redirects the user to the authorization URI by using a WebBrowser.
		/// </summary>
		/// <param name="serviceUri">The service URI.</param>
		/// <param name="authorizeUri">The authorize URI.</param>
		/// <param name="callbackUri">The callback URI.</param>
		/// <returns>Dictionary of parameters returned by the authorization URI (code, access_token, refresh_token, ...)</returns>
		public Task<IDictionary<string, string>> AuthorizeAsync(string serviceUri, string authorizeUri, string callbackUri)
		{
			if (_tcs != null || _window != null)
				throw new Exception(); // only one authorization process at a time

			_callbackUrl = callbackUri;
			_tcs = new TaskCompletionSource<IDictionary<string, string>>();
			var tcs = _tcs;

			var dispatcher = Application.Current.Dispatcher;

			if (dispatcher == null || dispatcher.CheckAccess())
				AuthorizeOnUIThread(authorizeUri);
			else
			{
				dispatcher.BeginInvoke((Action)(() => AuthorizeOnUIThread(authorizeUri)));
			}

			return tcs.Task;
		}

		private void AuthorizeOnUIThread(string authorizeUri)
		{
			// Set an embedded webBrowser that displays the authorize page
			var webBrowser = new WebBrowser();
			webBrowser.Navigating += WebBrowserOnNavigating;

			// Display the webBrowser in a window (default behavior, may be customized by an application)
			_window = new Window
			{
				Content = webBrowser,
				Height = 480,
				Width = 480,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				Owner = Application.Current != null && Application.Current.MainWindow != null
							? Application.Current.MainWindow
							: null
			};

			_window.Closed += OnWindowClosed;
			webBrowser.Navigate(authorizeUri);

			// Display the Window
			_window.ShowDialog();
		}

		void OnWindowClosed(object sender, EventArgs e)
		{
			if (_window != null && _window.Owner != null)
				_window.Owner.Focus();
			if (_tcs != null && !_tcs.Task.IsCompleted)
				_tcs.SetException(new OperationCanceledException()); // user closed the window
			_tcs = null;
			_window = null;
		}

		// Check if the web browser is redirected to the callback url
		void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			const string portalApprovalMarker = "/oauth2/approval";
			var webBrowser = sender as WebBrowser;
			Uri uri = e.Uri;
			if (webBrowser == null || uri == null || _tcs == null || string.IsNullOrEmpty(uri.AbsoluteUri))
				return;

			bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
				_callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker); // Portal OAuth workflow with org defined at runtime --> the redirect uri can change
			if (isRedirected)
			{
				// The web browser is redirected to the callbackUrl ==> close the window, decode the parameters returned as fragments or query, and return these parameters as result of the Task
				e.Cancel = true;
				var tcs = _tcs;
				_tcs = null;
				if (_window != null)
				{
					_window.Close();
				}
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
