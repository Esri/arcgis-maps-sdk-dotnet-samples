' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Text
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports Esri.ArcGISRuntime.UI.Controls

' Important
'    You must add the "Private Networks" capability to use Integrated Windows Authentication (IWA)
'    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
'    in your project's Package.appxmanifest file.

Public NotInheritable Class MainPage
    Inherits Page
    ' Note: The Universal Windows Platform handles challenging For Windows credentials.
    '       You do not need to surface your own UI to prompt the user for username, password, and domain.

    ' TODO - Add the URL for your IWA-secured portal
    Const SecuredPortalUrl As String = "https://my.secure.server.com/gis/sharing"

    ' TODO - Add the URL for a portal containing public content (your ArcGIS Online Organization, e.g.)
    Const PublicPortalUrl As String = "http://www.arcgis.com/sharing/rest"

    ' TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
    Const NetworkUsername As String = ""
    Const NetworkPassword As String = ""
    Const NetworkDomain As String = ""

    ' Variables to point to public And secured portals
    Private _iwaSecuredPortal As ArcGISPortal
    Private _publicPortal As ArcGISPortal

    ' Flag variable to track if the user is looking at maps from the public or secured portal
    Dim _usingPublicPortal As Boolean

    Public Sub New()
        InitializeComponent()

        ' Call a function to set up the AuthenticationManager and add a hard-coded credential (if defined)
        Initialize()
    End Sub

    Public Sub Initialize()
        ' Note unlike a WPF app, your current system credentials will not be used by default in a UWP app and
        '       you will be (initially) challenged even for resources to which your system account has access.
        '       Once you provide your credentials, you will not be challenged again for them

        ' Check for hard-coded username, password, and domain values
        If (Not String.IsNullOrEmpty(NetworkUsername) AndAlso
                Not String.IsNullOrEmpty(NetworkPassword) AndAlso
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
    Private Async Sub SearchSecureMapsClick(sender As Object, e As RoutedEventArgs)
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
            ProgressStatus.Visibility = Visibility.Collapsed
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
            Dim portalItm As PortalItem = Await PortalItem.CreateAsync(portal, itemId)

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