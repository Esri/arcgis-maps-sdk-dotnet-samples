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
Imports Esri.ArcGISRuntime.UI
Imports Windows.Graphics.Imaging
Imports Windows.Storage
Imports Windows.Storage.FileProperties
Imports Windows.Storage.Streams
Imports Windows.UI.Popups

Namespace AuthorMap
    Partial Public Class AuthorMapVB
        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with
        Private ServerUrl As String = "https://www.arcgis.com/sharing/rest"

        ' TODO: Add Client ID For an app registered With the server
        Private AppClientId As String = "2Gh53JRzkPtOENQq"

        ' TODO: Add URL For redirecting after a successful authorization
        '       Note - this must be a URL configured as a valid Redirect URI with your app
        Private OAuthRedirectUrl As String = "https://developers.arcgis.com"

        ' String array to store names of the available basemaps
        Private _basemapNames As String() =
        {
            "Light Gray",
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

            ' When the map view loads, show a dialog for entering OAuth settings
            AddHandler MyMapView.Loaded, AddressOf ShowOAuthSettingsDialog

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

            ' Update the extent labels whenever the view point (extent) changes
            AddHandler MyMapView.ViewpointChanged, AddressOf UpdateViewExtentLabels
        End Sub

#Region "UI event handlers"
        Private Sub LayerSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Call a sub to add operational layers to the map
            AddOperationalLayers()
        End Sub
#End Region

        Private Sub ApplyBasemap(basemapName As String)
            ' Set the basemap for the map according to the user's choice in the list box
            Dim myMap As Map = MyMapView.Map
            Select Case basemapName
                Case "Light Gray"
                    ' Set the basemap to Light Gray Canvas
                    myMap.Basemap = Basemap.CreateLightGrayCanvas()
                Case "Topographic"
                    ' Set the basemap to Topographic
                    myMap.Basemap = Basemap.CreateTopographic()
                Case "Streets"
                    ' Set the basemap to Streets
                    myMap.Basemap = Basemap.CreateStreets()
                Case "Imagery"
                    ' Set the basemap to Imagery
                    myMap.Basemap = Basemap.CreateImagery()
                Case "Ocean"
                    ' Set the basemap to Oceans
                    myMap.Basemap = Basemap.CreateOceans()
                Case Else
            End Select
        End Sub

        Private Sub AddOperationalLayers()
            ' Get the current map
            Dim myMap As Map = MyMapView.Map

            ' Loop through the selected items in the operational layers list box
            For Each item As KeyValuePair(Of String, String) In LayerListView.SelectedItems
                ' Get the service uri for each selected item 
                Dim layerUri As Uri = New Uri(item.Value)

                ' Create a New map image layer, set it 50% opaque, And add it to the map
                Dim mapServiceLayer As ArcGISMapImageLayer = New ArcGISMapImageLayer(layerUri)
                mapServiceLayer.Opacity = 0.5
                myMap.OperationalLayers.Add(mapServiceLayer)
            Next
        End Sub

        Private Sub BasemapItemClick(sender As Object, e As RoutedEventArgs)
            ' Get the name of the desired basemap 
            Dim radioBtn As RadioButton = TryCast(sender, RadioButton)
            Dim basemapName As String = radioBtn.Content.ToString()

            ' Apply the basemap to the current map
            ApplyBasemap(basemapName)
        End Sub

        Private Async Sub SaveMapClicked(sender As Object, e As RoutedEventArgs)
            Try
                ' Don't attempt to save if the OAuth settings weren't provided
                If (String.IsNullOrEmpty(AppClientId) Or String.IsNullOrEmpty(OAuthRedirectUrl)) Then
                    Dim dialog = New MessageDialog("OAuth settings were not provided.", "Cannot Save")
                    Await dialog.ShowAsync()

                    SaveMapFlyout.Hide()

                    Return
                End If

                ' Show the progress bar so the user knows work is happening
                SaveProgressBar.Visibility = Visibility.Visible

                ' Get the current map
                Dim myMap As Map = MyMapView.Map

                ' Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

                ' See if the map has already been saved (has an associated portal item)
                If myMap.Item Is Nothing Then
                    ' Get information for the New portal item
                    Dim title As String = TitleTextBox.Text
                    Dim description As String = DescriptionTextBox.Text
                    Dim tags As String() = TagsTextBox.Text.Split(",".ToCharArray())
                    ' Make sure all required info was entered
                    If String.IsNullOrEmpty(title) Or String.IsNullOrEmpty(description) Or tags.Length = 0 Then
                        Throw New Exception("Please enter a title, description, and some tags to describe the map.")
                    End If

                    ' Call a function to save the map as a new portal item
                    Await SaveNewMapAsync(myMap, title, description, tags)

                    ' Report a successful save
                    Dim dialog = New MessageDialog("Saved '" + title + "' to ArcGIS Online!", "Map Saved")
                    Await dialog.ShowAsync()
                Else
                    ' This is not the initial save, call SaveAsync to save changes to the existing portal item
                    Await myMap.SaveAsync()

                    ' Report update was successful
                    Dim dialog As MessageDialog = New MessageDialog("Saved changes to '" + myMap.Item.Title + "'", "Updates Saved")
                    Await dialog.ShowAsync
                End If

                ' Update the portal item thumbnail with the current map image
                Try
                    ' Export the current map view
                    Dim mapImage As ImageSource = Await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(Await MyMapView.ExportImageAsync())

                    ' Call a function that writes a temporary jpeg file of the map
                    Dim imagePath As String = Await WriteTempThumbnailImageAsync(mapImage)

                    ' Call a sub to update the portal item's thumbnail with the image
                    UpdatePortalItemThumbnailAsync(imagePath)
                Catch
                    ' Throw an exception to let the user know the thumbnail was Not saved (the map item was)
                    Throw New Exception("Thumbnail was not updated.")
                End Try
            Catch ex As Exception
                ' Report error message
                Dim dialog As MessageDialog = New MessageDialog("Error saving map to ArcGIS Online: " + ex.Message)
                dialog.ShowAsync()
            Finally
                ' Hide the progress bar
                SaveProgressBar.Visibility = Visibility.Collapsed
            End Try
        End Sub

        Private Async Function SaveNewMapAsync(myMap As Map, title As String, description As String, tags As String()) As Task
            ' Challenge the user for portal credentials (OAuth credential request for arcgis.com)
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
                Await thisAuthenticationManager.GetCredentialAsync(loginInfo, False)
            Catch canceledEx As OperationCanceledException
                ' user canceled the login
                Throw New Exception("Portal log in was canceled.")
            End Try

            ' Get the ArcGIS Online portal
            Dim agsOnline As ArcGISPortal = Await ArcGISPortal.CreateAsync()

            ' Export the current map view as the item thumbnail
            Dim img As RuntimeImage = Await MyMapView.ExportImageAsync()

            ' Save the current state of the map as a portal item in the user's default folder
            Await myMap.SaveAsAsync(agsOnline, Nothing, title, description, tags, img)
        End Function

        Private Async Function UpdatePortalItemThumbnailAsync(imageFileName As String) As Task
            ' Update the portal item with a thumbnail image of the current map
            Try
                ' Get the map's portal item
                Dim newPortalItem As PortalItem = MyMapView.Map.Item

                ' Open the image file (stored in the device's Pictures folder)
                Dim mapImageFile As StorageFile = Await KnownFolders.PicturesLibrary.GetFileAsync(imageFileName)

                If Not mapImageFile Is Nothing Then
                    ' Get a thumbnail image (scaled down version) of the original
                    Dim thumbnailData As StorageItemThumbnail = Await mapImageFile.GetScaledImageAsThumbnailAsync(0)

                    ' Assign the thumbnail data (file stream) to the content object
                    newPortalItem.SetThumbnailWithImage(thumbnailData.AsStreamForRead())

                    ' Update the portal item with the new content (just the thumbnail will be updated)
                    Await newPortalItem.UpdateItemPropertiesAsync()

                    ' Delete the map image file from disk
                    Await mapImageFile.DeleteAsync()
                End If
            Catch ex As Exception
                ' Warn the user that the thumbnail could Not be updated
                Dim dialog As MessageDialog = New MessageDialog("Unable to update thumbnail for portal item: " + ex.Message, "Portal Item Thumbnail")
                dialog.ShowAsync()
            End Try
        End Function

        Private Async Function WriteTempThumbnailImageAsync(mapImageSource As ImageSource) As Task(Of String)
            Dim outputFilename As String = String.Empty

            Try
                ' Export the current map view display to a bitmap
                Dim mapImage As WriteableBitmap = TryCast(mapImageSource, WriteableBitmap)

                ' Create a New file in the device's Pictures folder
                Dim outStorageFile As StorageFile = Await KnownFolders.PicturesLibrary.CreateFileAsync("MapImage_Temp.jpg", CreationCollisionOption.GenerateUniqueName)
                outputFilename = outStorageFile.Name

                ' Open the New file for read/write
                Using stream As IRandomAccessStream = Await outStorageFile.OpenAsync(FileAccessMode.ReadWrite)
                    ' Create a bitmap encoder to encode the image to Jpeg
                    Dim encoder As BitmapEncoder = Await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream)

                    ' Read the pixels from the map image into a byte array
                    Dim pixelStream As Stream = mapImage.PixelBuffer.AsStream()
                    Dim pixels(pixelStream.Length) As Byte
                    Await pixelStream.ReadAsync(pixels, 0, pixels.Length)

                    ' Use the encoder to write the map image pixels to the output file
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, mapImage.PixelWidth, mapImage.PixelHeight, 96.0, 96.0, pixels)
                    Await encoder.FlushAsync()
                End Using
            Catch ex As Exception
                ' Exception message will be shown in the calling function
                Throw
            End Try

            Return outputFilename
        End Function

        Private Sub ClearMapClicked(sender As Object, e As RoutedEventArgs)
            ' Create a new map (will not have an associated PortalItem)
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

        Private Async Sub ShowOAuthSettingsDialog()
            ' Show default settings for client ID And redirect URL
            ClientIdTextBox.Text = AppClientId
            RedirectUrlTextBox.Text = OAuthRedirectUrl

            ' Display inputs for a client ID And redirect URL to use for OAuth authentication
            Dim result As ContentDialogResult = Await OAuthSettingsDialog.ShowAsync()
            If (result = ContentDialogResult.Primary) Then
                ' Settings were provided, update the configuration settings for OAuth authorization
                AppClientId = ClientIdTextBox.Text.Trim()
                OAuthRedirectUrl = RedirectUrlTextBox.Text.Trim()

                ' Update authentication manager with the OAuth settings
                UpdateAuthenticationManager()
            Else
                ' User canceled, warn that won't be able to save
                Dim messageDlg = New MessageDialog("No OAuth settings entered, you will not be able to save your map.")
                Await messageDlg.ShowAsync()

                AppClientId = String.Empty
                OAuthRedirectUrl = String.Empty
            End If
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
