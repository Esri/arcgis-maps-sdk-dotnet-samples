using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS.Samples.Shared.Managers
{
    internal static partial class LicenseStrings
    {
        // Optional license strings
        public static string ArcGISLicenseKey { get; } = null; // ArcGIS SDK License Key

        public static string ArcGISUtilityNetworkLicenseKey { get; } = null; // Utility Network Extension License Key

        public static string ArcGISAnalysisLicenseKey { get; } = null; // Spatial Analyst Extension License Key

        public static string ArcGISAdvancedEditingUserTypeLicenseKey { get; } = null; // Advanced Editing User Type Extension License Key

        public static string[] ExtensionLicenses => new[] { ArcGISUtilityNetworkLicenseKey, ArcGISAnalysisLicenseKey, ArcGISAdvancedEditingUserTypeLicenseKey}.Where(l => !string.IsNullOrEmpty(l)).ToArray();
    }
}
