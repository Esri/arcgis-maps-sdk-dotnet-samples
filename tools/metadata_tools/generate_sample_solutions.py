from sample_metadata import *
import sys

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

def get_relative_path_to_samples_from_platform_root(platform):
    '''
    Returns the path from the platform's readme.md file to the folder containing the sample categories
    For use in sample TOC generation
    '''
    if (platform == "UWP"):
        return "ArcGISRuntime.UWP.Viewer/Samples"
    if (platform == "WPF"):
        return "ArcGISRuntime.WPF.Viewer/Samples"
    if (platform == "Android"):
        return "Xamarin.Android/Samples"
    if (platform == "iOS"):
        return "Xamarin.iOS/Samples"
    if (platform == "Forms" or platform in ["XFA", "XFI", "XFU"]):
        return "Shared/Samples"
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
    output_string += "\ncall \"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\Common7\\Tools\\VsDevCmd.bat\""
    output_string += "\nREM ==================================="
    for sample in list_of_samples:
        output_string += f"\nCALL %ESRI_SAMPLES_TEMP_BUILD_ROOT%\\nuget restore {sample}\\{sample}.sln -PackagesDirectory %ESRI_SAMPLES_TEMP_BUILD_ROOT%\\packages"
    output_string += "\n@echo on"
    
    for sample in list_of_samples:
        output_string += f"\nREM Building: {sample}"
        output_string += f"\nmsbuild {plat_to_msbuild_string(platform)} /clp:errorsonly /flp2:errorsonly;logfile=%ESRI_SAMPLES_TEMP_BUILD_ROOT%\\{platform}\\build.log;append {sample}\\{sample}.sln"
    
    file_name = os.path.join(output_dir, "BuildAll_CSharp.bat")

    with open(file_name, 'w+') as output_file:
        output_file.write(output_string)

def main():
    '''
    usage: python3 generate_sample_solutions.py {platform} {path_to_samples} {output_root}
    '''
    # Take the path to the samples root (ending in src)
    print(sys.argv)
    platform = sys.argv[1]
    sample_root = sys.argv[2] # TODO
    output_root = sys.argv[3]

    if not platform in ["UWP", "WPF", "Android", "iOS", "XFU", "XFA", "XFI"]:
        print("Platform must be UWP, WPF, Android, iOS, XFA, XFI, or XFU")
    if not sample_root.endswith("src"):
        print("sample dir should end in src")

    # make a list of samples, so that build_all_csproj.bat can be produced
    list_of_sample_dirs = []
    list_of_samples = {}
    skipped_categories = False
    sample_root = get_platform_samples_root(platform, sample_root)
    for r, d, f in os.walk(sample_root):
        if not skipped_categories:
            skipped_categories = True
            continue
        for sample_dir in d:
            # skip category directories
            sample = sample_metadata()
            path_to_json = os.path.join(r, sample_dir, "readme.metadata.json")
            if not os.path.exists(path_to_json):
                print(f"skipping path; does not exist: {path_to_json}")
                continue
            sample.populate_from_json(path_to_json)
            if platform == "Forms":
                for sub_plat in ["XFA", "XFI", "XFU"]:
                    sample.emit_standalone_solution(sub_plat, os.path.join(r, sample_dir), output_root)
            else:
                sample.emit_standalone_solution(platform, os.path.join(r, sample_dir), output_root)
            list_of_sample_dirs.append(sample_dir)
            # track samples in each category to enable TOC generation
            if sample.category in list_of_samples.keys():
                list_of_samples[sample.category].append(sample)
            else:
                list_of_samples[sample.category] = [sample]
    # write out build_all_csproj.bat
    if platform == "Forms":
        for sub_plat in ["XFA", "XFI", "XFU"]:
            write_build_script(list_of_sample_dirs, sub_plat, os.path.join(output_root, sub_plat))
    else:
        write_build_script(list_of_sample_dirs, platform, os.path.join(output_root, platform))

if __name__ == "__main__":
    main()