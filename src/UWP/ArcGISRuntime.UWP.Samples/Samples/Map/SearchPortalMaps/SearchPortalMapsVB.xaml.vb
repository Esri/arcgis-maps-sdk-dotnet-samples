' Copyright 2017 Esri.
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

Namespace SearchPortalMaps
    Partial Public Class SearchPortalMapsVB

        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with (ArcGIS Online)
        Private ArcGISOnlineUrl As String = "https://www.arcgis.com/sharing/rest"

        ' Client ID for the app registered with the server (Portal Maps)
        Private AppClientId As String = "2Gh53JRzkPtOENQq"

        ' Redirect URL after a successful authorization (configured for the Portal Maps application)
        Private OAuthRedirectUrl As String = "https://developers.arcgis.com"

        Public Sub New()

            InitializeComponent()

            ' Set up the authentication manager and display a default map
            UpdateAuthenticationManager()
            DisplayDefaultMap()

        End Sub

        Private Sub DisplayDefaultMap()

            ' Create a new Map instance
            Dim myMap As Map = New Map(Basemap.CreateLightGrayCanvas())

            ' Provide Map to the MapView
            MyMapView.Map = myMap

        End Sub


        Private Sub OnMapSelectionChanged(sender As Object, e As SelectionChangedEventArgs)

            ' When a web map Is selected, update the map in the map view
            If Not e.AddedItems Is Nothing AndAlso e.AddedItems.Count > 0 Then

                ' Make sure a portal item Is selected
                Dim selectedMap As PortalItem = TryCast(e.AddedItems(0), PortalItem)
                If selectedMap Is Nothing Then Return

                ' Create a New map And display it
                Dim webMap As Map = New Map(selectedMap)
                MyMapView.Map = webMap

            End If

            ' Hide the flyouts
            SearchMapsFlyout.Hide()
            MyMapsFlyout.Hide()

            ' Unselect the map item
            Dim mapList As ListView = TryCast(sender, ListView)
            mapList.SelectedItem = Nothing

        End Sub

        Private Async Sub MyMapsClicked(sender As Object, e As RoutedEventArgs)

            ' Get web map portal items in the current user's folder or from a keyword search
            Dim mapItems As IEnumerable(Of PortalItem) = Nothing
            Dim portal As ArcGISPortal

            ' If the list has already been populated, return
            If Not MyMapsList.ItemsSource Is Nothing Then Return

            ' Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
            Dim loggedIn As Boolean = Await EnsureLoggedInAsync()
            If Not loggedIn Then Return

            ' Connect to the portal (will connect using the provided credentials)
            portal = Await ArcGISPortal.CreateAsync(New Uri(ArcGISOnlineUrl))

            ' Get the user's content (items in the root folder and a collection of sub-folders)
            Dim myContent As PortalUserContent = Await portal.User.GetContentAsync()

            ' Get the web map items in the root folder
            mapItems = From item In myContent.Items Where item.Type = PortalItemType.WebMap Select item

            ' Loop through all sub-folders And get web map items, add them to the mapItems collection
            For Each folder As PortalFolder In myContent.Folders
                Dim folderItems As IEnumerable(Of PortalItem) = Await portal.User.GetContentAsync(folder.FolderId)
                mapItems.Concat(From item In folderItems Where item.Type = PortalItemType.WebMap Select item)
            Next

            ' Show the web maps in the list box
            MyMapsList.ItemsSource = mapItems

            ' Make sure the flyout is shown
            Dim element As FrameworkElement = TryCast(sender, FrameworkElement)
            MyMapsFlyout.ShowAt(element)

        End Sub

        Private Async Sub SearchMapsClicked(sender As Object, e As RoutedEventArgs)

            ' Get web map portal items in the current user's folder or from a keyword search
            Dim mapItems As IEnumerable(Of PortalItem) = Nothing
            Dim portal As ArcGISPortal

            ' Connect to the portal (anonymously)
            portal = Await ArcGISPortal.CreateAsync(New Uri(ArcGISOnlineUrl))

            ' Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
            Dim queryExpression As String = String.Format("tags:""{0}"" access:public type: (""web map"" NOT ""web mapping application"")", SearchText.Text)
            ' Create a query parameters object with the expression and a limit of 10 results
            Dim queryParams As PortalQueryParameters = New PortalQueryParameters(queryExpression, 10)

            ' Search the portal using the query parameters and await the results
            Dim findResult As PortalQueryResultSet(Of PortalItem) = Await portal.FindItemsAsync(queryParams)
            ' Get the items from the query results
            mapItems = findResult.Results

            ' Show the search result items in the list
            SearchMapsList.ItemsSource = mapItems

        end Sub


        Private Async Function EnsureLoggedInAsync() As Task(Of Boolean)

            Dim loggedIn As Boolean = False

            Try
                ' Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                Dim challengeRequest As CredentialRequestInfo = New CredentialRequestInfo()

                ' Use the OAuth implicit grant flow
                Dim tokenOptions = New GenerateTokenOptions()
                tokenOptions.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                challengeRequest.GenerateTokenOptions = tokenOptions

                ' Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = New Uri(ArcGISOnlineUrl)

                ' Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Dim cred As Credential = Await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, False)
                loggedIn = Not cred Is Nothing

            Catch ex As OperationCanceledException
                'TODO: handle login cancellation
            Catch ex As Exception
                'TODO: handle login failure
            End Try

            Return loggedIn

        End Function

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
            Dim cred As Credential = Nothing

            Try

                ' User will be challenged for credentials
                cred = Await AuthenticationManager.Current.GenerateCredentialAsync(
                            info.ServiceUri,
                            info.GenerateTokenOptions)

            Catch ex As Exception
                ' Exception will be reported in calling function
                Throw (ex)
            End Try

            Return cred
        End Function
    End Class
End Namespace
