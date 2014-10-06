using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

public class ContinuationManager
{
    internal event EventHandler<FileOpenPickerContinuationEventArgs> FilePickerOpened;

    private void OnFilePickerOpened(FileOpenPickerContinuationEventArgs e)
    {
        if (FilePickerOpened != null)
            FilePickerOpened(this, e);
    }

    internal void OnContinue(IContinuationActivatedEventArgs e)
    {
        if (e is FileOpenPickerContinuationEventArgs)
            OnFilePickerOpened((FileOpenPickerContinuationEventArgs)e);
    }
}