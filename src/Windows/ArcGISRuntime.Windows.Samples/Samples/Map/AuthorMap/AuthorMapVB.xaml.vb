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
Imports Windows.Graphics.Imaging
Imports Windows.Storage
Imports Windows.Storage.Streams
Imports Windows.UI.Popups

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
            ' Fill the basemap combo box with basemap names
            BasemapListView.ItemsSource = _basemapNames

            ' Fill the operational layers list box with layer names
            LayerListView.ItemsSource = _operationalLayerUrls

            ' Show a plain gray map in the map view
            MyMapView.Map = New Map(Basemap.CreateLightGrayCanvas())

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
            For Each item As KeyValuePair(Of String, String) In LayerListView.SelectedItems
                ' Get the service uri for each selected item 
                Dim layerUri As Uri = New Uri(item.Value)

                ' Create a New map image layer, set it 50% opaque, And add it to the map
                Dim mapServiceLayer As ArcGISMapImageLayer = New ArcGISMapImageLayer(layerUri)
                mapServiceLayer.Opacity = 0.5
                _myMap.OperationalLayers.Add(mapServiceLayer)
            Next
        End Sub

        Private _basemapName As String = String.Empty
        Private Sub BasemapItemClick(sender As Object, e As RoutedEventArgs)
            ' Store the name of the desired basemap when one Is selected
            ' (will be applied to the map view when "Update Map" Is clicked)
            Dim radioBtn As RadioButton = TryCast(sender, RadioButton)
            _basemapName = radioBtn.Content.ToString()
        End Sub

        Private Sub UpdateMap(sender As Object, e As RoutedEventArgs)
            ' Create a New (empty) map
            If _myMap Is Nothing OrElse _myMap.PortalItem Is Nothing Then
                _myMap = New Map()
            End If

            ' Call functions that apply the selected basemap And operational layers
            ApplyBasemap(_basemapName)
            AddOperationalLayers()

            ' Use the current extent to set the initial viewpoint for the map
            _myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

            ' Show the New map in the map view
            MyMapView.Map = _myMap
        End Sub

        Private Async Sub SaveMap(sender As Object, e As RoutedEventArgs)
            ' Make sure the map Is Not null
            If _myMap Is Nothing Then
                Dim dialog = New MessageDialog("Please update the map before saving.", "Map is empty")
                dialog.ShowAsync()
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
                    Dim dialog = New MessageDialog("Saved '" + title + "' to ArcGIS Online!", "Map Saved")
                    dialog.ShowAsync()
                Catch ex As Exception
                    Dim dialog = New MessageDialog("Unable to save map to ArcGIS Online: " + ex.Message)
                    dialog.ShowAsync()
                Finally
                    ' Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Collapsed
                End Try
            Else
                ' This Is an update to the existing portal item
                Try
                    ' Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible

                    ' This Is Not the initial save, call SaveAsync to save changes to the existing portal item
                    Await _myMap.SaveAsync()
                    Dim dialog = New MessageDialog("Saved changes to '" + _myMap.PortalItem.Title + "'", "Updates Saved")
                    dialog.ShowAsync()
                Catch ex As Exception
                    Dim dialog = New MessageDialog("Unable to save map updates: " + ex.Message)
                    dialog.ShowAsync()
                Finally
                    ' Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Collapsed
                End Try
            End If

            UpdatePortalItemThumbnailAsync()
        End Sub


        Private Async Sub UpdatePortalItemThumbnailAsync()
            ' Update the portal item with a thumbnail image of the current map
            Try
                ' Get the map's portal item
                Dim newPortalItem As ArcGISPortalItem = MyMapView.Map.PortalItem

                ' Portal item will be null if the map hasn't been saved
                If newPortalItem Is Nothing Then
                    Throw New Exception("Map has not been saved to the portal")
                End If

                ' Call a function that will create an image from the map              
                Dim imageFileName As String = newPortalItem.Title + ".jpg"
                Await WriteCurrentMapImageAsync(imageFileName)

                ' Open the image file (stored in the device's Pictures folder)
                Dim mapImageFile As StorageFile = Await KnownFolders.PicturesLibrary.GetFileAsync(imageFileName)

                If Not mapImageFile Is Nothing Then
                    ' Get a thumbnail image (scaled down version) of the original
                    Dim thumbnailData As FileProperties.StorageItemThumbnail = Await mapImageFile.GetScaledImageAsThumbnailAsync(0)

                    ' Create a New ArcGISPortalItemContent object to contain the thumbnail image
                    Dim portalItemContent As ArcGISPortalItemContent = New ArcGISPortalItemContent()

                    ' Assign the thumbnail data (stream) to the content object
                    portalItemContent.Thumbnail = thumbnailData.AsStreamForRead()

                    ' Update the portal item with the New content (just the thumbnail will be updated)
                    Await newPortalItem.UpdateAsync(portalItemContent)

                    ' Delete the map image file from disk
                    mapImageFile.DeleteAsync()
                End If
            Catch ex As Exception
                ' Warn the user that the thumbnail could Not be updated
                Dim dialog As MessageDialog = New MessageDialog("Unable to update thumbnail for portal item: " + ex.Message, "Portal Item Thumbnail")
                dialog.ShowAsync()
            End Try
        End Sub

        ' Export the map view And store it in a local file
        Private Async Function WriteCurrentMapImageAsync(imageName As String) As Task
            Try
                ' Export the current map view display to a bitmap
                Dim mapImage As WriteableBitmap = TryCast(Await MyMapView.ExportImageAsync(), WriteableBitmap)

                ' Create a New file in the device's Pictures folder
                Dim outStorageFile As StorageFile = Await KnownFolders.PicturesLibrary.CreateFileAsync(imageName)

                ' Open the New file for read/write
                Using outStream As IRandomAccessStream = Await outStorageFile.OpenAsync(FileAccessMode.ReadWrite)
                    ' Create a bitmap encoder to encode the image to Jpeg
                    Dim encoder As BitmapEncoder = Await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outStream)

                    ' Read the pixels from the map image into a byte array
                    Dim pixelStream As Stream = mapImage.PixelBuffer.AsStream()
                    Dim pixels(pixelStream.Length) As Byte
                    Await pixelStream.ReadAsync(pixels, 0, pixels.Length)

                    ' Use the encoder to write the map image pixels to the output file
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, mapImage.PixelWidth, mapImage.PixelHeight, 96.0, 96.0, pixels)
                    Await encoder.FlushAsync()
                End Using
            Catch
                ' Exception message will be shown in the calling function
                Throw
            End Try
        End Function


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
                Dim dialog = New MessageDialog("Maps cannot be saved unless logged in to ArcGIS Online.")
                dialog.ShowAsync()
            Catch ex As Exception
                Dim dialog = New MessageDialog("Error logging in: " + ex.Message)
                dialog.ShowAsync()
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
End Namespace
