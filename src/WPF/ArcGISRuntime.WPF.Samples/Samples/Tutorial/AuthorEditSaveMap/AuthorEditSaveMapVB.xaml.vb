' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Navigation
Imports System.Runtime.CompilerServices
Imports System.ComponentModel

Namespace AuthorEditSaveMap
    Partial Public Class AuthorEditSaveMapVB
        ' MapViewModel variable
        Private _mapViewModel As MapViewModel

        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with
        Private Const ArcGISOnlineUrl As String = "https://www.arcgis.com/sharing/rest"

        ' Client ID For an app registered With the server
        Private Const AppClientId As String = "2Gh53JRzkPtOENQq"

        ' URL For redirecting after a successful authorization
        '       Note - this must be a URL configured as a valid Redirect URI with your app
        Private Const OAuthRedirectUrl As String = "https://developers.arcgis.com"

        Public Sub New()
            InitializeComponent()

            ' Get the view model (static resource defined in the page XAML)
            _mapViewModel = TryCast(Me.FindResource("MapViewModel"), MapViewModel)

            ' Define a handler for selection changed on the basemap list
            AddHandler BasemapListBox.SelectionChanged, AddressOf OnBasemapsClicked

            ' Define a handler for the Save Map click
            AddHandler SaveMapButton.Click, AddressOf OnSaveMapClick

            ' Define a handler for the New Map click
            AddHandler NewMapButton.Click, AddressOf OnNewMapClicked

            ' Call a function to update the authentication manager settings
            UpdateAuthenticationManager()
        End Sub

        Private Sub OnBasemapsClicked(sender As Object, e As SelectionChangedEventArgs)
            ' Get the text (basemap name) selected in the list box
            Dim basemapName As String = e.AddedItems(0).ToString()

            ' Pass the basemap name to the view model method to change the basemap
            _mapViewModel.ChangeBasemap(basemapName)
        End Sub

        Private Async Sub OnSaveMapClick(sender As Object, e As RoutedEventArgs)
            Try
                ' Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                Dim challengeRequest As New CredentialRequestInfo()

                ' Use the OAuth implicit grant flow
                Dim tokenOptions As New GenerateTokenOptions()
                tokenOptions.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                challengeRequest.GenerateTokenOptions = tokenOptions

                ' Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = New Uri("https://www.arcgis.com/sharing/rest")

                ' Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, False)

                ' Get information for the New portal item
                Dim title As String = TitleTextBox.Text
                Dim description As String = DescriptionTextBox.Text
                Dim tags As String() = TagsTextBox.Text.Split(",".ToCharArray())

                ' Return if the text Is null Or empty
                If (String.IsNullOrEmpty(title) Or String.IsNullOrEmpty(description)) Then
                    Return
                End If

                ' Get current map extent (viewpoint) for the map initial extent
                Dim currentViewpoint As Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

                ' See if the map has already been saved
                If (Not _mapViewModel.MapIsSaved) Then
                    ' Call the SaveNewMapAsync method on the view model, pass in the required info
                    Await _mapViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags)

                    ' Report success
                    MessageBox.Show("Map '" + title + "' was saved to the portal.", "Saved Map")
                Else
                    ' Map has previously been saved as a portal item, update it (title And description will remain the same)
                    _mapViewModel.UpdateMapItem()

                    ' Report success
                    MessageBox.Show("Changes to '" + title + "' were updated to the portal.")
                End If
            Catch ex As OperationCanceledException
                ' Report canceled login
                MessageBox.Show("Login to the portal was canceled.", "Save canceled")
            Catch ex As Exception
                ' Report error
                MessageBox.Show("Error while saving: " + ex.Message, "Cannot save")
            End Try
        End Sub

        ' Reset (create a New) map
        Private Sub OnNewMapClicked(sender As Object, e As EventArgs)
            _mapViewModel.ResetMap()
        End Sub

        Private Sub UpdateAuthenticationManager()
            ' Register the server information with the AuthenticationManager
            Dim portalServerInfo As ServerInfo = New ServerInfo With
            {
                .TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                .ServerUri = New Uri(ArcGISOnlineUrl),
                .OAuthClientInfo = New OAuthClientInfo With
                {
                    .ClientId = AppClientId,
                    .RedirectUri = New Uri(OAuthRedirectUrl)
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
            Dim credenshul As Credential = Nothing

            Try
                ' Create generate token options if necessary
                If info.GenerateTokenOptions Is Nothing Then
                    info.GenerateTokenOptions = New GenerateTokenOptions()
                End If

                ' IOAuthAuthorizeHandler will challenge the user for credentials
                credenshul = Await AuthenticationManager.Current.GenerateCredentialAsync(
                            info.ServiceUri,
                            info.GenerateTokenOptions)

            Catch ex As Exception
                ' Exception will be reported in calling function
                Throw (ex)
            End Try

            Return credenshul
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
            If Not _tcs Is Nothing Or Not _window Is Nothing Then
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
                ' Set the task completion source exception to indicate a canceled operation
                _tcs.SetException(New OperationCanceledException())
            End If

            ' Set the task completion source and window to Nothing to indicate the authorization process is complete
            _tcs = Nothing
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

    ' Provides map data to an application
    ' Note: in a ArcGIS Runtime for .NET (C#) template project, this class will be in a separate file: "MapViewModel.cs"
    Public Class MapViewModel
        Implements INotifyPropertyChanged

        ' String array to store basemap constructor types
        Private _basemapTypes As String() =
        {
            "Topographic",
            "Topographic Vector",
            "Streets",
            "Streets Vector",
            "Imagery",
            "Oceans"
        }

        ' Read-only property to return the available basemap names
        Public ReadOnly Property BasemapChoices As String()
            Get
                Return _basemapTypes
            End Get
        End Property

        Public Sub New()
            ' Default constructor
        End Sub

        Private _map As Map = New Map(Basemap.CreateStreetsVector())

        ' Gets or sets the map
        Public Property MyMap As Map
            Get
                Return _map
            End Get
            Set
                _map = Value
                OnPropertyChanged()
            End Set
        End Property

        Public Sub ChangeBasemap(basemapName As String)
            ' Apply the selected basemap to the map
            Select Case basemapName
                Case "Topographic"
                    ' Set the basemap to Topographic
                    MyMap.Basemap = Basemap.CreateTopographic()
                Case "Topographic Vector"
                    ' Set the basemap to Topographic vector
                    MyMap.Basemap = Basemap.CreateTopographicVector()
                Case "Streets"
                    ' Set the basemap to Streets
                    MyMap.Basemap = Basemap.CreateStreets()
                Case "Streets Vector"
                    ' Set the basemap to Streets vector
                    MyMap.Basemap = Basemap.CreateStreetsVector()
                Case "Imagery"
                    ' Set the basemap to Imagery
                    MyMap.Basemap = Basemap.CreateImagery()
                Case "Oceans"
                    ' Set the basemap to Oceans
                    MyMap.Basemap = Basemap.CreateOceans()
                Case Else
            End Select
        End Sub

        ' Save the current map to ArcGIS Online. The initial extent, title, description, And tags are passed in.
        Public Async Function SaveNewMapAsync(initialViewpoint As Viewpoint, title As String, description As String, tags As String()) As Task
            ' Get the ArcGIS Online portal 
            Dim agsOnline As ArcGISPortal = Await ArcGISPortal.CreateAsync(New Uri("https://www.arcgis.com/sharing/rest"))

            ' Set the map's initial viewpoint using the extent (viewpoint) passed in
            _map.InitialViewpoint = initialViewpoint

            ' Save the current state of the map as a portal item in the user's default folder
            Await _map.SaveAsAsync(agsOnline, Nothing, title, description, tags, Nothing)
        End Function

        Public Sub ResetMap()
            ' Set the current map to null
            _map = Nothing

            ' Create a New map with light gray canvas basemap
            Dim newMap As New Map(Basemap.CreateLightGrayCanvasVector())

            ' Store the New map 
            MyMap = newMap
        End Sub

        Public ReadOnly Property MapIsSaved As Boolean
            ' Return True if the current map has a value for the Item property
            Get
                Return (Not _map Is Nothing AndAlso Not _map.Item Is Nothing)
            End Get
        End Property

        Public Sub UpdateMapItem()
            ' Save the map
            _map.SaveAsync()
        End Sub

        ' Raises the PropertyChanged event for a property
        Protected Sub OnPropertyChanged(<CallerMemberName> Optional ByVal propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    End Class
End Namespace
