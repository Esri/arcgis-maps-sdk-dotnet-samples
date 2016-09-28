' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports System.Windows.Media
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Layers
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology

Namespace CreateFeatureCollectionLayer
    Partial Public Class CreateFeatureCollectionLayerVB
        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            Try
                ' Create a New map with the oceans basemap And add it to the map view
                Dim myMap = New Map(Basemap.CreateOceans())
                MyMapView.Map = myMap

                ' Call a function that will create a New feature collection layer And zoom to it
                CreateNewFeatureCollection()
            Catch ex As Exception
                MessageBox.Show("Unable to create feature collection layer: " + ex.Message, "Error")
            End Try
        End Sub

        Private Async Sub CreateNewFeatureCollection()
            ' Create the schema for a points table (one text field to contain a name attribute)
            Dim pointFields As List(Of Field) = New List(Of Field)()
            Dim placeField As Field = New Field(FieldType.Text, "Place", "Place Name", 50)
            pointFields.Add(placeField)

            ' Create the schema for a lines table (one text field to contain a name attribute)
            Dim lineFields As List(Of Field) = New List(Of Field)()
            Dim boundaryField As Field = New Field(FieldType.Text, "Boundary", "Boundary Name", 50)
            lineFields.Add(boundaryField)

            ' Create the schema for a polygon table (one text field to contain a name attribute)
            Dim polyFields As List(Of Field) = New List(Of Field)()
            Dim areaField As Field = New Field(FieldType.Text, "AreaName", "Area Name", 50)
            polyFields.Add(areaField)

            ' Instantiate FeatureCollectionTables with schema And geometry type
            Dim pointsTable As FeatureCollectionTable = New FeatureCollectionTable(pointFields, GeometryType.Point, SpatialReferences.Wgs84)
            Dim linesTable As FeatureCollectionTable = New FeatureCollectionTable(lineFields, GeometryType.Polyline, SpatialReferences.Wgs84)
            Dim polysTable As FeatureCollectionTable = New FeatureCollectionTable(polyFields, GeometryType.Polygon, SpatialReferences.Wgs84)

            ' Set rendering for each table
            pointsTable.Renderer = CreateRenderer(GeometryType.Point)
            linesTable.Renderer = CreateRenderer(GeometryType.Polyline)
            polysTable.Renderer = CreateRenderer(GeometryType.Polygon)

            ' Create a New point feature, provide geometry And attribute values
            Dim pointFeature As Feature = pointsTable.CreateFeature()
            pointFeature.SetAttributeValue(placeField, "Current location")
            Dim point1 As MapPoint = New MapPoint(-79.497238, 8.849289, SpatialReferences.Wgs84)
            pointFeature.Geometry = point1

            ' Create a New line feature, provide geometry And attribute values
            Dim lineFeature As Feature = linesTable.CreateFeature()
            lineFeature.SetAttributeValue(boundaryField, "AManAPlanACanalPanama")
            Dim point2 As MapPoint = New MapPoint(-80.035568, 9.432302, SpatialReferences.Wgs84)
            Dim line As Polyline = New Polyline(New MapPoint() {point1, point2})
            lineFeature.Geometry = line

            ' Create a New polygon feature, provide geometry And attribute values
            Dim polyFeature As Feature = polysTable.CreateFeature()
            polyFeature.SetAttributeValue(areaField, "Restricted area")
            Dim point3 As MapPoint = New MapPoint(-79.337936, 8.638903, SpatialReferences.Wgs84)
            Dim point4 As MapPoint = New MapPoint(-79.11409, 8.895422, SpatialReferences.Wgs84)
            Dim poly As Polygon = New Polygon(New MapPoint() {point1, point3, point4})
            polyFeature.Geometry = poly

            ' Add the New features to the appropriate feature collection table 
            Await pointsTable.AddFeatureAsync(pointFeature)
            Await linesTable.AddFeatureAsync(lineFeature)
            Await polysTable.AddFeatureAsync(polyFeature)

            ' Create a feature collection And add the feature collection tables
            Dim featuresCollection As FeatureCollection = New FeatureCollection()
            featuresCollection.Tables.Add(pointsTable)
            featuresCollection.Tables.Add(linesTable)
            featuresCollection.Tables.Add(polysTable)

            ' Create a FeatureCollectionLayer And add to the Map's Operational Layers collection
            Dim collectionLayer As FeatureCollectionLayer = New FeatureCollectionLayer(featuresCollection)
            MyMapView.Map.OperationalLayers.Add(collectionLayer)

            ' Zoom the map view to the extent of the feature collection
            MyMapView.SetViewpointAsync(New Viewpoint(collectionLayer.FullExtent))
        End Sub

        Private Function CreateRenderer(rendererType As GeometryType) As Renderer
            ' Return a simple renderer to match the geometry type provided
            Dim sym As Symbol = Nothing

            Select Case (rendererType)

                Case GeometryType.Point, GeometryType.Multipoint
                    ' Create a marker symbol
                    sym = New SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Colors.Red, 18)
                Case GeometryType.Polyline
                    ' Create a line symbol
                    sym = New SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Green, 3)
                Case GeometryType.Polygon
                    ' Create a fill symbol
                    Dim lineSym As SimpleLineSymbol = New SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.DarkBlue, 2)
                    sym = New SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Colors.Cyan, lineSym)
            End Select

            ' Return a New renderer that uses the symbol created above
            Return New SimpleRenderer(sym)
        End Function
    End Class
End Namespace