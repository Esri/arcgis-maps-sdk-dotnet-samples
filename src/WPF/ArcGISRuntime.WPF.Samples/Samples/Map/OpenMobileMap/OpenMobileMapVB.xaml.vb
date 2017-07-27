' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may Not use this file except in compliance with the License.
' You may obtain a copy of the License at: http : //www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law Or agreed to in writing, software distributed under the License Is distributed on an
' "AS IS" BASIS, WITHOUT WARRANTIES Or CONDITIONS OF ANY KIND, either express Or implied. See the License for the specific
' language governing permissions And limitations under the License.

Imports System.Linq
Imports Esri.ArcGISRuntime.Mapping

Namespace OpenMobileMap
    Public Class OpenMobileMapVB

        ' Hold a reference to a Mobile Map Package
        Dim MyMapPackage As MobileMapPackage

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()
            ' Load the Mobile Map Package from the Bundle
            '     File Is located in Resources/MobileMapPackages/Yellowstone.mmpk
            '     Build Action Is Content; Copy if newer
            MyMapPackage = Await MobileMapPackage.OpenAsync("Resources\\MobileMapPackages\\Yellowstone.mmpk")

            ' Check that there Is at least one map
            If MyMapPackage.Maps.Count > 0 Then
                ' Display the first map in the package
                MyMapView.Map = MyMapPackage.Maps.First()
            End If
        End Sub
    End Class
End Namespace

