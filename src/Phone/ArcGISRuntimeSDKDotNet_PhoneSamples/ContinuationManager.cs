using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

public class ContinuationManager
{
    internal event EventHandler<FileOpenPickerContinuationEventArgs> FilePickerOpened;
	internal event EventHandler<FileSavePickerContinuationEventArgs> FilePickerSaved;

    private void OnFilePickerOpened(FileOpenPickerContinuationEventArgs e)
    {
        if (FilePickerOpened != null)
            FilePickerOpened(this, e);
    }

	private void OnFilePickerSaved(FileSavePickerContinuationEventArgs e)
	{
		if (FilePickerSaved != null)
			FilePickerSaved(this, e);
	}

    internal void OnContinue(IContinuationActivatedEventArgs e)
    {
		if (e is FileOpenPickerContinuationEventArgs)
			OnFilePickerOpened((FileOpenPickerContinuationEventArgs)e);
		else if (e is FileSavePickerContinuationEventArgs)
			OnFilePickerSaved((FileSavePickerContinuationEventArgs)e);
    }
}