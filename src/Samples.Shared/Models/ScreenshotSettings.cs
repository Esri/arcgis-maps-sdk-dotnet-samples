namespace ArcGIS.Samples.Shared.Models
{
    public class ScreenshotSettings
    {
        public bool ScreenshotEnabled { get; set; }
        public string SourcePath { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? ScaleFactor { get; set; }
    }
}