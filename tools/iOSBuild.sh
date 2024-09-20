#!/bin/bash
{
  # exit when any command fails
  set -e
  
  # Helper functions ::
  
  function _copy_nugets {
    # Nuget server path from which nugets will be copied to local source
    local nuget_path="/net/runtime.esri.com/windows/api_dotnet/${BUILDVER}/${BUILDNUM}/output/dotnet_AnyCPU_Release/"
    #Copying nugets
    echo "Fetching latest nugets from ${nuget_path}"
    local tempInstallLoc=/tmp/nuget/Desktop/nuget
    rm -rf ${tempInstallLoc}
    mkdir -p ${tempInstallLoc}
    rsync ${nuget_path}/NuGet/ ${tempInstallLoc}
  }

  # Start script::

    # If Help flag is not used and all other arguments are present

    # Copy nugets to local directory
    _copy_nugets

    # Adding the bundle identifier to the plist
    echo "Adding Bundle ID to Plist"
    /usr/libexec/PlistBuddy -c "add :CFBundleIdentifier string ${bundleID}" "${WORKSPACE}/src/MAUI/Maui.Samples/Platforms/iOS/Info.plist"
   
    echo "Clearing nuget cache"
    dotnet nuget locals all -c

    # Updating the Release Version in the central package manager to ensure current release nugets are referenced
    echo "Updating Release verison in Central Package Manager"
    xmlstarlet ed -P -L -u '/Project/PropertyGroup/ArcGISMapsSDKVersion' -v $release_ver "${WORKSPACE}/src/Directory.Packages.props"

    # Add necessary keys to API Key Manager & License Key Manager file in order to access basemaps (only necessary when building)
    echo "Adding API Key"
    sed -ie "s/\/\/ return \"YOUR_API_KEY_HERE\";/return \"${API_KEY}\";/" "${WORKSPACE}/src/Samples.Shared/Managers/ApiKeyManager.cs"

    echo "Adding License Key"
    sed -ie "s/public static string ArcGISLicenseKey { get; } = null; \/\/ ArcGIS SDK License Key/public static string ArcGISLicenseKey { get; } = ${LICENSE_KEY}; \/\/ ArcGIS SDK License Key/" "${WORKSPACE}/src/Samples.Shared/Managers/LicenseStrings.cs"
    
    # Disable API key tab in settings menu
    echo "Injecting line that will remove the API Key Settings menu tab"
    sed -i'' -e  '/string versionNumber = string.Empty;/a\
            ToolbarItems.Remove(ApiKeyButton);
            ' "${WORKSPACE}/src/MAUI/Maui.Samples/Views/SettingsPage.xaml.cs"

    # dotnet publish command responsible 
    echo "Test Flight (ipa) Build Process"
    dotnet publish "${WORKSPACE}/src/MAUI/Maui.Samples/ArcGIS.Samples.Maui.csproj" -f:net8.0-ios -p:Configuration="Release" -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="${codesignKey_VAR}" -p:CodesignProvision="${codesignProvision_VAR}" -p:ApplicationDisplayVersion=${release_ver} -p:ApplicationVersion=${build_num}

}
