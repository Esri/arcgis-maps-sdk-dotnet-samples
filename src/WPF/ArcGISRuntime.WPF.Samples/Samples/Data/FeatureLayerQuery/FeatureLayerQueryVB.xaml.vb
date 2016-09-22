' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports System.Windows
Imports System.Windows.Media

Namespace FeatureLayerQuery

    Public Class FeatureLayerQueryVB

        ' Create reference to service of US States  
        Private _statesUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2"

        ' Create globally available feature table for easy referencing 
        Private _featureTable As ServiceFeatureTable

        ' Create globally available feature layer for easy referencing 
        Private _featureLayer As FeatureLayer

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create and set initial map location
            Dim initialLocation As New MapPoint(-11000000, 5000000, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation, 100000000)

            ' Create feature table using a url
            _featureTable = New ServiceFeatureTable(New Uri(_statesUrl))

            ' Create feature layer using this feature table
            _featureLayer = New FeatureLayer(_featureTable)

            ' Set the Opacity of the Feature Layer
            _featureLayer.Opacity = 0.6

            ' Create a new renderer for the States Feature Layer
            Dim lineSymbol As New SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 1)
            Dim fillSymbol As New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Yellow, lineSymbol)

            ' Set States feature layer renderer
            _featureLayer.Renderer = New SimpleRenderer(fillSymbol)

            ' Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer)

            ' Assign the map to the MapView
            myMapView.Map = myMap

        End Sub

        Private Async Sub OnQueryClicked(sender As Object, e As System.Windows.RoutedEventArgs)

            ' Remove any previous feature selections that may have been made 
            _featureLayer.ClearSelection()

            ' Begin query process 
            Await QueryStateFeature(queryEntry.Text)

        End Sub

        Private Async Function QueryStateFeature(ByVal stateName As String) As Task

            Try

                ' Create a query parameters that will be used to Query the feature table  
                Dim queryParams As New QueryParameters()

                ' Construct and assign the where clause that will be used to query the feature table 
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" & (stateName.ToUpper()) & "%'"

                ' Query the feature table 
                Dim queryResult As FeatureQueryResult = Await _featureTable.QueryFeaturesAsync(queryParams)

                ' Cast the QueryResult to a List so the results can be interrogated
                Dim features = queryResult.ToList()

                If features.Any() Then

                    ' Get the first feature returned in the Query result 
                    Dim feature As Feature = features(0)

                    ' Add the returned feature to the collection of currently selected features
                    _featureLayer.SelectFeature(feature)

                    ' Zoom to the extent of the newly selected feature
                    Await myMapView.SetViewpointGeometryAsync(feature.Geometry.Extent)

                Else

                    MessageBox.Show("State Not Found!", "Add a valid state name.")

                End If

            Catch ex As Exception

                MessageBox.Show("Sample error", "An error occurred" & ex.ToString())

            End Try

        End Function

    End Class

End Namespace
