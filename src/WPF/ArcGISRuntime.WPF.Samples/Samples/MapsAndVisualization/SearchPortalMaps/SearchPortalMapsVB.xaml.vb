' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Navigation
Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security

Namespace SearchPortalMaps

    Partial Public Class SearchPortalMapsVB

        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with (ArcGIS Online)
        Private Const ArcGISOnlineUrl As String = "https://www.arcgis.com/sharing/rest"

        ' Client ID for the app registered with the server (Portal Maps)
        Private _appClientId As String = "2Gh53JRzkPtOENQq"

        ' Redirect URL after a successful authorization (configured for the Portal Maps application)
        Private _oAuthRedirectUrl As String = "https://developers.arcgis.com"

        ' Construct sample class
        Public Sub New()

            InitializeComponent()

            ' Show the OAuth settings in the page
            ClientIdTextBox.Text = _appClientId
            RedirectUrlTextBox.Text = _oAuthRedirectUrl

            ' Display a default map
            DisplayDefaultMap()

        End Sub

        Private Sub DisplayDefaultMap()

            ' Create a new Map instance
            Dim myMap As Map = New Map(Basemap.CreateLightGrayCanvas())

            ' Provide Map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Async Sub SearchButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)

            ' Get web map portal items in the current user's folder or from a keyword search
            Dim mapItems As IEnumerable(Of PortalItem) = Nothing
            Dim portal As ArcGISPortal

            ' See if the user wants to search public web map items
            If SearchPublicMaps.IsChecked = True Then
                ' Connect to the portal (anonymously)
                portal = Await ArcGISPortal.CreateAsync(New Uri(ArcGISOnlineUrl))

                ' Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
                Dim queryExpression As String = String.Format("tags:""{0}"" access:public type: (""web map"" NOT ""web mapping application"")", SearchText.Text)
                ' Create a query parameters object with the expression and a limit of 10 results
                Dim queryParams As PortalQueryParameters = New PortalQueryParameters(queryExpression, 10)

                ' Search the portal using the query parameters and await the results
                Dim findResult As PortalQueryResultSet(Of PortalItem) = Await portal.FindItemsAsync(queryParams)
                ' Get the items from the query results
                mapItems = findResult.Results
            Else
                ' Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
                Dim loggedIn As Boolean = Await EnsureLoggedInAsync()
                If Not loggedIn Then Return

                ' Connect to the portal (will connect using the provided credentials)
                portal = Await ArcGISPortal.CreateAsync(New Uri(ArcGISOnlineUrl))

                ' Get the user's content (items in the root folder and a collection of sub-folders)
                Dim myContent As PortalUserContent = Await portal.User.GetContentAsync()

                ' Get the web map items in the root folder
                mapItems = From item In myContent.Items Where item.Type = PortalItemType.WebMap Select item

                ' Loop through all sub-folders and get web map items, add them to the mapItems collection
                For Each folder As PortalFolder In myContent.Folders
                    Dim folderItems As IEnumerable(Of PortalItem) = Await portal.User.GetContentAsync(folder.FolderId)
                    mapItems.Concat(From item In folderItems Where item.Type = PortalItemType.WebMap Select item)
                Next
            End If

            ' Show the web map portal items in the list box
            MapListBox.ItemsSource = mapItems
        End Sub

        Private Sub LoadMapButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)

            ' Get the selected web map item in the list box
            Dim selectedMap As PortalItem = TryCast(MapListBox.SelectedItem, PortalItem)
            If selectedMap Is Nothing Then Return

            ' Create a new map, pass the web map portal item to the constructor
            Dim webMap As Map = New Map(selectedMap)

            ' Handle changes in the load status (to report errors)
            AddHandler webMap.LoadStatusChanged, AddressOf WebMapLoadStatusChanged

            ' Show the web map in the map view
            MyMapView.Map = webMap

        End Sub


        Private Sub WebMapLoadStatusChanged(sender As Object, e As Esri.ArcGISRuntime.LoadStatusEventArgs)

            ' Get the current status
            Dim status As LoadStatus = e.Status

            ' Report errors if map failed to load
            If status = Esri.ArcGISRuntime.LoadStatus.FailedToLoad Then

                Dim myMap As Map = TryCast(sender, Map)
                Dim loadErr As Exception = myMap.LoadError
                If Not loadErr Is Nothing Then

                    MessageBox.Show(loadErr.Message, "Map Load Error")

                End If
            End If
        End Sub

        Private Sub RadioButtonUnchecked(sender As Object, e As System.Windows.RoutedEventArgs)

            ' When the search/user radio buttons are unchecked, clear the list box
            MapListBox.ItemsSource = Nothing

            ' Set the map to the default (if necessary)
            If Not MyMapView.Map.Item Is Nothing Then
                DisplayDefaultMap()
            End If

        End Sub

        Private Async Function EnsureLoggedInAsync() As Task(Of Boolean)

            Dim loggedIn As Boolean = False

            Try
                ' Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                Dim challengeRequest As CredentialRequestInfo = New CredentialRequestInfo()

                ' Use the OAuth implicit grant flow
                Dim tokenOptions = New GenerateTokenOptions()
                tokenOptions.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                challengeRequest.GenerateTokenOptions = tokenOptions

                ' Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = New Uri(ArcGISOnlineUrl)

                ' Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Dim cred As Credential = Await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, False)
                loggedIn = Not cred Is Nothing

            Catch ex As OperationCanceledException
                'TODO: handle login cancellation
            Catch ex As Exception
                'TODO: handle login failure
            End Try

            Return loggedIn

        End Function


        Private Sub SaveOAuthSettingsClicked(sender As Object, e As RoutedEventArgs)

            ' Settings were provided, update the configuration settings for OAuth authorization
            _appClientId = ClientIdTextBox.Text.Trim()
            _oAuthRedirectUrl = RedirectUrlTextBox.Text.Trim()

            ' Update authentication manager with the OAuth settings
            UpdateAuthenticationManager()

            ' Hide the OAuth input, show the search UI
            OAuthSettingsGrid.Visibility = Visibility.Collapsed
            SearchUI.Visibility = Visibility.Visible

        End Sub

        Private Sub CancelOAuthSettingsClicked(sender As Object, e As RoutedEventArgs)

            ' Warn that browsing user's ArcGIS Online maps won't be available without OAuth settings
            Dim noAuth As Boolean = MessageBox.Show("Without OAuth settings, you will not be able to browse maps from your ArcGIS Online account.", "No OAuth Settings", MessageBoxButton.OKCancel) = MessageBoxResult.OK

            If noAuth Then
                ' Disable browsing maps from your ArcGIS Online account
                BrowseMyMaps.IsEnabled = False

                ' Hide the OAuth input, show the search UI
                OAuthSettingsGrid.Visibility = Visibility.Collapsed
                SearchUI.Visibility = Visibility.Visible
            End If
        End Sub

        Private Sub UpdateAuthenticationManager()
            ' Register the server information with the AuthenticationManager
            Dim portalServerInfo As ServerInfo = New ServerInfo With
            {
                .TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                .ServerUri = New Uri(ArcGISOnlineUrl),
                .OAuthClientInfo = New OAuthClientInfo With
                {
                    .ClientId = _appClientId,
                    .RedirectUri = New Uri(_oAuthRedirectUrl)
                }
            }

            ' Get a reference to the (singleton) AuthenticationManager for the app
            Dim thisAuthenticationManager As AuthenticationManager = AuthenticationManager.Current

            ' Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo)

            ' Use the OAuthAuthorize class in this project to create a New web view that contains the OAuth challenge handler.
            thisAuthenticationManager.OAuthAuthorizeHandler = New OAuthAuthorize()

            ' Create a New ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)
        End Sub

        ' ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        ' resource is attempted
        Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
            Dim cred As Credential = Nothing

            Try
                ' Create generate token options if necessary
                If info.GenerateTokenOptions Is Nothing Then
                    info.GenerateTokenOptions = New GenerateTokenOptions()
                End If

                ' IOAuthAuthorizeHandler will challenge the user for credentials
                cred = Await AuthenticationManager.Current.GenerateCredentialAsync(
                            info.ServiceUri,
                            info.GenerateTokenOptions)

            Catch ex As Exception
                ' Exception will be reported in calling function
                Throw (ex)
            End Try

            Return cred
        End Function

    End Class

#Region "OAuth Authorization class (IOAuthAuthorizeHandler)"
    Public Class OAuthAuthorize
        Implements IOAuthAuthorizeHandler
        ' Window to contain the OAuth UI
        Private _window As Window

        ' Use a TaskCompletionSource to track the completion of the authorization
        Private _tcs As TaskCompletionSource(Of IDictionary(Of String, String))

        ' URL for the authorization callback result (the redirect URI configured for your application)
        Private _callbackUrl As String

        ' URL that handles the OAuth request
        Private _authorizeUrl As String

        ' Function to handle authorization requests, takes the URIs for the secured service, the authorization endpoint, And the redirect URI
        Public Function AuthorizeAsync(serviceUri As Uri, authorizeUri As Uri, callbackUri As Uri) As Task(Of IDictionary(Of String, String)) Implements IOAuthAuthorizeHandler.AuthorizeAsync
            ' If the TaskCompletionSource Or Window are Not null, authorization Is in progress
            If Not _tcs Is Nothing AndAlso Not _tcs.Task.IsCompleted Then
                ' Allow only one authorization process at a time
                Throw New Exception()
            End If

            ' Store the authorization And redirect URLs
            _authorizeUrl = authorizeUri.AbsoluteUri
            _callbackUrl = callbackUri.AbsoluteUri

            ' Create a task completion source
            _tcs = New TaskCompletionSource(Of IDictionary(Of String, String))()
            Dim localTcs As TaskCompletionSource(Of IDictionary(Of String, String)) = _tcs

            ' Call a function to show the login controls, make sure it runs on the UI thread for this app
            Dim thisDispatcher As Threading.Dispatcher = Application.Current.Dispatcher
            If thisDispatcher Is Nothing Or thisDispatcher.CheckAccess() Then
                AuthorizeOnUIThread(_authorizeUrl)
            Else
                Dim authorizeOnUIDelegate As AuthorizeDelegate = New AuthorizeDelegate(AddressOf AuthorizeOnUIThread)
                thisDispatcher.BeginInvoke(authorizeOnUIDelegate, _authorizeUrl)
            End If

            ' Return the task associated with the TaskCompletionSource
            Return localTcs.Task
        End Function

        ' Delegate to handle calling AuthorizeOnUIThread from BeginInvoke
        Delegate Sub AuthorizeDelegate(url As String)

        ' Challenge for OAuth credentials on the UI thread
        Private Sub AuthorizeOnUIThread(authorizeUri As String)
            ' Create a WebBrowser control to display the authorize page
            Dim browser As WebBrowser = New WebBrowser()
            ' Handle the navigation event for the browser to check for a response to the redirect URL
            AddHandler browser.Navigating, AddressOf WebBrowserOnNavigating

            ' Display the web browser in a New window 
            _window = New Window With
            {
                .Content = browser,
                .Height = 430,
                .Width = 395,
                .WindowStartupLocation = WindowStartupLocation.CenterOwner
            }


            ' Set the app's window as the owner of the browser window (if main window closes, so will the browser)
            If Not Application.Current Is Nothing And Not Application.Current.MainWindow Is Nothing Then
                _window.Owner = Application.Current.MainWindow
            End If

            ' Handle the window closed event then navigate to the authorize url
            AddHandler _window.Closed, AddressOf OnWindowClosed
            browser.Navigate(authorizeUri)

            ' Display the Window
            _window.ShowDialog()
        End Sub

        ' Handle the browser window closing
        Private Sub OnWindowClosed(sender As Object, e As EventArgs)
            ' If the browser window closes, return the focus to the main window
            If Not _window Is Nothing AndAlso Not _window.Owner Is Nothing Then
                _window.Owner.Focus()
            End If

            ' If the task wasn't completed, the user must have closed the window without logging in
            If Not _tcs Is Nothing AndAlso Not _tcs.Task.IsCompleted Then
                ' Indicate a canceled operation
                _tcs.SetCanceled()
            End If

            ' Set the task completion source and window to Nothing to indicate the authorization process is complete
            _window = Nothing
        End Sub

        ' Handle browser navigation (content changing)
        Private Sub WebBrowserOnNavigating(sender As Object, e As NavigatingCancelEventArgs)
            ' Check for a response to the callback url
            Const portalApprovalMarker As String = "/oauth2/approval"
            Dim browser As WebBrowser = TryCast(sender, WebBrowser)
            Dim currentUri As Uri = e.Uri

            ' If no browser, uri, task completion source, Or an empty url, return
            If browser Is Nothing Or currentUri Is Nothing Or _tcs Is Nothing OrElse String.IsNullOrEmpty(currentUri.AbsoluteUri) Then Return

            ' Check for redirect
            Dim isRedirected As Boolean = currentUri.AbsoluteUri.StartsWith(_callbackUrl) Or
                _callbackUrl.Contains(portalApprovalMarker) And currentUri.AbsoluteUri.Contains(portalApprovalMarker)

            If isRedirected Then
                ' If the web browser Is redirected to the callbackUrl:
                '   -close the window 
                '   -decode the parameters (returned as fragments Or query)
                '   -return these parameters as result of the Task
                e.Cancel = True
                Dim localTcs As TaskCompletionSource(Of IDictionary(Of String, String)) = _tcs
                _tcs = Nothing
                If Not _window Is Nothing Then
                    _window.Close()
                End If

                ' Call a helper function to decode the response parameters
                Dim authResponse As IDictionary(Of String, String) = DecodeParameters(currentUri)

                ' Set the result for the task completion source
                localTcs.SetResult(authResponse)
            End If
        End Sub

        ' Decodes the parameters returned when the browser Is redirected to the callback url
        Private Shared Function DecodeParameters(responseUri As Uri) As IDictionary(Of String, String)
            Dim answer As String = String.Empty
            ' Get the response text from the URI fragment or query string
            If Not String.IsNullOrEmpty(responseUri.Fragment) Then
                answer = responseUri.Fragment.Substring(1)
            Else
                If Not String.IsNullOrEmpty(responseUri.Query) Then
                    answer = responseUri.Query.Substring(1)
                End If
            End If

            ' Parse the response into a dictionary of key / value pairs
            Dim parametersDictionary As Dictionary(Of String, String) = New Dictionary(Of String, String)()
            Dim keysAndValues As String() = answer.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            For Each keyAndValueString As String In keysAndValues
                Dim keyAndValue As String() = keyAndValueString.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                Dim key As String = keyAndValue(0)
                Dim value As String = String.Empty
                If keyAndValue.Length > 1 Then
                    value = Uri.UnescapeDataString(keyAndValue(1))
                End If

                parametersDictionary.Add(key, value)
            Next

            Return parametersDictionary
        End Function
    End Class
#End Region

End Namespace


