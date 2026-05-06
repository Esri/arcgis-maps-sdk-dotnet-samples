#!/usr/bin/env bash

# Default environment variables
DOTNET_VERSION="${DOTNET_VERSION:-10.0.203}"
RELEASE_VERSION="${RELEASE_VERSION:-300.1.0}"
FRAMEWORK="net${DOTNET_VERSION%%.*}.0-ios"

# Get script directory
SCRIPT_DIR="$( realpath "$(dirname "${BASH_SOURCE[0]}")" )"

# Install the desired dotnet version if not already cached
if [[ -z "${DOTNET_CACHE_FOLDER}" ]]; then
  DOTNET_INSTALL_FOLDER="${WORKSPACE}/.dotnet"
else
  DOTNET_INSTALL_FOLDER="${DOTNET_CACHE_FOLDER}/${DOTNET_VERSION}"
fi

echo "Install folder: ${DOTNET_INSTALL_FOLDER}"
echo $SCRIPT_DIR

if [[ ! -x "${DOTNET_INSTALL_FOLDER}/dotnet" ]]; then
  mkdir -p "${DOTNET_INSTALL_FOLDER}"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "${WORKSPACE}/dotnet-install.sh"
  bash "${WORKSPACE}/dotnet-install.sh" --version "${DOTNET_VERSION}" --install-dir "${DOTNET_INSTALL_FOLDER}"
fi

DOTNET_EXE="${DOTNET_INSTALL_FOLDER}/dotnet"
echo "Installed dotnet at ${DOTNET_EXE}"

"${DOTNET_EXE}" nuget config get ALL --show-path

# Configure NuGet
"${DOTNET_EXE}" nuget list source
CONFIG_FILE="${SCRIPT_DIR}/../NuGet.Config"
"${DOTNET_EXE}" new nugetconfig --force -o "${SCRIPT_DIR}/../"
"${DOTNET_EXE}" nuget list source
if [[ -e "${NUGET_REPO}" ]]; then
  echo "Made it inside the if"
  "${DOTNET_EXE}" nuget add source "${NUGET_REPO}" --configfile "${CONFIG_FILE}"
fi
"${DOTNET_EXE}" nuget list source --configfile "${CONFIG_FILE}"

echo "NUGET_REPO: ${NUGET_REPO}"

export NUGET_PACKAGES="${SCRIPT_DIR}/../.nuget/packages"
export NUGET_HTTP_CACHE_PATH="${SCRIPT_DIR}/../.nuget/cache"
mkdir -p "${NUGET_PACKAGES}"
mkdir -p "${NUGET_HTTP_CACHE_PATH}"

echo "NUGET_PACKAGES: ${NUGET_PACKAGES}"
echo "NUGET_HTTP_CACHE_PATH: ${NUGET_HTTP_CACHE_PATH}"

"${DOTNET_EXE}" nuget config get ALL --show-path

# Install maui workload
"${DOTNET_EXE}" workload install maui --version "${DOTNET_VERSION}"

# Embed API key
if [[ ! -z "${API_KEY}" ]]; then
  sed -i '' "s/\/\/ return \"YOUR_API_KEY_HERE\";/return \"${API_KEY}\";/" \
    ${SCRIPT_DIR}/../src/Samples.Shared/Managers/ApiKeyManager.cs || exit 1

  # Disable API key tab in settings menu
  sed -i '' '/string versionNumber = string.Empty;/a\
    ToolbarItems.Remove(ApiKeyButton);' \
    ${SCRIPT_DIR}/../src/MAUI/Maui.Samples/Views/SettingsPage.xaml.cs || exit 1
fi

# Set license keys
if [[ ! -z "${LICENSE_KEY}" ]]; then
  # License key
  sed -i '' "s/public static string ArcGISLicenseKey { get; } = null;/public static string ArcGISLicenseKey { get; } = \"${LICENSE_KEY}\";/" \
    ${SCRIPT_DIR}/../src/Samples.Shared/Managers/LicenseStrings.cs || exit 1

  # Analysis license key
  sed -i '' "s/public static string ArcGISAnalysisLicenseKey { get; } = null;/public static string ArcGISAnalysisLicenseKey { get; } = \"${ANALYSIS_LICENSE_KEY}\";/" \
    ${SCRIPT_DIR}/../src/Samples.Shared/Managers/LicenseStrings.cs || exit 1

  # Advanced Editing User Type License Key
  sed -i '' "s/public static string ArcGISAdvancedEditingUserTypeLicenseKey { get; } = null;/public static string ArcGISAdvancedEditingUserTypeLicenseKey { get; } = \"${ADVANCED_EDITING_USER_TYPE_LICENSE_KEY}\";/" \
    ${SCRIPT_DIR}/../src/Samples.Shared/Managers/LicenseStrings.cs || exit 1
fi

# Set ArcGISMapsSDKVersion in Directory.Packages.props
export PATH=/opt/homebrew/bin:/usr/local/bin:${PATH} # add homebrew bin to path to use xmlstarlet

xmlstarlet ed -P -L -u '/Project/PropertyGroup/ArcGISMapsSDKVersion' \
  -v ${RELEASE_VERSION} \
  ${SCRIPT_DIR}/../src/Directory.Packages.props || exit 1

# Add an empty bundleid so it can be set during the different builds
/usr/libexec/PlistBuddy -c "add :CFBundleIdentifier string """ \
  ${SCRIPT_DIR}/../src/MAUI/Maui.Samples/Platforms/iOS/Info.plist || exit 1

# Internal Build
/usr/libexec/PlistBuddy -c "set :CFBundleIdentifier com.esri.arcgisruntime.samples.maui-enterprise" \
  ${SCRIPT_DIR}/../src/MAUI/Maui.Samples/Platforms/iOS/Info.plist || exit 1

"${DOTNET_EXE}" publish ${SCRIPT_DIR}/../src/MAUI/Maui.Samples/ArcGIS.Samples.Maui.csproj \
  -f:${FRAMEWORK} \
  -p:ApplicationDisplayVersion=${RELEASE_VERSION} \
  -p:ApplicationVersion=${ABSOLUTE_BUILD_NUM} \
  -p:ArchiveOnBuild=true \
  -p:CodesignKey="${CODESIGN_IPHONE_DISTRIBUTION_INTERNAL}" \
  -p:CodesignProvision="${CODESIGN_PROVISION_INTERNAL}" \
  -p:Configuration="Release" \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:PublishDir=${WORKSPACE}/output/internalBuild/ \
  -p:ValidateXcodeVersion=false

echo "Made it past internal build"

# External Build
/usr/libexec/PlistBuddy -c "set :CFBundleIdentifier com.esri.arcgisruntime.samples.maui" \
  "${SCRIPT_DIR}"/../src/MAUI/Maui.Samples/Platforms/iOS/Info.plist || exit 1

echo "Updated plist:"
cat "${SCRIPT_DIR}"/../src/MAUI/Maui.Samples/Platforms/iOS/Info.plist

"${DOTNET_EXE}" publish "${SCRIPT_DIR}"/../src/MAUI/Maui.Samples/ArcGIS.Samples.Maui.csproj \
  -f:${FRAMEWORK} \
  -p:ApplicationDisplayVersion=${RELEASE_VERSION} \
  -p:ApplicationVersion=${ABSOLUTE_BUILD_NUM} \
  -p:ArchiveOnBuild=true \
  -p:CodesignKey="${CODESIGN_APPLE_DISTRIBUTION}" \
  -p:CodesignProvision="${CODESIGN_PROVISION_APPSTORE}" \
  -p:Configuration="Release" \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:PublishDir=${WORKSPACE}/output/externalBuild/ \
  -p:ValidateXcodeVersion=false