' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Navigation

Namespace AuthorMap
    Partial Public Class AuthorMapVB
        ' The map object that will be saved as a portal item
        Private _myMap As Map

        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with
        Private Const ServerUrl As String = "https://www.arcgis.com/sharing/rest"

        ' TODO: Add Client ID For an app registered With the server
        Private Const AppClientId As String = "2Gh53JRzkPtOENQq"

        ' TODO: Add URL For redirecting after a successful authorization
        '       Note - this must be a URL configured as a valid Redirect URI with your app
        Private Const OAuthRedirectUrl As String = "http://myapps.portalmapapp"

        ' String array to store names of the available basemaps
        Private _basemapNames As String() =
        {
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        }

        ' Dictionary of operational layer names And URLs
        Private _operationalLayerUrls As Dictionary(Of String, String) = New Dictionary(Of String, String) From
        {
            {"World Elevations", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"World Cities", "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/"},
            {"US Census Data", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
        }

        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Show a plain gray map in the map view
            MyMapView.Map = New Map(Basemap.CreateLightGrayCanvas())

            ' Fill the basemap combo box with basemap names
            BasemapListBox.ItemsSource = _basemapNames

            ' Select the first basemap in the list by default
            BasemapListBox.SelectedIndex = 0

            ' Fill the operational layers list box with layer names
            OperationalLayerListBox.ItemsSource = _operationalLayerUrls

            ' Setup the AuthenticationManager to challenge for credentials
            UpdateAuthenticationManager()

            ' Update the extent labels whenever the view point (extent) changes
            AddHandler MyMapView.ViewpointChanged, AddressOf UpdateViewExtentLabels
        End Sub

        Private Sub ApplyBasemap(basemapName As String)
            ' Set the basemap for the map according to the user's choice in the list box
            Select Case basemapName
                Case "Topographic"
                    ' Set the basemap to Topographic
                    _myMap.Basemap = Basemap.CreateTopographic()
                Case "Streets"
                    ' Set the basemap to Streets
                    _myMap.Basemap = Basemap.CreateStreets()
                Case "Imagery"
                    ' Set the basemap to Imagery
                    _myMap.Basemap = Basemap.CreateImagery()
                Case "Ocean"
                    ' Set the basemap to Oceans
                    _myMap.Basemap = Basemap.CreateOceans()
                Case Else
            End Select
        End Sub

        Private Sub AddOperationalLayers()
            ' Loop through the selected items in the operational layers list box
            For Each item As KeyValuePair(Of String, String) In OperationalLayerListBox.SelectedItems
                ' Get the service uri for each selected item 
                Dim layerUri As Uri = New Uri(item.Value)

                ' Create a New map image layer, set it 50% opaque, And add it to the map
                Dim mapServiceLayer As ArcGISMapImageLayer = New ArcGISMapImageLayer(layerUri)
                mapServiceLayer.Opacity = 0.5
                _myMap.OperationalLayers.Add(mapServiceLayer)
            Next
        End Sub

        Private Sub UpdateMap(sender As Object, e As System.Windows.RoutedEventArgs)
            ' Create a New (empty) map
            If _myMap Is Nothing OrElse _myMap.PortalItem Is Nothing Then
                _myMap = New Map()
            End If

            ' Call functions that apply the selected basemap And operational layers
            ApplyBasemap(BasemapListBox.SelectedValue.ToString())
            AddOperationalLayers()

            ' Use the current extent to set the initial viewpoint for the map
            _myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

            ' Show the New map in the map view
            MyMapView.Map = _myMap
        End Sub

        Private Async Sub SaveMap(sender As Object, e As System.Windows.RoutedEventArgs)
            ' Make sure the map Is Not null
            If _myMap Is Nothing Then
                MessageBox.Show("Please update the map before saving.", "Map is empty")
                Return
            End If

            ' See if the map has already been saved (has an associated portal item)
            If _myMap.PortalItem Is Nothing Then
                ' This Is the initial save for this map

                ' Call a function that will challenge the user for ArcGIS Online credentials
                Dim isLoggedIn As Boolean = Await EnsureLoginToArcGISAsync()

                ' If the user could Not log in (Or canceled the login), exit
                If Not isLoggedIn Then Return

                ' Get the ArcGIS Online portal
                Dim agsOnline As ArcGISPortal = Await ArcGISPortal.CreateAsync()

                ' Get information for the New portal item
                Dim title As String = TitleTextBox.Text
                Dim description As String = DescriptionTextBox.Text
                Dim tags As String() = TagsTextBox.Text.Split(",".ToCharArray())

                Try
                    ' Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible

                    ' Save the current state of the map as a portal item in the user's default folder
                    Await _myMap.SaveAsAsync(agsOnline, Nothing, title, description, tags, Nothing)
                    MessageBox.Show("Saved '" + title + "' to ArcGIS Online!", "Map Saved")
                Catch ex As Exception
                    MessageBox.Show("Unable to save map to ArcGIS Online: " + ex.Message)
                Finally
                    ' Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Hidden
                End Try
            Else
                ' This Is an update to the existing portal item
                Try
                    ' Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible

                    ' This Is Not the initial save, call SaveAsync to save changes to the existing portal item
                    Await _myMap.SaveAsync()
                    MessageBox.Show("Saved changes to '" + _myMap.PortalItem.Title + "'", "Updates Saved")
                Catch ex As Exception
                    MessageBox.Show("Unable to save map updates: " + ex.Message)
                Finally
                    ' Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Hidden
                End Try
            End If
        End Sub

        Private Sub ClearMap(sender As Object, e As RoutedEventArgs)
            ' Set the map to null
            _myMap = Nothing

            ' Show a plain gray map in the map view
            MyMapView.Map = New Map(Basemap.CreateLightGrayCanvas())
        End Sub

        Private Sub UpdateViewExtentLabels(sender As Object, e As EventArgs)
            ' Get the current view point for the map view
            Dim currentViewpoint As Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
            If currentViewpoint Is Nothing Then Return

            ' Get the current map extent (envelope) from the view point
            Dim currentExtent As Envelope = TryCast(currentViewpoint.TargetGeometry, Envelope)

            ' Project the current extent to geographic coordinates (longitude / latitude)
            Dim currentGeoExtent As Envelope = TryCast(GeometryEngine.Project(currentExtent, SpatialReferences.Wgs84), Envelope)

            ' Fill the app text boxes with min / max longitude (x) And latitude (y) to four decimal places
            XMinTextBox.Text = currentGeoExtent.XMin.ToString("0.####")
            YMinTextBox.Text = currentGeoExtent.YMin.ToString("0.####")
            XMaxTextBox.Text = currentGeoExtent.XMax.ToString("0.####")
            YMaxTextBox.Text = currentGeoExtent.YMax.ToString("0.####")
        End Sub

        Private Sub UpdateAuthenticationManager()
            ' Register the server information with the AuthenticationManager
            Dim portalServerInfo As ServerInfo = New ServerInfo With
            {
                .TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                .ServerUri = New Uri(ServerUrl),
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

        Private Async Function EnsureLoginToArcGISAsync() As Task(Of Boolean)
            Dim authenticated As Boolean = False

            ' Create an OAuth credential request for arcgis.com
            Dim loginInfo As CredentialRequestInfo = New CredentialRequestInfo()

            ' Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = New GenerateTokenOptions With
            {
                .TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            }

            ' Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = New Uri("http://www.arcgis.com/sharing/rest")

            Try
                ' Get a reference to the (singleton) AuthenticationManager for the app
                Dim thisAuthenticationManager As AuthenticationManager = AuthenticationManager.Current

                ' Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Dim cred As Credential = Await thisAuthenticationManager.GetCredentialAsync(loginInfo, False)
                authenticated = Not cred Is Nothing
            Catch canceledEx As OperationCanceledException
                ' user canceled the login
                MessageBox.Show("Maps cannot be saved unless logged in to ArcGIS Online.")
            Catch ex As Exception
                MessageBox.Show("Error logging in: " + ex.Message)
            End Try

            Return authenticated
        End Function

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
                '    -close the window 
                '    -decode the parameters (returned as fragments Or query)
                '    -return these parameters as result of the Task
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
End Namespace
