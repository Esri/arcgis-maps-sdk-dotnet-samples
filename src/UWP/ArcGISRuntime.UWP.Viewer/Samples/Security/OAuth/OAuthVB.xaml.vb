' Copyright 2016 Esri.
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

Namespace OAuth
    Partial Public Class OAuthVB
        ' Constants for OAuth-related values ...
        ' URL of the server to authenticate with
        Private Const ServerUrl As String = "https://www.arcgis.com/sharing/rest"

        ' TODO: Provide the client ID for your app (registered with the server)
        Private Const ClientId As String = "lgAdHkYZYlwwfAhC"

        ' TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
        Private Const ClientSecret As String = ""

        ' TODO: Provide a URL registered for the app for redirecting after a successful authorization
        Private Const RedirectUrl As String = "my-ags-app://auth"

        ' TODO: Provide an ID for a secured web map item hosted on the server
        Private Const WebMapId As String = "cbd8ac5252fa4cf8a55d8350265c531b"

        Public Sub New()
            InitializeComponent()

            ' Call a sub to initialize the app
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Set up the AuthenticationManager to authenticate requests for secure ArcGIS Online resources with OAuth
            SetOAuthInfo()

            ' Connect to a portal (ArcGIS Online, for example)
            Dim arcgisPortal As ArcGISPortal = Await ArcGISPortal.CreateAsync(New Uri(ServerUrl))

            ' Get a web map portal item using its ID
            ' (If the item contains layers not shared publicly, the user will be challenged for credentials at this point)
            Dim portalItem As PortalItem = Await PortalItem.CreateAsync(arcgisPortal, WebMapId)

            ' Create a New map with the portal item
            Dim myMap As Map = New Map(portalItem)

            ' Assign the map to the MapView.Map property to display it in the app
            MyMapView.Map = myMap
        End Sub

        Private Sub SetOAuthInfo()
            ' Register the server information with the AuthenticationManager
            Dim serverInfo As Esri.ArcGISRuntime.Security.ServerInfo = New ServerInfo With
                {
                    .ServerUri = New Uri(ServerUrl),
                    .TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                    .OAuthClientInfo = New OAuthClientInfo With
                    {
                        .ClientId = ClientId,
                        .RedirectUri = New Uri(RedirectUrl)
                    }
                }

            ' If a client secret has been configured, set the authentication type to OAuthAuthorizationCode
            If Not String.IsNullOrEmpty(ClientSecret) Then
                ' Use OAuthAuthorizationCode if you need a refresh token (And have specified a valid client secret)
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode
                serverInfo.OAuthClientInfo.ClientSecret = ClientSecret
            End If

            ' Register this server with AuthenticationManager
            AuthenticationManager.Current.RegisterServer(serverInfo)

            ' Use a function in this class to challenge for credentials
            AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)

            ' Note: In a WPF app, you need to associate a custom IOAuthAuthorizeHandler component with the AuthenticationManager to 
            '     handle showing OAuth login controls (AuthenticationManager.Current.OAuthAuthorizeHandler = new MyOAuthAuthorize();).
            '     The UWP AuthenticationManager, however, uses a built-in IOAuthAuthorizeHandler (based on WebAuthenticationBroker).
        End Sub

        Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
            ' ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
            ' resource Is attempted
            Dim cred As TokenCredential = Nothing

            Try
                ' AuthenticationManager will handle challenging the user for credentials
                cred = Await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri)
            Catch
                ' Exception will be reported in calling function
                Throw
            End Try

            Return cred
        End Function
    End Class
End Namespace