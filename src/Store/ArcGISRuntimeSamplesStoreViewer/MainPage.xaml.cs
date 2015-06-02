using System;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
 
namespace ArcGISRuntime.Samples.StoreViewer
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			DataContext = AppState.Current;
			SampleFrame.Navigated += SampleFrame_Navigated;
			SampleFrame.Navigate(typeof(SampleListPage));
		}

		void SampleFrame_Navigated(object sender, NavigationEventArgs e)
		{
			AppState.Current.CanGoBack = SampleFrame.CanGoBack;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			if(SampleFrame.CanGoBack)
			{
				SampleFrame.GoBack();
				AppState.Current.SetDefaultTitle();
			}
		}
	}

	public class AppState : System.ComponentModel.INotifyPropertyChanged
	{
		private static AppState m_Current;
		public static AppState Current
		{
			get
			{
				if (m_Current == null)
				{
					m_Current = new AppState();
					m_Current.SetDefaultTitle();
				}
				return m_Current;
			}
		}
		public void SetDefaultTitle()
		{
			CurrentSampleTitle = "ArcGIS Runtime SDK for .NET - Samples";
		}
		private string m_CurrentSampleTitle;

		public string CurrentSampleTitle
		{
			get { return m_CurrentSampleTitle; }
			set { m_CurrentSampleTitle = value; OnPropertyChanged(); }
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}


		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		private bool m_CanGoBack;

		public bool CanGoBack
		{
			get { return m_CanGoBack; }
			set { m_CanGoBack = value; OnPropertyChanged(); }
		}

	}
}
