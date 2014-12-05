using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

public class ContinuationManager
{
    internal event EventHandler<FileOpenPickerContinuationEventArgs> FilePickerOpened;
	internal event EventHandler<FileSavePickerContinuationEventArgs> FilePickerSaved;
	internal event EventHandler<FolderPickerContinuationEventArgs> FolderPicker;

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

	private void OnFolderPicker(FolderPickerContinuationEventArgs e)
	{
		if (FolderPicker != null)
			FolderPicker(this, e);
	}

    internal void OnContinue(IContinuationActivatedEventArgs e)
    {
		if (e is FileOpenPickerContinuationEventArgs)
			OnFilePickerOpened((FileOpenPickerContinuationEventArgs)e);
		else if (e is FileSavePickerContinuationEventArgs)
			OnFilePickerSaved((FileSavePickerContinuationEventArgs)e);
		else if (e is FolderPickerContinuationEventArgs)
			OnFolderPicker((FolderPickerContinuationEventArgs)e);

    }
}