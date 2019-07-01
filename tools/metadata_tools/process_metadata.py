from sample_metadata import *

import os

def get_platform_samples_root(platform, sample_root):
    '''
    Gets the root directory for each platform
    '''
    if (platform == "UWP"):
        return os.path.join(sample_root, "UWP", "ArcGISRuntime.UWP.Viewer", "Samples")
    if (platform == "WPF"):
        return os.path.join(sample_root, "WPF", "ArcGISRuntime.WPF.Viewer", "Samples")
    if (platform == "Android"):
        return os.path.join(sample_root, "Android", "Xamarin.Android", "Samples")
    if (platform == "iOS"):
        return os.path.join(sample_root, "iOS", "Xamarin.iOS", "Samples")
    if (platform == "Forms" or platform in ["XFA", "XFI", "XFU"]):
        return os.path.join(sample_root, "Forms", "Shared", "Samples")    
    raise AssertionError(None, None)

def plat_to_msbuild_string(platform):
    if platform in ["XFU", "UWP"]:
        return "/p:Configuration=Debug,Platform=\"x86\" /p:AppxPackageSigningEnabled=false"
    elif platform in ["XFI", "iOS"]:
        return "/p:Configuration=Debug;Platform=iPhone"
    else:
        return "/p:Configuration=Debug,Platform=\"Any CPU\"  /p:AppxPackageSigningEnabled=false"

def write_build_script(list_of_samples, platform, output_dir):
    '''
    output_dir: platform-specific output folder containing sample solutions
    list_of_samples: flat list of sample formal names; should correspond to directories
    '''
    output_string = "@echo on"
    output_string += "\nREM Set up environment variables for Visual Studio."
    output_string += "\nREM ==================================="
    for sample in list_of_samples:
        output_string += f"\nCALL %ESRI_SAMPLES_TEMP_BUILD_ROOT%\\nuget restore {sample}\\{sample}.sln -PackagesDirectory %ESRI_SAMPLES_TEMP_BUILD_ROOT%\\packages"
    output_string += "\ncall \"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\Tools\\VsDevCmd.bat\""
    output_string += "\n@echo on"
    
    for sample in list_of_samples:
        output_string += f"\nREM Building: {sample}"
        output_string += f"\nmsbuild {plat_to_msbuild_string(platform)} /clp:errorsonly /flp2:errorsonly;logfile=%ESRI_SAMPLES_TEMP_BUILD_ROOT%\\{platform}\\build.log;append {sample}\\{sample}.sln"
    
    file_name = os.path.join(output_dir, "BuildAll_CSharp.bat")

    with open(file_name, 'w+') as output_file:
        output_file.write(output_string)

def main():
    # Take the path to the samples root (ending in src)
    sample_root = "C:\\SamplesDotNET\\src" # TODO
    output_root = "C:\\API\\"
    shared_project_path = "C:\\SamplesDotNET\\src\\ArcGISRuntime.Samples.Shared"
    # TODO - make readme replace a run option, disable by default, parameterize common-samples path
    common_dir_path = "C:\\Users\\nath9278\\Documents\\Dev\\common-samples\\designs"

    for platform in ["UWP", "WPF", "Android", "XFI", "XFA", "XFU", "iOS"]:
        # make a list of samples, so that build_all_csproj.bat can be produced
        list_of_samples = []
        skipped_categories = False
        for r, d, f in os.walk(get_platform_samples_root(platform, sample_root)):
            if not skipped_categories:
                skipped_categories = True
                continue
            for sample_dir in d:
                # skip category directories
                sample = sample_metadata()
                path_to_readme = os.path.join(r, sample_dir, "readme.md")
                sample.populate_from_readme(platform, path_to_readme)
                sample.try_replace_with_common_readme(platform, common_dir_path, path_to_readme)
                sample.flush_to_json(os.path.join(r, sample_dir, "readme.metadata.json"))
                sample.emit_standalone_solution(platform, os.path.join(r, sample_dir), output_root, shared_project_path)
                list_of_samples.append(sample_dir)
        # write out build_all_csproj.bat
        write_build_script(list_of_samples, platform, os.path.join(output_root, platform))
    return

if __name__ == "__main__":
    main()