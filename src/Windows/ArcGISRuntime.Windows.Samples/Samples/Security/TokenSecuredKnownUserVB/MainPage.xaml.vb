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
Imports Windows.UI.Popups

Public NotInheritable Class MainPage
    Inherits Page
    ' Constants for the public and secured map service URLs
    Private Const PublicMapServiceUrl As String = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer"
    Private Const SecureMapServiceUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer"

    ' Constants for the public and secured layer names
    Private Const PublicLayerName As String = "World Street Map - Public"
    Private Const SecureLayerName As String = "USA - Secure"

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

    ' Challenge method that checks for service access with known (hard coded) credentials
    Private Async Function Challenge(info As CredentialRequestInfo) As Task(Of Credential)
        ' If this isn't the expected resource, the credential will stay null
        Dim knownCredential As Credential = Nothing

        Try
            ' Check the URL of the requested resource
            If (info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1")) Then
                ' Username And password Is hard-coded for this resource
                ' (Would be better to read them from a secure source)
                Dim username As String = "user1"
                Dim password As String = "user1"

                ' Create a credential for this resource
                knownCredential = Await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri,
                                             username,
                                             password,
                                             info.GenerateTokenOptions)
            Else
                ' Another option would be to prompt the user here if the username And password Is Not known
            End If
        Catch ex As Exception
            ' Report error accessing a secured resource
            Dim messageDlg As MessageDialog = New MessageDialog("Access to " + info.ServiceUri.AbsoluteUri + " denied. " + ex.Message, "Credential Error")
            messageDlg.ShowAsync()
        End Try

        ' Return the credential
        Return knownCredential
    End Function
End Class
