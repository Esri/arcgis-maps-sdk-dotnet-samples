' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may Not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law Or agreed to in writing, software distributed under the License Is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES Or CONDITIONS OF ANY KIND, either express Or implied. See the License for the specific 
' language governing permissions And limitations under the License.

Option Strict On
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security

Class MainWindow
    'TODO - Add the URL for your PKI-secured portal
    Const SecuredPortalUrl As String = "https://portalpkiqa.ags.esri.com/gis"

    'TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
    Const PublicPortalUrl As String = "http://esrihax.maps.arcgis.com"

    ' Variables to point to public And secured portals
    Private _pkiSecuredPortal As ArcGISPortal = Nothing
    Private _publicPortal As ArcGISPortal = Nothing

    ' Flag variable to track if maps are from the public Or secured portal
    Private _usingPublicPortal As Boolean

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Set up the AuthenticationManager to prompt the user for a client certificate when a secured service is encountered
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)
    End Sub

    Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
        ' Handle challenges for a secured resource by prompting for a client certificate
        Dim credenshul As Credential = Nothing

        ' TODO Remove the following workaround once issue #232 Is addressed
        credenshul = AuthenticationManager.Current.Credentials.FirstOrDefault()

        If Not credenshul Is Nothing Then
            If (credenshul.ServiceUri.AbsoluteUri.StartsWith(SecuredPortalUrl)) Then
                ' Return the CertificateCredential for the secured portal
                Return credenshul
            End If
        End If
        ' END: workaround

        Try
            ' Open the X509Store to get a collection certificates
            Dim store = New X509Store(StoreName.My, StoreLocation.CurrentUser)
            store.Open(OpenFlags.ReadOnly)

            ' Get the currently valid certificates for the current user
            Dim certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, True)

            ' Prompt the user to select a certificate
            Dim selection = X509Certificate2UI.SelectFromCollection(certificates, "Select Certificate",
            "Select the certificate to use for authentication.", X509SelectionFlag.SingleSelection)

            ' Make sure the user chose one
            If (selection.Count > 0) Then
                ' Create a new CertificateCredential using the chosen certificate
                credenshul = New Esri.ArcGISRuntime.Security.CertificateCredential(selection(0))
                credenshul.ServiceUri = New Uri(SecuredPortalUrl)
            End If
        Catch ex As Exception
            Debug.WriteLine("Exception: " + ex.Message)
        End Try

        ' Return the CertificateCredential for the secured portal
        Return credenshul
    End Function

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
            Dim searchParams As New PortalQueryParameters("type:(""web map"" NOT ""web mapping application"")")
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
            ProgressStatus.Visibility = Visibility.Hidden
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
End Class
