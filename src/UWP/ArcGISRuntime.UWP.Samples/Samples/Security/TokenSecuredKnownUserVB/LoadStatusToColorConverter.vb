Imports Windows.UI

Public Class LoadStatusToColorConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim statusColor As Color

        Select Case value
            Case Esri.ArcGISRuntime.LoadStatus.Loaded
                statusColor = Colors.Green
            Case Esri.ArcGISRuntime.LoadStatus.Loading
                statusColor = Colors.Gray
            Case Esri.ArcGISRuntime.LoadStatus.FailedToLoad
                statusColor = Colors.Red
            Case Esri.ArcGISRuntime.LoadStatus.NotLoaded
                statusColor = Colors.Red
            Case Else
                statusColor = Colors.Gray
        End Select

        Return New SolidColorBrush(statusColor)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
