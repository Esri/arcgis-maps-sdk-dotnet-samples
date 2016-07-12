' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ManageBookmarks

    Partial Public Class ManageBookmarksVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization. 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new map with a basemap.
            Dim myMap As New Map(Basemap.CreateImageryWithLabels())

            ' Set the mapview, map property to the basemap.
            MyMapView.Map = myMap

            ' Create a set of predefined bookmarks; each one follows the pattern of:
            ' ~ Initialize a viewpoint pointing to a latitude longitude
            ' ~ Create a new bookmark
            ' ~ Give the bookmark a name
            ' ~ Assign the viewpoint
            ' ~ Add the bookmark to bookmark collection of the map
            ' ~ Add the bookmark name to the UI combobox for the user to choose from 

            ' Bookmark-1
            Dim myViewpoint1 As New Viewpoint(27.3805833, 33.6321389, 6000)
            Dim myBookmark1 As New Bookmark()
            myBookmark1.Name = "Mysterious Desert Pattern"
            myBookmark1.Viewpoint = myViewpoint1
            MyMapView.Map.Bookmarks.Add(myBookmark1)
            bookmarkChooser.Items.Add(myBookmark1.Name)

            ' Bookmark-2
            Dim myViewpoint2 As New Viewpoint(37.401573, -116.867808, 6000)
            Dim myBookmark2 As New Bookmark()
            myBookmark2.Name = "Strange Symbol"
            myBookmark2.Viewpoint = myViewpoint2
            MyMapView.Map.Bookmarks.Add(myBookmark2)
            bookmarkChooser.Items.Add(myBookmark2.Name)

            ' Bookmark-3
            Dim myViewpoint3 As New Viewpoint(-33.867886, -63.985, 40000)
            Dim myBookmark3 As New Bookmark()
            myBookmark3.Name = "Guitar-Shaped Forest"
            myBookmark3.Viewpoint = myViewpoint3
            MyMapView.Map.Bookmarks.Add(myBookmark3)
            bookmarkChooser.Items.Add(myBookmark3.Name)

            ' Bookmark-4
            Dim myViewpoint4 As New Viewpoint(44.525049, -110.83819, 6000)
            Dim myBookmark4 As New Bookmark()
            myBookmark4.Name = "Grand Prismatic Spring"
            myBookmark4.Viewpoint = myViewpoint4
            MyMapView.Map.Bookmarks.Add(myBookmark4)
            bookmarkChooser.Items.Add(myBookmark4.Name)

            ' Set the initial combobox selection to the lat bookmark added.
            bookmarkChooser.SelectedIndex = 3

            ' Zoom to the last bookmark.
            myMap.InitialViewpoint = myMap.Bookmarks.Last().Viewpoint

            ' Hide the controls for adding an additional bookmark.
            BorderAddBookmark.Visibility = Visibility.Collapsed

        End Sub

        Private Sub OnBookmarkChooserSelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)

            ' Get the selected bookmarks name.
            Dim selectedBookmarkName = e.AddedItems(0).ToString()

            ' Get the collection of bookmarks in the map.
            Dim myBookmarkCollection As BookmarkCollection = MyMapView.Map.Bookmarks

            ' Loop through each bookmark. 
            For Each myBookmark In myBookmarkCollection

                ' Get the bookmarks name.
                Dim theBookmarkName = myBookmark.Name

                ' If the selected bookmarks name matches one in the bookmark collection
                ' set that to be the maps viewpoint.
                If theBookmarkName = selectedBookmarkName.ToString() Then
                    MyMapView.SetViewpoint(myBookmark.Viewpoint)
                End If

            Next myBookmark

        End Sub

        Private Sub ButtonAddBookmark_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

            ' Show the controls to add a bookmark.
            BorderAddBookmark.Visibility = Visibility.Visible
            ButtonAddBookmark.Visibility = Visibility.Collapsed
            TextBoxBookmarkName.Text = ""

        End Sub

        Private Sub ButtonCanel_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

            ' Hide the controls to add a bookmark.
            BorderAddBookmark.Visibility = Visibility.Collapsed
            ButtonAddBookmark.Visibility = Visibility.Visible

        End Sub

        Private Sub ButtonAddDone_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

            ' Get the name from the text field.
            Dim name = TextBoxBookmarkName.Text

            ' Exit if the name is empty.
            If String.IsNullOrEmpty(name) Then
                Return
            End If

            ' Check to see if there is a bookmark with same name.
            Dim doesNameExist As Boolean = MyMapView.Map.Bookmarks.Any(Function(b) b.Name = name)
            If doesNameExist Then
                Return
            End If

            ' Create a new bookmark.
            Dim myBookmark As New Bookmark()
            myBookmark.Name = name

            ' Get the current viewpoint from map and assign it to bookmark. 
            myBookmark.Viewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)

            ' Add the bookmark to bookmark collection of the map.
            MyMapView.Map.Bookmarks.Add(myBookmark)

            ' Add the bookmark name to the list of choices in the combobox.
            bookmarkChooser.Items.Add(name)

            ' Set the newly added bookmark to be the one selected in the combobox.
            bookmarkChooser.SelectedValue = name

            ' Hide the controls to add a bookmark.
            BorderAddBookmark.Visibility = Visibility.Collapsed
            ButtonAddBookmark.Visibility = Visibility.Visible

        End Sub

    End Class

End Namespace
