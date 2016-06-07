' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping

Namespace SetMinMaxScale
    Partial Public Class SetMinMaxScaleVB
        Public Sub New()
            InitializeComponent()
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with Streets basemap 
            Dim myMap As New Map(Basemap.CreateStreets())

            ' Set the scale at which this layer can be viewed
            ' MinScale defines how far 'out' you can zoom where
            ' MaxScale defines how far 'in' you can zoom.
            myMap.MinScale = 8000
            myMap.MaxScale = 2000

            ' Assign the map to the MapView
            MyMapView.Map = myMap

            ' Create central point where map is centered
            Dim centralPoint As New MapPoint(-355453, 7548720, SpatialReferences.WebMercator)

            ' Create starting viewpoint
            Dim startingViewpoint As New Viewpoint(centralPoint, 3000)

            ' Set starting viewpoint
            MyMapView.SetViewpoint(startingViewpoint)
        End Sub
    End Class
End Namespace