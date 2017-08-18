' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports Esri.ArcGISRuntime.UI
Imports Esri.ArcGISRuntime.UI.Controls
Imports Windows.UI.Popups

Namespace AuthorEditSaveMap
    Partial Public Class AuthorEditSaveMapVB
        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with
        Private Const ArcGISOnlineUrl As String = "https://www.arcgis.com/sharing/rest"

        ' Client ID For an app registered With the server
        Private Const AppClientId As String = "2Gh53JRzkPtOENQq"

        ' URL For redirecting after a successful authorization
        '       Note - this must be a URL configured as a valid Redirect URI with your app
        Private Const OAuthRedirectUrl As String = "https://developers.arcgis.com"

        ' Gets the view-model that provides mapping capabilities to the view
        Private _viewModel As MapViewModel = New MapViewModel()
        Public ReadOnly Property MyViewModel As MapViewModel
            Get
                Return _viewModel
            End Get
        End Property

        Public Sub New()
            InitializeComponent()

            ' Pass the map view to the map view model
            MyViewModel.AppMapView = MyMapView

            ' Define a handler for selection changed on the basemap list
            AddHandler BasemapListBox.SelectionChanged, AddressOf OnBasemapsClicked

            ' Define a handler for the Save Map click
            AddHandler SaveMapButton.Click, AddressOf OnSaveMapClick

            ' Call a function to update the authentication manager settings
            UpdateAuthenticationManager()
        End Sub

        Private Sub OnBasemapsClicked(sender As Object, e As SelectionChangedEventArgs)
            ' Get the text (basemap name) selected in the list box
            Dim basemapName As String = e.AddedItems(0).ToString()

            ' Pass the basemap name to the view model method to change the basemap
            MyViewModel.ChangeBasemap(basemapName)

            ' Hide the basemaps flyout
            flyguy.Hide()
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
                Dim tags As String() = TagsTextBox.Text.Split(",")

                ' Return if the text Is null Or empty
                If (String.IsNullOrEmpty(title) Or String.IsNullOrEmpty(description)) Then
                    Return
                End If

                ' Get current map extent (viewpoint) for the map initial extent
                Dim currentViewpoint As Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

                ' Export the current map view as the item thumbnail
                Dim thumbnailImg As RuntimeImage = Await MyMapView.ExportImageAsync()

                ' See if the map has already been saved
                If (Not MyViewModel.MapIsSaved) Then
                    ' Call the SaveNewMapAsync method on the view model, pass in the required info
                    Await MyViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags, thumbnailImg)

                    ' Report success
                    Dim dialog As New MessageDialog("Map '" + title + "' was saved to the portal.", "Saved Map")
                    dialog.ShowAsync()
                Else
                    ' Map has previously been saved as a portal item, update it (title And description will remain the same)
                    MyViewModel.UpdateMapItem()

                    ' Report success
                    Dim dialog As New MessageDialog("Changes to '" + title + "' were updated to the portal.")
                    dialog.ShowAsync()
                End If
            Catch ex As OperationCanceledException
                ' Report canceled login
                Dim dialog As New MessageDialog("Login to the portal was canceled.", "Save canceled")
                dialog.ShowAsync()
            Catch ex As Exception
                ' Report error
                Dim dialog As New MessageDialog("Error while saving: " + ex.Message, "Cannot save")
                dialog.ShowAsync()
            End Try
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

    ' Provides map data to an application
    ' Note: in a ArcGIS Runtime for .NET (C#) template project, this class will be in a separate file: "MapViewModel.cs"
    Public Class MapViewModel
        Implements INotifyPropertyChanged

        ' Store the map view used by the app
        Private _mapView As MapView
        Public WriteOnly Property AppMapView As MapView
            Set
                _mapView = Value
            End Set
        End Property

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

        ' Create a default map with the vector streets basemap
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
        Public Async Function SaveNewMapAsync(initialViewpoint As Viewpoint, title As String, description As String, tags As String(), img As RuntimeImage) As Task
            ' Get the ArcGIS Online portal 
            Dim agsOnline As ArcGISPortal = Await ArcGISPortal.CreateAsync(New Uri("https://www.arcgis.com/sharing/rest"))

            ' Set the map's initial viewpoint using the extent (viewpoint) passed in
            _map.InitialViewpoint = initialViewpoint

            ' Save the current state of the map as a portal item in the user's default folder
            Await MyMap.SaveAsAsync(agsOnline, Nothing, title, description, tags, img)
        End Function

        Public ReadOnly Property MapIsSaved As Boolean
            ' Return True if the current map has a value for the Item property
            Get
                Return (Not _map Is Nothing AndAlso Not _map.Item Is Nothing)
            End Get
        End Property

        Public Async Sub UpdateMapItem()
            ' Save the map
            Await _map.SaveAsync()

            ' Export the current map view as the item thumbnail
            Dim thumbnailImg As RuntimeImage = Await _mapView.ExportImageAsync()

            ' Get the file stream from the New thumbnail image
            Dim imageStream As Stream = Await thumbnailImg.GetEncodedBufferAsync()

            ' Update the item thumbnail
            Dim portalMapItem As PortalItem = TryCast(MyMap.Item, PortalItem)
            portalMapItem.SetThumbnailWithImage(imageStream)
        End Sub

        ' Raises the PropertyChanged event for a property
        Protected Sub OnPropertyChanged(<CallerMemberName> Optional ByVal propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    End Class
End Namespace
