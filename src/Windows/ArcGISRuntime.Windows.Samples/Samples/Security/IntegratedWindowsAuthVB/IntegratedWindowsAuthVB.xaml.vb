' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Net
Imports System.Text
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports Esri.ArcGISRuntime.UI
Imports Windows.ApplicationModel.Core
Imports Windows.UI.Core

Public NotInheritable Class MainPage
    Inherits Page
    ' TODO - Add the URL for your IWA-secured portal
    Const SecuredPortalUrl As String = "https://my.secure.server.com/gis/sharing"

    ' TODO - Add the URL for a portal containing public content (your ArcGIS Online Organization, e.g.)
    Const PublicPortalUrl As String = "http://www.arcgis.com/sharing/rest"

    ' TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
    ' Note: adding bogus credential info can provide a way to verify unauthorized users will be challenged For a log in
    Const NetworkUsername As String = ""
    Const NetworkPassword As String = ""
    Const NetworkDomain As String = ""

    ' Variables to point to public And secured portals
    Private _iwaSecuredPortal As ArcGISPortal
    Private _publicPortal As ArcGISPortal

    ' Flag variable to track if the user is looking at maps from the public or secured portal
    Dim _usingPublicPortal As Boolean

    ' Variable to store the result of the login task
    Private _loginTaskCompletionSrc As TaskCompletionSource(Of Credential)

    Public Sub New()
        InitializeComponent()

        ' Call a function to set up the AuthenticationManager and add a hard-coded credential (if defined)
        Initialize()
    End Sub

    Public Sub Initialize()
        ' Define a challenge handler method for the AuthenticationManager 
        ' (this method handles getting credentials when a secured resource Is encountered)
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)

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

            ' Add the credential to the AuthenticationManager And report that a non-default credential Is being used
            AuthenticationManager.Current.AddCredential(hardcodedCredential)
            MessagesTextBlock.Text = "Using credentials for user '" + NetworkUsername + "'"
        End If
    End Sub

    Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
        ' Prompting the user must happen on the UI thread, use Dispatcher if necessary
        Dim myDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher

        ' If no dispatcher, call the ChallengeUI method directly to get user input
        If myDispatcher Is Nothing Then
            Return Await ChallengeUI(info)
        Else
            ' Use the dispatcher to show the login panel on the UI thread, then await completion of the task
            Await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                Async Sub()
                    Try
                        ' Call the method that shows the login panel And creates a credential 
                        Await ChallengeUI(info)
                    Catch tce As TaskCanceledException
                        ' The user clicked the "Cancel" button, login panel will close
                    End Try
                End Sub)

            ' return the task
            Return Await _loginTaskCompletionSrc.Task
        End If
    End Function

    Public Async Function ChallengeUI(info As CredentialRequestInfo) As Task(Of Credential)
        Try
            ' Create a New instance of LoginInfo (defined in this project) to store credential info
            Dim loginInfo As LoginInfo = New LoginInfo(info)

            ' Set the login panel data context with the LoginInfo object
            ' (two-way binding will provide access to the data entered by the user)
            LoginPanel.DataContext = loginInfo

            ' Show the login UI And hide the load map UI
            LoginPanel.Visibility = Visibility.Visible
            LoadMapPanel.Visibility = Visibility.Collapsed

            ' Create a New TaskCompletionSource for the login operation
            ' (passing the loginInfo helper to the constructor will make it available from the Task's AsyncState property) 
            _loginTaskCompletionSrc = New TaskCompletionSource(Of Credential)(loginInfo)

            ' Return the login task, result will be ready when completed (user provides login info And clicks the "Login" button)
            Return Await _loginTaskCompletionSrc.Task
        Finally
            ' The user Is done logging in (Or canceled); hide the login UI, show the load map UI
            LoginPanel.Visibility = Visibility.Collapsed
            LoadMapPanel.Visibility = Visibility.Visible
        End Try
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
            Dim items As SearchResultInfo(Of ArcGISPortalItem) = Await currentPortal.SearchItemsAsync(New SearchParameters("type:(""web map"" NOT ""web mapping application"")"))

            ' Build a list of items from the results that shows the map name and stores the item ID (with the Tag property)
            Dim resultItems As IEnumerable(Of ListBoxItem) = From r In items.Results Select New ListBoxItem With {.Tag = r.Id, .Content = r.Title}

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
            Dim portalItem = Await ArcGISPortalItem.CreateAsync(portal, itemId)

            If Not portalItem Is Nothing Then
                'Create a Map using the web map (portal item)
                Dim webMap As Map = New Map(portalItem)

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

    Private Sub LoginButtonClick(sender As Object, e As RoutedEventArgs)
        'If no login information is available from the Task, return
        If _loginTaskCompletionSrc Is Nothing _
            Or _loginTaskCompletionSrc.Task Is Nothing _
            Or _loginTaskCompletionSrc.Task.AsyncState Is Nothing Then Return

        ' Get the login info (helper class) that was stored with the task
        Dim userInfo As LoginInfo = TryCast(_loginTaskCompletionSrc.Task.AsyncState, LoginInfo)

        Try
            ' Create a New System.Net.NetworkCredential with the user name, password, And domain provided
            Dim networkCred As NetworkCredential = New NetworkCredential(userInfo.UserName, userInfo.Password, userInfo.Domain)

            ' Create a New ArcGISNetworkCredential with the NetworkCredential And URI of the secured resource
            Dim cred As Credential = New ArcGISNetworkCredential With
                {
                    .Credentials = networkCred,
                    .ServiceUri = New Uri(userInfo.ServiceUrl)
                }

            ' Set the result of the login task with the New ArcGISNetworkCredential
            _loginTaskCompletionSrc.TrySetResult(cred)
        Catch ex As Exception
            ' Report login exceptions at the bottom of the dialog
            userInfo.ErrorMessage = ex.Message
        End Try
    End Sub

    Private Sub CancelButtonClick(sender As Object, e As RoutedEventArgs)
        'Set the login task status to canceled
        _loginTaskCompletionSrc.TrySetCanceled()
    End Sub

End Class

' A helper class to hold information about a network credential
Public Class LoginInfo
    Implements INotifyPropertyChanged

    Private _requestInfo As CredentialRequestInfo
    Public Property RequestInfo As CredentialRequestInfo
        Get
            Return _requestInfo
        End Get
        Set
            _requestInfo = Value
            OnPropertyChanged()
        End Set
    End Property

    ' URL of the secure resource
    Private _serviceUrl As String
    Public Property ServiceUrl As String
        Get
            Return _serviceUrl
        End Get
        Set
            _serviceUrl = Value
            OnPropertyChanged()
        End Set
    End Property

    ' User name for the credential
    Private _userName As String
    Public Property UserName As String
        Get
            Return _userName
        End Get
        Set
            _userName = Value
            OnPropertyChanged()
        End Set
    End Property

    ' Password for the credential
    Private _password As String
    Public Property Password As String
        Get
            Return _password
        End Get
        Set
            _password = Value
            OnPropertyChanged()
        End Set
    End Property

    ' Domain for the network credential
    Private _domain As String
    Public Property Domain As String
        Get
            Return _domain
        End Get
        Set
            _domain = Value
            OnPropertyChanged()
        End Set
    End Property

    ' Login error messages
    Private _errorMessage As String
    Public Property ErrorMessage As String
        Get
            Return _errorMessage
        End Get
        Set
            _errorMessage = Value
            OnPropertyChanged()
        End Set
    End Property

    Public Sub New(info As CredentialRequestInfo)
        ' Store the request info
        RequestInfo = info

        ' Build the service URL from the request info
        ServiceUrl = requestInfo.ServiceUri.AbsoluteUri

        ' Login info Is empty by default, will be populated by the user
        UserName = String.Empty
        Password = String.Empty
        Domain = String.Empty
        ErrorMessage = String.Empty
    End Sub

    ' Raise an Event When properties change To make sure data bindings are updated
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
