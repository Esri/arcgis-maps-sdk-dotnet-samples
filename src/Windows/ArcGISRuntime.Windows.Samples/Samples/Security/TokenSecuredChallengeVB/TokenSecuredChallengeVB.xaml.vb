' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Security
Imports Windows.UI.Core

Public NotInheritable Class MainPage
    Inherits Page
    ' Constants for the public and secured map service URLs
    Private Const PublicMapServiceUrl As String = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer"
    Private Const SecureMapServiceUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer"

    ' Constants for the public and secured layer names
    Private Const PublicLayerName As String = "World Street Map - Public"
    Private Const SecureLayerName As String = "USA - Secure"

    ' Task completion source to track a login attempt
    Private _loginTaskCompletionSource As TaskCompletionSource(Of Credential)

    Public Sub New()
        InitializeComponent()

        ' Define a method to challenge the user for credentials when a secured resource is encountered
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf Challenge)

        ' Call a function to create a map and add public and secure layers
        Initialize()
    End Sub

    Private Sub Initialize()
        ' Create the public layer and provide a name
        Dim publicLayer As ArcGISTiledLayer = New ArcGISTiledLayer(New Uri(PublicMapServiceUrl))
        publicLayer.Name = PublicLayerName

        ' Set the data context for the public layer stack panel controls (to report name And load status)
        PublicLayerPanel.DataContext = publicLayer

        ' Create the secured layer and provide a name
        Dim tokenSecuredLayer As ArcGISMapImageLayer = New ArcGISMapImageLayer(New Uri(SecureMapServiceUrl))
        tokenSecuredLayer.Name = SecureLayerName

        ' Set the data context for the secure layer stack panel controls (to report name and load status)
        SecureLayerPanel.DataContext = tokenSecuredLayer

        ' Create a new map and add the layers
        Dim myMap As Map = New Map()
        myMap.OperationalLayers.Add(publicLayer)
        myMap.OperationalLayers.Add(tokenSecuredLayer)

        ' Add the map to the map view
        MyMapView.Map = myMap
    End Sub

    Private Async Function Challenge(info As CredentialRequestInfo) As Task(Of Credential)
        ' Call code to get user credentials
        ' Make sure it runs on the UI thread
        If Me.Dispatcher Is Nothing Then
            ' No current dispatcher, code is already running on the UI thread
            Return Await GetUserCredentialsFromUI(info)
        Else
            ' Use the dispatcher to invoke the challenge UI
            Await Me.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                         Async Sub()
                                             Try
                                                 Await GetUserCredentialsFromUI(info)
                                             Catch
                                                 ' Login was canceled or unsuccessful, dialog will close
                                             End Try
                                         End Sub)
        End If

        ' Use the task completion source to return the results
        Return Await _loginTaskCompletionSource.Task
    End Function

    Private Async Function GetUserCredentialsFromUI(info As CredentialRequestInfo) As Task(Of Credential)
        ' Show the login UI
        Try
            ' Create a New LoginInfo to store the entered username And password
            ' Pass the CredentialRequestInfo object so the resource URI can be stored
            Dim loginInputInfo As LoginInfo = New LoginInfo(info)

            ' Set the login UI data context with the LoginInfo
            loginPanel.DataContext = loginInputInfo

            ' Show the login UI
            loginPanel.Visibility = Visibility.Visible

            ' Create a new task completion source to return the user's login when complete
            ' Set the login UI data context (LoginInfo object) as the AsyncState so it can be retrieved later
            _loginTaskCompletionSource = New TaskCompletionSource(Of Credential)(loginPanel.DataContext)

            ' Return the task from the completion source
            ' When the login button on the UI is clicked, the info will be returned for creating the credential
            Return Await _loginTaskCompletionSource.Task
        Finally
            ' Hide the login UI
            loginPanel.Visibility = Visibility.Collapsed
        End Try
    End Function

    ' Handle the click event for the login button on the login UI
    Private Async Sub LoginButtonClick(sender As Object, e As RoutedEventArgs)
        ' Make sure there's a task completion source for the login operation
        If _loginTaskCompletionSource Is Nothing Or
            _loginTaskCompletionSource.Task Is Nothing Or
            _loginTaskCompletionSource.Task.AsyncState Is Nothing Then
            Return
        End If

        ' Get the login info from the task completion source
        Dim loginEntry As LoginInfo = TryCast(_loginTaskCompletionSource.Task.AsyncState, LoginInfo)

        Try
            ' Create a token credential using the provided username and password
            Dim userCredentials As TokenCredential = Await AuthenticationManager.Current.GenerateCredentialAsync(New Uri(loginEntry.ServiceUrl),
                                             loginEntry.UserName,
                                             loginEntry.Password,
                                             loginEntry.RequestInfo.GenerateTokenOptions)

            ' Set the result on the task completion source
            _loginTaskCompletionSource.TrySetResult(userCredentials)
        Catch ex As Exception
            ' Show exceptions on the login UI
            loginEntry.ErrorMessage = ex.Message

            ' Increment the login attempt count
            loginEntry.AttemptCount += 1

            ' Set an exception on the login task completion source after three login attempts
            If (loginEntry.AttemptCount >= 3) Then
                ' This causes the login attempt to fail
                _loginTaskCompletionSource.TrySetException(New Exception("Exceeded the number of allowed login attempts"))
            End If
        End Try
    End Sub
End Class

' A helper class to hold information about a network credential
Friend Class LoginInfo
    Implements INotifyPropertyChanged

    ' Credential request information (from the challenge handler)
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

    ' Number of login attempts
    Private _attemptCount As Integer
    Public Property AttemptCount As Integer
        Get
            Return _attemptCount
        End Get
        Set
            _attemptCount = Value
            OnPropertyChanged()
        End Set
    End Property

    Public Sub New(info As CredentialRequestInfo)
        ' Store the request info
        RequestInfo = info

        ' Build the service URL from the request info
        ServiceUrl = RequestInfo.ServiceUri.AbsoluteUri

        ' Login info is empty by default, will be populated by the user
        UserName = String.Empty
        Password = String.Empty
        ErrorMessage = String.Empty
        AttemptCount = 0
    End Sub

    ' Raise an event when properties change to make sure data bindings are updated
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
