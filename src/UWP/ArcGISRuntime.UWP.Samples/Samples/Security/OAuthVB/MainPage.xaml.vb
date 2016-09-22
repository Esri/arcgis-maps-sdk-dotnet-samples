Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal
Imports Esri.ArcGISRuntime.Security
Imports Windows.UI.Popups

Public NotInheritable Class MainPage
    Inherits Page

    ' Constants for OAuth-related values ...
    ' URL of the server to authenticate with
    Private Const ServerUrl As String = "https://www.arcgis.com/sharing/rest"
    ' TODO: Provide the client ID for your app (registered with the server)
    Private Const ClientId As String = ""
    ' TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
    Private Const ClientSecret As String = ""
    ' TODO: Provide a URL registered for the app for redirecting after a successful authorization
    Private Const RedirectUrl As String = "http://my.redirect.url"
    ' TODO: Provide an ID for a secured web map item hosted on the server
    Private Const WebMapId As String = ""

    Public Sub New()
        ' This call is required by the designer
        InitializeComponent()

        ' Call a sub to initialize the app
        Initialize()
    End Sub

    Private Sub Initialize()
        ' Set up the AuthenticationManager to authenticate requests for secure ArcGIS Online resources with OAuth
        UpdateAuthenticationManager()

        ' Display a secured web map from ArcGIS Online (will be challenged to log in)
        DisplayWebMap()
    End Sub

    Private Sub UpdateAuthenticationManager()
        ' Register the server information with the AuthenticationManager
        Dim serverInfo As Esri.ArcGISRuntime.Security.ServerInfo = New ServerInfo With
            {
                .ServerUri = New Uri(ServerUrl),
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
        Else
            ' Otherwise, use OAuthImplicit
            serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
        End If

        ' Register this server with AuthenticationManager
        AuthenticationManager.Current.RegisterServer(serverInfo)

        ' Use a function in this class to challenge for credentials
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateCredentialAsync)

        ' Note In a WPF app, you need to associate a custom IOAuthAuthorizeHandler component with the AuthenticationManager to 
        '     handle showing OAuth login controls (AuthenticationManager.Current.OAuthAuthorizeHandler = New MyOAuthAuthorize()).
        '     The UWP AuthenticationManager, however, uses a built-in IOAuthAuthorizeHandler (based on WebAuthenticationBroker).
    End Sub

    Private Async Sub DisplayWebMap()
        ' Display a web map hosted in a portal. If the web map item Is secured, AuthenticationManager will
        ' challenge for credentials
        Try
            ' Connect to a portal (ArcGIS Online, for example)
            Dim arcgisPortal As ArcGISPortal = Await ArcGISPortal.CreateAsync(New Uri(ServerUrl))
            ' Get a web map portal item using its ID
            ' If the item Is secured (Not shared publicly) the user will be challenged for credentials at this point
            Dim portalItem As ArcGISPortalItem = Await ArcGISPortalItem.CreateAsync(arcgisPortal, WebMapId)
            ' Create a New map with the portal item
            Dim myMap As Map = New Map(portalItem)

            ' Assign the map to the MapView.Map property to display it in the app
            MyMapView.Map = myMap
            Await myMap.RetryLoadAsync()
        Catch ex As Exception
            Dim dlg As MessageDialog = New MessageDialog(ex.Message)
            dlg.ShowAsync()
        End Try
    End Sub

    Public Async Function CreateCredentialAsync(info As CredentialRequestInfo) As Task(Of Credential)
        ' ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        ' resource Is attempted
        Dim credenshul As OAuthTokenCredential = Nothing

        Try
            ' Create generate token options if necessary
            If info.GenerateTokenOptions Is Nothing Then
                info.GenerateTokenOptions = New GenerateTokenOptions()
            End If

            ' AuthenticationManager will handle challenging the user for credentials
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
