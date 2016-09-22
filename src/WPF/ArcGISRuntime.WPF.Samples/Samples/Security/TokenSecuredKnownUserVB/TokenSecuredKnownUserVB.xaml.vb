' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Security

Class MainWindow

    Public Sub New()

        InitializeComponent()

        ' Define a method that will try to create the required credentials when a secured resource is encountered
        ' (Access to the secure resource will be seamless to the user)
        AuthenticationManager.Current.ChallengeHandler = New ChallengeHandler(AddressOf CreateKnownCredentials)
    End Sub

    ' Challenge method that checks for service access with known (hard coded) credentials
    Private Async Function CreateKnownCredentials(info As CredentialRequestInfo) As Task(Of Credential)
        ' If this isn't the expected resource, the credential will stay null
        Dim knownCredential As Credential = Nothing

        Try
            ' Check the URL of the requested resource
            If info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1") Then
                ' Username and password is hard-coded for this resource
                ' (Would be better to read them from a secure source)
                Dim username As String = "user1"
                Dim password As String = "user1"

                ' Create a credential for this resource
                knownCredential = Await AuthenticationManager.Current.GenerateCredentialAsync(
                                             info.ServiceUri,
                                             username,
                                             password,
                                             info.GenerateTokenOptions)
            Else
                ' Another option would be to prompt the user here if the username And password Is Not known
            End If
        Catch ex As Exception
            ' Report error accessing a secured resource
            MessageBox.Show("Access to " + info.ServiceUri.AbsolutePath + " denied. " + ex.Message, "Credential Error")
        End Try

        ' Return the credential
        Return knownCredential
    End Function
End Class
