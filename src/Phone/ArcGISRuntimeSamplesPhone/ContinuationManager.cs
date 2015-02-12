using System;
using Windows.ApplicationModel.Activation;

public class ContinuationManager
{

	internal event EventHandler<FileOpenPickerContinuationEventArgs> FilePickerOpened;
	internal event EventHandler<FileSavePickerContinuationEventArgs> FilePickerSaved;

	// Static initialization of the unique instance 
	private static readonly ContinuationManager SingleInstance = new ContinuationManager();

	/// <summary>
	/// For handling FileOpenPicker.PickSingleFileAndContinue
	/// Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn614994.aspx
	/// </summary>
	public static ContinuationManager Current
	{
		get { return SingleInstance; }
	}

	public void OnFilePickerOpened(FileOpenPickerContinuationEventArgs e)
	{
		if (FilePickerOpened != null)
			FilePickerOpened(this, e);
	}

	public void OnFilePickerSaved(FileSavePickerContinuationEventArgs e)
	{
		if (FilePickerSaved != null)
			FilePickerSaved(this, e);
	}

	public void OnContinue(IContinuationActivatedEventArgs e)
	{
		if (e is FileOpenPickerContinuationEventArgs)
			OnFilePickerOpened((FileOpenPickerContinuationEventArgs)e);
		else if (e is FileSavePickerContinuationEventArgs)
			OnFilePickerSaved((FileSavePickerContinuationEventArgs)e);
	}
}