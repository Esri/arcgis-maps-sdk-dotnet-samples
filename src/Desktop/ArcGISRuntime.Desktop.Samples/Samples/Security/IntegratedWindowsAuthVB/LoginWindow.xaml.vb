' A simple UI for entering network credential information (username, password, and domain)
Public Class LoginWindow
    Public Sub New()
        InitializeComponent()
    End Sub

    ' A handler for both the "Login" And "Cancel" buttons on the page
    Private Sub ButtonClick(sender As Object, e As RoutedEventArgs)
        ' Set the dialog result to indicate whether or not "Cancel" was clicked
        Dim clickedButton As Button = TryCast(sender, Button)
        Me.DialogResult = Not clickedButton.IsCancel

        ' Close the window
        Me.Close()
    End Sub
End Class
