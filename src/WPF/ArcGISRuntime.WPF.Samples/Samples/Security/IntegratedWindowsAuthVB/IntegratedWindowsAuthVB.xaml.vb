' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports System.Text
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.UI.Controls

Class MainWindow
    ' TODO - Add the URL for your IWA-secured portal
    Const SecuredPortalUrl As String = "https://my.secure.server.com/gis/sharing"

    ' TODO - Add the URL for a portal containing public content (your ArcGIS Online Organization, e.g.)
    Const PublicPortalUrl As String = "http://www.arcgis.com/sharing/rest"

    ' TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
    ' Note: adding bogus credential info can provide a way To verify unauthorized users will be challenged For a log In
    Const NetworkUsername As String = ""
    Const NetworkPassword As String = ""
    Const NetworkDomain As String = ""

    ' Variables to point to public And secured portals
    Private _iwaSecuredPortal As ArcGISPortal
    Private _publicPortal As ArcGISPortal

    ' Flag variable to track if the user Is looking at maps from the public Or secured portal
    Dim _usingPublicPortal As Boolean

    ' Flag to track if the user has canceled the login dialog
    Dim _canceledLogin As Boolean

    Public Sub New()
        InitializeComponent()

        ' Call a function to set up the AuthenticationManager and add a hard-coded credential (if defined)
        Initialize()
    End Sub

    Private Sub Initialize()
        ' Define a challenge handler method for the AuthenticationManager 
        ' (this method handles getting credentials when a secured resource Is encountered)
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)

        ' Note For IWA - secured services, your current system credentials will be used by default And you will only
        '       be challenged for resources to which your system account doesn't have access

        ' Check for hard-coded username, password, And domain values
        If (Not String.IsNullOrEmpty(NetworkUsername) And
                Not String.IsNullOrEmpty(NetworkPassword) And
                Not String.IsNullOrEmpty(NetworkDomain)) Then

            ' Create a hard-coded network credential (other than the one that started the app, in other words)
            Dim hardcodedCredential As ArcGISNetworkCredential = New ArcGISNetworkCredential With
                {
                    .Credentials = New System.Net.NetworkCredential(NetworkUsername, NetworkPassword, NetworkDomain),
                    .ServiceUri = New Uri(SecuredPortalUrl)
                }

            ' Add the credential to the AuthenticationManager and report that a non-default credential is being used
            AuthenticationManager.Current.AddCredential(hardcodedCredential)
            MessagesTextBlock.Text = "Using credentials for user '" + NetworkUsername + "'"
        End If
    End Sub

    ' Delegate to handle calling authorization logic on the UI thread
    Delegate Function AuthorizeDelegate()

    ' Prompt the user for a credential if unauthorized access to a secured resource Is attempted
    Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
        Dim credenshul As Credential = Nothing
        Try
            ' Call a function to show the login controls, make sure it runs on the UI thread for this app 
            Dim thisDispatcher As Threading.Dispatcher = Application.Current.Dispatcher
            Dim authorizeOnUIDelegate As AuthorizeDelegate = New AuthorizeDelegate _
            (Function() As Credential
                 Dim cred As ArcGISNetworkCredential = Nothing

                 ' Exit if the user clicked "Cancel" in the login window
                 ' (if the user can't provide credentials for a resource they will continue to be challenged)
                 If (_canceledLogin) Then
                     _canceledLogin = False
                     Return Nothing
                 End If

                 ' Create a new login window
                 Dim win As LoginWindow = New LoginWindow()
                 win.Owner = Me

                 ' Show the window to get user input (if canceled, false Is returned)
                 _canceledLogin = (win.ShowDialog() = False)

                 If Not _canceledLogin Then
                     ' Get the credential information provided
                     Dim username As String = win.UsernameTextBox.Text
                     Dim password As String = win.PasswordTextBox.Password
                     Dim domain As String = win.DomainTextBox.Text

                     ' Create a New network credential using the user input And the URI of the resource
                     cred = New ArcGISNetworkCredential()
                     cred.Credentials = New System.Net.NetworkCredential(username, password, domain)
                     cred.ServiceUri = info.ServiceUri
                 End If

                 ' Return the credential
                 Return cred
             End Function)
            credenshul = thisDispatcher.Invoke(authorizeOnUIDelegate)
        Catch ex As Exception
            Debug.WriteLine("Exception: " + ex.Message)
        End Try

        ' Add the credential to the AuthenticationManager
        AuthenticationManager.Current.AddCredential(credenshul)

        ' Return the credential
        Return credenshul
    End Function

    ' Search the public portal for web maps And display the results in a list box.
    Private Async Sub SearchPublicMapsClick(sender As Object, e As RoutedEventArgs)
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

    'Search the IWA-secured portal for web maps And display the results in a list box.        
    Private Async Sub SearchSecureMapsButtonClick(sender As Object, e As RoutedEventArgs)
        ' Set the flag variable to indicate this is the secured portal
        ' (if the user wants to load a map, will need to know which portal it came from)
        _usingPublicPortal = False

        Try
            ' Create an instance of the secure portal
            _iwaSecuredPortal = Await ArcGISPortal.CreateAsync(New Uri(SecuredPortalUrl))

            ' Call a sub to search the portal
            SearchPortal(_iwaSecuredPortal)
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
            messageBuilder.AppendLine("Version: " + currentPortal.CurrentVersion)

            ' Report the user name used for this connection
            If Not currentPortal.CurrentUser Is Nothing Then
                messageBuilder.AppendLine("Connected as: " + currentPortal.CurrentUser.UserName)
            Else
                ' Connected anonymously
                messageBuilder.AppendLine("Anonymous")
            End If

            ' Search the portal for web maps
            Dim items As SearchResultInfoItems = Await currentPortal.SearchItemsAsync(New SearchParameters("type:(""web map"" NOT ""web mapping application"")"))

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

    Private Async Sub AddMapItemClick(sender As Object, e As RoutedEventArgs)
        'Get a web map from the selected portal item And display it in the app
        If MapItemListBox.SelectedItem Is Nothing Then Return

        'Clear status messages
        MessagesTextBlock.Text = String.Empty

        'Store status (Or errors) when adding the map
        Dim statusInfo = New StringBuilder()

        Try
            'Clear the current MapView control from the app
            MyMapGrid.Children.Clear()

            'See if using the public Or secured portal  get the appropriate object reference
            Dim portal As ArcGISPortal = Nothing
            If _usingPublicPortal Then
                portal = _publicPortal
            Else
                portal = _iwaSecuredPortal
            End If

            'Throw an exception if the portal Is null
            If portal Is Nothing Then
                Throw New Exception("Portal has not been instantiated.")
            End If

            'Get the portal item ID from the selected list box item (read it from the Tag property)
            Dim itemId = TryCast(MapItemListBox.SelectedItem, ListBoxItem).Tag.ToString()

            'Use the item ID to create an ArcGISPortalItem from the appropriate portal 
            Dim portalItm = Await PortalItem.CreateAsync(portal, itemId)

            If Not portalItm Is Nothing Then
                'Create a Map using the web map (portal item)
                Dim webMap As Map = New Map(portalItm)

                'Create a New MapView control to display the Map
                Dim myMapView As MapView = New MapView()
                myMapView.Map = webMap

                'Add the MapView to the app
                MyMapGrid.Children.Add(myMapView)
            End If

            'Report success
            statusInfo.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host)
        Catch ex As Exception
            'Add an error message
            statusInfo.AppendLine("Error accessing web map: " + ex.Message)
        Finally
            'Show messages
            MessagesTextBlock.Text = statusInfo.ToString()
        End Try
    End Sub

End Class
