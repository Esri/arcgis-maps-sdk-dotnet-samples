#!/bin/bash
{
  # exit when any command fails
  set -e
  
  # Helper functions ::
  
  function _copy_nugets {
    # Nuget server path from which nugets will be copied to local source
    local nuget_path="/net/runtime.esri.com/windows/api_dotnet/${release_ver}/${build_num}/output/dotnet_AnyCPU_Release/"
    #Copying nugets
    echo "Fetching latest nugets from ${nuget_path}"
    local tempInstallLoc=${HOME}/Desktop/nuget
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

    # Add API key to API Key manager file in order to access basemaps (only necessary when building)
    echo "Adding API Key"
    sed -ie "s/\/\/ return \"YOUR_API_KEY_HERE\";/return \"${apiKey}\";/" "${WORKSPACE}/src/Samples.Shared/Managers/ApiKeyManager.cs"

    # dotnet publish command responsible 
    echo "Test Flight (ipa) Build Process"
    dotnet publish "${WORKSPACE}/src/MAUI/Maui.Samples/ArcGIS.Samples.Maui.csproj" -f:net8.0-ios -p:Configuration="Release" -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="${codesignKey_VAR}" -p:CodesignProvision="${codesignProvision_VAR}" -p:ApplicationDisplayVersion=${release_ver} -p:ApplicationVersion=${build_num}
    _build_project

}
