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

def write_samples_toc(platform_dir, relative_path_to_samples, samples_in_categories):
    '''
    sample_in_categories is a dictionary of categories, each key is a list of sample_metadata
    platform_dir is where the readme.md file should be written
    '''
    readme_text = "# Table of contents\n\n"

    for category in samples_in_categories.keys():
        readme_text += f"## {category}\n\n"
        for sample in samples_in_categories[category]:
            readme_text += f"* [{sample.friendly_name}]({relative_path_to_samples}/{sample.category}/{sample.formal_name}/readme.md) - {sample.description}\n"
        readme_text += "\n"
    
    readme_path = os.path.join(platform_dir, "../..", "readme.md")
    with open(readme_path, 'w+') as file:
        file.write(readme_text)

def main():
    '''
    Usage: python3 process_metadata.py {operation} {path_to_samples (ends in src)} {path_to_secondary}
    Operations: toc; secondary path is empty
                improve; secondary path is common readme
                sync; keep metadata in sync with readme
    '''

    if len(sys.argv) < 3:
        print("Usage: python3 process_metadata.py {operation} {path_to_samples (ends in src)} {path_to_secondary}")
        print("Operations are toc, improve, and sync; secondary path is path to common readme source for the improve operation.")

    operation = sys.argv[1]        
    sample_root = sys.argv[2]
    common_dir_path = ""
    if operation == "improve":
        common_dir_path = sys.argv[3]

    for platform in ["UWP", "WPF", "Android", "Forms", "iOS"]:
        # make a list of samples, so that build_all_csproj.bat can be produced
        list_of_sample_dirs = []
        list_of_samples = {}
        skipped_categories = False
        for r, d, f in os.walk(get_platform_samples_root(platform, sample_root)):
            if not skipped_categories:
                skipped_categories = True
                continue
            for sample_dir in d:
                # skip category directories
                sample = sample_metadata()
                path_to_readme = os.path.join(r, sample_dir, "readme.md")
                if not os.path.exists(path_to_readme):
                    print(f"skipping path; does not exist: {path_to_readme}")
                    continue
                sample.populate_from_readme(platform, path_to_readme)
                sample.populate_snippets_from_folder(platform, path_to_readme)
                if operation == "improve":
                    sample.try_replace_with_common_readme(platform, common_dir_path, path_to_readme)
                if operation in ["improve", "sync"]:
                    sample.flush_to_json(os.path.join(r, sample_dir, "readme.metadata.json"))
                list_of_sample_dirs.append(sample_dir)
                # track samples in each category to enable TOC generation
                if sample.category in list_of_samples.keys():
                    list_of_samples[sample.category].append(sample)
                else:
                    list_of_samples[sample.category] = [sample]
        # write out samples TOC
        if operation in ["toc", "improve", "sync"]:
            write_samples_toc(get_platform_samples_root(platform, sample_root), get_relative_path_to_samples_from_platform_root(platform), list_of_samples)
    return

if __name__ == "__main__":
    main()