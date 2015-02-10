using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to print a map using client-side capabilities of the PrintDialog class.
    /// </summary>
    /// <title>Client Printing</title>
    /// <category>Printing</category>
    public partial class ClientPrinting : UserControl
    {
        /// <summary>Construct Client Printing sample control</summary>
        public ClientPrinting()
        {
            InitializeComponent();
        }

        private void ActivatePrintPreview(object sender, RoutedEventArgs e)
        {
            previewPanel.Visibility = Visibility.Visible;
        }

        private void DeactivatePrintPreview(object sender, RoutedEventArgs e)
        {
            previewPanel.Visibility = Visibility.Collapsed;
        }

        private void OnPrint(object sender, RoutedEventArgs e)
        {
            printControl.Print();
        }
    }


    /// <summary>Collection of PreviewSize (creatable in XAML)</summary>
    public class PreviewSizes : ObservableCollection<PreviewSize> { }

    /// <summary>Represents a Preview Size (creatable in XAML)</summary>
    public class PreviewSize
    {
        /// <summary>ID or Name of the preview size</summary>
        public string Id { get; set; }
        /// <summary>Width of the preview size</summary>
        public double Width { get; set; }
        /// <summary>Height of the preview size</summary>
        public double Height { get; set; }
    }


    // Main print control that displays the map using the print template
    internal class MapPrinter : Control, INotifyPropertyChanged
    {
        public MapPrinter()
        {
            _isPrinting = false;
            _attributionItems = new ObservableCollection<string>();
            DataContext = this; // simplify binding in print styles

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        // Executed when the print template changed
        public override async void OnApplyTemplate()
        {
            var extent = (PrintMapView != null) ? PrintMapView.Extent : null;

            base.OnApplyTemplate();

            PrintMapView = GetTemplateChild("PrintMapView") as MapView;
            if (PrintMapView.Extent == null)
            {
                PrintMapView.MaximumExtent = BaseMapView.MaximumExtent;
                PrintMapView.MaxScale = BaseMapView.MaxScale;
                PrintMapView.MinScale = BaseMapView.MinScale;
				PrintMapView.SetRotation(BaseMapView.Rotation);
                PrintMapView.WrapAround = BaseMapView.WrapAround;
                PrintMapView.Map = BaseMapView.Map;

                await PrintMapView.LayersLoadedAsync();
            }

            if (extent != null)
                PrintMapView.SetView(extent);
            else if (BaseMapView != null)
				PrintMapView.SetView(BaseMapView.Extent ?? (BaseMapView.Map.InitialViewpoint == null ? null : BaseMapView.Map.InitialViewpoint.TargetGeometry));

            AttributionItems = new ObservableCollection<string>(
                PrintMapView.Map.Layers.Where(layer => layer.IsVisible)
                .Select(CopyrightText).Where(cp => !string.IsNullOrEmpty(cp))
                .Select(cp => cp.Trim()).Distinct());
        }

        // BaseMapView to print (Dependency Property)
        public MapView BaseMapView
        {
            get { return (MapView)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register("BaseMapView", typeof(MapView), typeof(MapPrinter), new PropertyMetadata(null, OnBaseMapViewChanged));

        private static void OnBaseMapViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapPrinter = d as MapPrinter;
            var newBaseMapView = e.NewValue as MapView;
            if (mapPrinter != null)
            {
                if (newBaseMapView != null && mapPrinter.PrintMapView != null)
					mapPrinter.PrintMapView.SetView(newBaseMapView.Extent ?? (newBaseMapView.Map.InitialViewpoint == null ? null : newBaseMapView.Map.InitialViewpoint.TargetGeometry));
                if (newBaseMapView == null && mapPrinter.IsPrinting)
                    mapPrinter.IsCancelingPrint = true;
            }
        }

        // Title of the print document (Dependency Property)
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MapPrinter), new PropertyMetadata("Map Document"));

        // Flag indicating that the map must be rotated 90°
        public bool RotateMap
        {
            get { return (bool)GetValue(RotateMapProperty); }
            set { SetValue(RotateMapProperty, value); }
        }

        public static readonly DependencyProperty RotateMapProperty =
                DependencyProperty.Register("RotateMap", typeof(bool), typeof(MapPrinter), new PropertyMetadata(false, OnRotateMapChanged));

        private static void OnRotateMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapPrinter = d as MapPrinter;
            if (mapPrinter != null)
                mapPrinter.PrintMapView.SetRotation((bool)e.NewValue ? -90 : 0);
        }

        private bool _isPrinting;
        // Indicates if a print task is going on.
        public bool IsPrinting
        {
            get { return _isPrinting; }
            private set
            {
                if (value != _isPrinting)
                {
                    _isPrinting = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // Attribution Items
        private ObservableCollection<string> _attributionItems;
        public ObservableCollection<string> AttributionItems
        {
            get { return _attributionItems; }
            private set
            {
                if (value != _attributionItems)
                {
                    _attributionItems = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // The print map (defined in the print template)
        public MapView PrintMapView { get; private set; }

        // Gets the current date/time.
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        // Start the print process (by delegating either to the Silverlight print engine or to the WPF print engine)
        public void Print()
        {
            if (IsPrinting)
                return;

            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true)
                    return;

                BeginPrint();

                if (!IsCancelingPrint)
                    printDialog.PrintVisual(this, Title);

                EndPrint(null);
            }
            catch (Exception e)
            {
                EndPrint(e);
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Notifies the property changed.
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        // Internal methods/properties
        internal bool IsCancelingPrint { get; set; }

        internal void BeginPrint()
        {
            IsCancelingPrint = false;
            IsPrinting = true;
            NotifyPropertyChanged("Now"); // in case time is displayed
        }

        internal void EndPrint(Exception error)
        {
            if (error != null && !IsCancelingPrint)
                MessageBox.Show(string.Format("Error during print: {0}", error.Message));

            IsPrinting = false;
            IsCancelingPrint = false;
        }

        private string CopyrightText(Layer lyr)
        {
            if ((lyr is IQueryCopyright) && (PrintMapView != null))
            {
                return ((IQueryCopyright)lyr).QueryCopyright(PrintMapView.Extent, PrintMapView.Scale);
            }
            else if (lyr is ICopyright)
            {
                return ((ICopyright)lyr).CopyrightText;
            }
            else
                return null;
        }
    }
}
