' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may Not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law Or agreed to in writing, software distributed under the License Is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES Or CONDITIONS OF ANY KIND, either express Or implied. See the License for the specific 
' language governing permissions And limitations under the License.

Option Strict On
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports System
Imports System.Linq
Imports System.Text
Imports Windows.Security.Cryptography.Certificates
Imports Windows.Storage
Imports Windows.Storage.Pickers
Imports Windows.Storage.Streams
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls

' Important:
'    You must add the "Private Networks" capability to use Public Key Infrastructure (PKI) authentication
'    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
'    in your project's Package.appxmanifest file.

Public NotInheritable Class MainPage
    Inherits Page

    'TODO - Add the URL for your PKI-secured portal
    Const SecuredPortalUrl As String = "https://my.secure.portal.com/gis"

    'TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
    Const PublicPortalUrl As String = "http://esrihax.maps.arcgis.com"

    ' Variable to hold certificate information as an encrypted string
    Private _certificateString As String = String.Empty

    ' Display name to use for the certificate
    Private _certificateName As String = String.Empty

    ' Variables to point to public And secured portals
    Private _pkiSecuredPortal As ArcGISPortal = Nothing
    Private _publicPortal As ArcGISPortal = Nothing

    ' Flag variable to track if maps are from the public Or secured portal
    Private _usingPublicPortal As Boolean

    Private Async Sub ChooseCertificateFile(sender As Object, e As RoutedEventArgs)
        ' Create a file picker dialog so the user can select an exported certificate (*.pfx)
        Dim pfxFilePicker As FileOpenPicker = New FileOpenPicker()
        pfxFilePicker.FileTypeFilter.Add(".pfx")
        pfxFilePicker.CommitButtonText = "Open"

        ' Show the dialog And get the selected file (if any)
        Dim pfxFile As StorageFile = Await pfxFilePicker.PickSingleFileAsync()
        If Not pfxFile Is Nothing Then
            ' Use the file's display name for the certificate name
            _certificateName = pfxFile.DisplayName

            ' Read the contents of the file
            Dim fileBuffer As IBuffer = Await FileIO.ReadBufferAsync(pfxFile)
            Using reader As DataReader = DataReader.FromBuffer(fileBuffer)
                ' Store the contents of the file as an encrypted string
                ' The string will be imported as a certificate when the user enters the password
                Dim bytes(Convert.ToInt32(fileBuffer.Length - 1)) As Byte
                reader.ReadBytes(bytes)
                _certificateString = Convert.ToBase64String(bytes)
            End Using

            ' Show the certificate password box (and hide the map search controls)
            LoginPanel.Visibility = Visibility.Visible
            LoadMapPanel.Visibility = Visibility.Collapsed
        End If
    End Sub

    ' Load a client certificate for accessing a PKI-secured server 
    Private Async Sub LoadClientCertButton_Click(sender As Object, e As RoutedEventArgs)
        ' Show the progress bar And a message
        ProgressStatus.Visibility = Visibility.Visible
        MessagesTextBlock.Text = "Loading certificate ..."

        Try
            ' Import the certificate by providing 
            ' -the encoded certificate string, 
            ' -the password (entered by the user)
            ' -certificate options (export, key protection, install)
            ' -a friendly name (the name of the pfx file)
            Await CertificateEnrollmentManager.ImportPfxDataAsync(
                _certificateString,
                CertPasswordBox.Password,
                ExportOption.Exportable,
                KeyProtectionLevel.NoConsent,
                InstallOptions.None,
                _certificateName)

            ' Report success
            MessagesTextBlock.Text = "Client certificate (" + _certificateName + ") was successfully imported"
        Catch ex As Exception
            ' Report error
            MessagesTextBlock.Text = "Error loading certificate: " + ex.Message
        Finally
            ' Hide progress bar And the password controls
            ProgressStatus.Visibility = Visibility.Collapsed
            HideCertLogin(Nothing, Nothing)
        End Try
    End Sub

    ' Search the public portal for web maps and display the results in a list box
    Private Async Sub SearchPublicMapsButton_Click(sender As Object, e As RoutedEventArgs)
        ' Set the flag variable to indicate this Is the public portal
        ' (if the user wants to load a map, will need to know which portal it came from)
        _usingPublicPortal = True

        Try
            ' Create an instance of the public portal
            _publicPortal = Await ArcGISPortal.CreateAsync(New Uri(PublicPortalUrl))

            ' Call a sub to search the portal
            SearchPortal(_publicPortal)
        Catch ex As Exception
            ' Report errors connecting to the public portal
            MessagesTextBlock.Text = ex.Message
        End Try
    End Sub

    ' Search the PKI-secured portal for web maps and display the results in a list box
    Private Async Sub SearchSecureMapsButton_Click(sender As Object, e As RoutedEventArgs)
        ' Set the flag variable to indicate this is the secured portal
        ' (if the user wants to load a map, will need to know which portal it came from)
        _usingPublicPortal = False

        Try
            ' Create an instance of the secure portal
            _pkiSecuredPortal = Await ArcGISPortal.CreateAsync(New Uri(SecuredPortalUrl))

            ' Call a sub to search the portal
            SearchPortal(_pkiSecuredPortal)
        Catch ex As Exception
            ' Report errors connecting to the secured portal
            MessagesTextBlock.Text = ex.Message
        End Try
    End Sub

    Private Async Sub SearchPortal(currentPortal As ArcGISPortal)
        ' Show status message and the status bar
        MessagesTextBlock.Text = "Searching for web map items on portal at " + currentPortal.Uri.AbsoluteUri
        ProgressStatus.Visibility = Visibility.Visible

        ' Clear the map list
        MapItemListBox.Items.Clear()
        Dim messageBuilder As StringBuilder = New StringBuilder()

        Try
            ' Report connection info
            messageBuilder.AppendLine("Connected to the portal on " + currentPortal.Uri.Host)

            ' Report the user name used for this connection
            If Not currentPortal.User Is Nothing Then
                messageBuilder.AppendLine("Connected as: " + currentPortal.User.UserName)
            Else
                ' Connected anonymously
                messageBuilder.AppendLine("Anonymous")
            End If

            ' Search the portal for web maps
            Dim searchParams As PortalQueryParameters = New PortalQueryParameters("type:(""web map"" NOT ""web mapping application"")")
            Dim items As PortalQueryResultSet(Of PortalItem) = Await currentPortal.FindItemsAsync(searchParams)

            ' Build a list of items from the results that shows the map name and stores the item ID (with the Tag property)
            Dim resultItems As IEnumerable(Of ListBoxItem) = From r In items.Results Select New ListBoxItem With {.Tag = r.ItemId, .Content = r.Title}

            ' Add the list items
            For Each itm As ListBoxItem In resultItems
                MapItemListBox.Items.Add(itm)
            Next
        Catch ex As Exception
            ' Report errors searching the portal
            messageBuilder.AppendLine(ex.Message)
        Finally
            ' Show messages, hide progress bar
            MessagesTextBlock.Text = messageBuilder.ToString()
            ProgressStatus.Visibility = Visibility.Collapsed
        End Try
    End Sub

    Private Async Sub AddMapItem_Click(sender As Object, e As RoutedEventArgs)
        ' Get a web map from the selected portal item And display it in the app
        If MapItemListBox.SelectedItem Is Nothing Then Return

        ' Clear status messages
        MessagesTextBlock.Text = String.Empty

        Dim messageBuilder = New StringBuilder()

        Try
            ' See if this is the public or secured portal and get the appropriate object reference
            Dim portal As ArcGISPortal = Nothing

            If _usingPublicPortal Then
                portal = _publicPortal
            Else
                portal = _pkiSecuredPortal
            End If

            ' Throw an exception if the portal is nothing
            If portal Is Nothing Then
                Throw New Exception("Portal has not been instantiated.")
            End If

            ' Get the portal item ID from the selected list box item (read it from the Tag property)
            Dim itemId As String = TryCast(MapItemListBox.SelectedItem, ListBoxItem).Tag.ToString()

            ' Use the item ID to create an ArcGISPortalItem from the appropriate portal 
            Dim portalItem As PortalItem = Await PortalItem.CreateAsync(portal, itemId)

            ' Create a Map from the portal item (all items in the list represent web maps)
            Dim webMap As Map = New Map(portalItem)

            ' Display the Map in the map view
            MyMapView.Map = webMap

            ' Report success
            messageBuilder.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host)
        Catch ex As Exception
            ' Add an error message
            messageBuilder.AppendLine("Error accessing web map: " + ex.Message)
        Finally
            ' Show messages
            MessagesTextBlock.Text = messageBuilder.ToString()
        End Try
    End Sub


    Private Sub HideCertLogin(sender As Object, e As RoutedEventArgs)
        ' Hide the certificate password box (And show the map search controls)
        LoginPanel.Visibility = Visibility.Collapsed
        LoadMapPanel.Visibility = Visibility.Visible
    End Sub
End Class
