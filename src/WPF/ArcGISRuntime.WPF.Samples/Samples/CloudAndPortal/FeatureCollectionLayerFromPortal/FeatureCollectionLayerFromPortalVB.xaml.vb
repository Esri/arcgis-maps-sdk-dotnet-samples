' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Portal

Namespace FeatureCollectionLayerFromPortal
    Partial Public Class FeatureCollectionLayerFromPortalVB

        ' Default portal item Id to load features from
        Private Const FeatureCollectionItemId As String = "5ffe7733754f44a9af12a489250fe12b"

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            Try
                ' Add a default value for the portal item Id
                CollectionItemIdTextBox.Text = FeatureCollectionItemId

                ' Create a New map with the oceans basemap And add it to the map view
                Dim myMap = New Map(Basemap.CreateOceans())
                MyMapView.Map = myMap
            Catch ex As Exception
                MessageBox.Show("Unable to create feature collection layer: " + ex.Message, "Error")
            End Try
        End Sub


        Private Async Sub OpenFeaturesFromArcGISOnline(itemId As String)
            Try
                ' Open a portal item containing a feature collection
                Dim portal As ArcGISPortal = Await ArcGISPortal.CreateAsync()
                Dim collectionItem As PortalItem = Await PortalItem.CreateAsync(portal, itemId)

                ' Verify that the item Is a feature collection
                If collectionItem.Type = PortalItemType.FeatureCollection Then
                    ' Create a New FeatureCollection from the item
                    Dim featureCollexion As FeatureCollection = New FeatureCollection(collectionItem)

                    ' Create a layer to display the collection And add it to the map as an operational layer
                    Dim featureCollexionLayer = New FeatureCollectionLayer(featureCollexion)
                    featureCollexionLayer.Name = collectionItem.Title

                    MyMapView.Map.OperationalLayers.Add(featureCollexionLayer)
                Else
                    MessageBox.Show("Portal item with ID '" + itemId + "' is not a feature collection.", "Feature Collection")
                End If
            Catch ex As Exception
                MessageBox.Show("Unable to open item with ID '" + itemId + "': " + ex.Message, "Error")
            End Try
        End Sub

        Private Sub OpenPortalFeatureCollectionClick(sender As Object, e As RoutedEventArgs)
            ' Get the portal item Id from the user
            Dim collectionItemId As String = CollectionItemIdTextBox.Text.Trim()

            ' Make sure an Id was entered
            If String.IsNullOrEmpty(collectionItemId) Then
                MessageBox.Show("Please enter a portal item ID", "Feature Collection ID")
                Return
            End If

            ' Call a function to add the feature collection from the specified portal item
            OpenFeaturesFromArcGISOnline(collectionItemId)
        End Sub

    End Class
End Namespace