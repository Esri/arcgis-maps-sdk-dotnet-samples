﻿' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace MapRotation

    Partial Public Class MapRotationVB

        Public Sub New()

            InitializeComponent()

            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a new Map instance with the basemap  
            Dim myBasemap As Basemap = Basemap.CreateStreets()
            Dim myMap As New Map(myBasemap)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub MySlider_ValueChanged(sender As Object, e As RangeBaseValueChangedEventArgs)

            ' Display the rotation value in the Label formatted nicely with degree symbol.
            MyTextBlock.Text = String.Format("{0:0}° VB", MySlider.Value)

            ' Set the MapView rotation to that of the Slider.
            MyMapView.SetViewpointRotationAsync(e.NewValue)

        End Sub

    End Class

End Namespace
