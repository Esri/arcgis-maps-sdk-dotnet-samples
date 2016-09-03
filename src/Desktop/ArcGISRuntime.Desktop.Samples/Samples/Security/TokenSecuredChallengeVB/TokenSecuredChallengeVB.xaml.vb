' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Esri.ArcGISRuntime.Security

Class MainWindow
    ' Task completion source to track a login attempt
    Private _loginTaskCompletionSource As TaskCompletionSource(Of Credential)

    Public Sub New()

        InitializeComponent()

        ' Define a method that will try to create the required credentials when a secured resource is encountered
        ' (Access to the secure resource will be seamless to the user)
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf Challenge)
    End Sub

    Private Async Function Challenge(info As CredentialRequestInfo) As Task(Of Credential)
        ' Call code to get user credentials
        ' Make sure it runs on the UI thread
        If Me.Dispatcher Is Nothing Then
            ' No current dispatcher, code Is already running on the UI thread
            Return Await GetUserCredentialsFromUI(info)
        Else
            ' Use the dispatcher to invoke the challenge UI
            Return Await Me.Dispatcher.Invoke(Function() GetUserCredentialsFromUI(info))
        End If
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

            ' Create a New task completion source to return the user's login when complete
            ' Set the login UI data context (LoginInfo object) as the AsyncState so it can be retrieved later
            _loginTaskCompletionSource = New TaskCompletionSource(Of Credential)(loginPanel.DataContext)

            ' Return the task from the completion source
            ' When the login button on the UI Is clicked, the info will be returned for creating the credential
            Return Await _loginTaskCompletionSource.Task
        Finally
            ' Hide the login UI
            loginPanel.Visibility = Visibility.Collapsed
        End Try
    End Function

    ' Handle the click event for the login button on the login UI
    Private Async Sub LoginButtonClick(sender As Object, e As RoutedEventArgs)
        ' Make sure there's a task completion source for the login operation
        If _loginTaskCompletionSource Is Nothing Or _loginTaskCompletionSource.Task Is Nothing Or _loginTaskCompletionSource.Task.AsyncState Is Nothing Then
            Return
        End If

        ' Get the login info from the task completion source
        Dim loginEntry As LoginInfo = TryCast(_loginTaskCompletionSource.Task.AsyncState, LoginInfo)

            Try
                ' Create a token credential using the provided username And password
                Dim userCredentials As TokenCredential = Await AuthenticationManager.Current.GenerateCredentialAsync(New Uri(loginEntry.ServiceUrl), loginEntry.UserName, loginEntry.Password, loginEntry.RequestInfo.GenerateTokenOptions)

                ' Set the result on the task completion source
                _loginTaskCompletionSource.TrySetResult(userCredentials)
            Catch ex As Exception
                ' Show exceptions on the login UI
                loginEntry.ErrorMessage = ex.Message

                ' Increment the login attempt count
                loginEntry.AttemptCount += 1

                ' Set an exception on the login task completion source after three login attempts
                If loginEntry.AttemptCount >= 3 Then
                    ' This causes the login attempt to fail
                    _loginTaskCompletionSource.TrySetException(ex)
                End If
            End Try
    End Sub
End Class


' Helper class to contain login information
Friend Class LoginInfo
    Implements INotifyPropertyChanged

    ' Information about the current request for credentials 
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

    ' URI for the service that is requesting credentials
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

    ' Username entered by the user
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

    ' Password entered by the user
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

    ' Last error message encountered while creating credentials
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

    ' Store the credential request information when the class is constructed
    Public Sub New(info As CredentialRequestInfo)
        RequestInfo = info
        ServiceUrl = info.ServiceUri.GetLeftPart(UriPartial.Path)
        ErrorMessage = String.Empty
        AttemptCount = 0
    End Sub

    ' Raise a property changed event so bound controls can update
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
