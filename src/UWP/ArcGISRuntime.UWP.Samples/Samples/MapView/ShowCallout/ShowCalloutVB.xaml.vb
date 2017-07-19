' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.UI
Imports Esri.ArcGISRuntime.UI.Controls

Namespace ShowCallout

    Partial Public Class ShowCalloutVB

        Public Sub New()

            InitializeComponent()

            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a new basemap using the streets base layer
            Dim myBasemap As Basemap = Basemap.CreateStreets()

            ' Create a new map based on the streets basemap
            Dim myMap As New Map(myBasemap)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub MyMapView_GeoViewTapped(sender As Object, e As GeoViewInputEventArgs) Handles MyMapView.GeoViewTapped
            ' Get the user-tapped location
            Dim mapLocation As MapPoint = e.Location

            ' Project the user-tapped map point location to a geometry
            Dim myGeometry As Geometry = GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84)

            ' Convert to geometry to a traditional Lat/Long map point
            Dim projectedLocation As MapPoint = CType(myGeometry, MapPoint)

            ' Format the display callout string based upon the projected map point (example "Lat: 100.123, Long: 100.234")
            Dim mapLocationDescription As String = String.Format("Lat: {0:F3} Long:{1:F3}", projectedLocation.Y, projectedLocation.X)

            ' Create a New callout definition using the formatted string
            Dim myCalloutDefinition As CalloutDefinition = New CalloutDefinition("Location:", mapLocationDescription)

            ' Display the callout
            MyMapView.ShowCalloutAt(mapLocation, myCalloutDefinition)
        End Sub

    End Class

End Namespace