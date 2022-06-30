from http.client import NETWORK_AUTHENTICATION_REQUIRED
from sample_metadata import *
import urllib.parse
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
    if (platform == "FormsAR"):
        return os.path.join(sample_root, "Forms", "AugmentedReality")   
    if (platform == "WinUI"):
        return os.path.join(sample_root, "WinUI", "ArcGISRuntime.WinUI.Viewer", "Samples")     
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
    if (platform == "FormsAR"):
        return "AugmentedReality"
    if (platform == "WinUI"):
        return "ArcGISRuntime.WinUI.Viewer/Samples"
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

    keys = list(samples_in_categories.keys())
    keys.sort()
    for category in keys:
        readme_text += f"## {category}\n\n"
        formal_category = category
        if ' ' in formal_category:
            formal_category = formal_category.title().replace(' ', '')

        samples = list(samples_in_categories[category])
        samples.sort(key=lambda s: s.friendly_name)
        for sample in samples:
            entry_url = f"{relative_path_to_samples}/{formal_category}/{sample.formal_name}"
            entry_url = urllib.parse.quote(entry_url)
            readme_text += f"* [{sample.friendly_name}]({entry_url}) - {sample.description}\n"
        readme_text += "\n"
    readme_text = readme_text[:-1]   

    readme_path = os.path.join(platform_dir, "../..", "readme.md")
    with open(readme_path, 'w+') as file:
        file.write(readme_text)

def update_attribute(sample, sample_dir):
    try:
        # Get the formal name of the sample
        if '\\' in sample_dir:
            name = sample_dir.split('\\')[-1]
        elif  '/' in sample_dir:
            name = sample_dir.split('/')[-1]

        # Get the correct file ending
        if "Xamarin.iOS" in sample_dir or "Xamarin.Android" in sample_dir:
            ending = ".cs"
        else:
            ending = ".xaml.cs"

        # Handle edge case with AR samples
        name = name.replace("NavigateAR", "RoutePlanner").replace("ViewHiddenInfrastructureAR", "PipePlacer")

        # Open the file
        path_to_source = os.path.join(sample_dir, name + ending)
        with open(path_to_source, 'r') as f:
            lines = f.readlines()
            i = 0
            start_found = False

            # Use an indexed while loop so we can delete sections of lines
            while i < len(lines):
                line = lines[i]

                # Check if the line is the start of the attributes
                if ".Sample(" in line and "[" in line:
                    #store the start index
                    start = i
                    start_found = True

                # Check for the end of the attributes
                if "]" in line and start_found:
                    # Store the end index
                    end = i
                    # Delete the existing attributes
                    oldcontent = lines[start:end+1]
                    del lines[start:end+1]

                    # Create the new attributes
                    new_attributes = "    [ArcGISRuntime.Samples.Shared.Attributes.Sample(\n"
                    new_attributes += "        name: \"" + sample.friendly_name + "\",\n"
                    new_attributes += "        category: \"" + sample.category + "\",\n"
                    new_attributes += "        description: \"" + sample.description.replace("\"", "\\\"") + "\",\n"

                    # Add the instructions
                    if type(sample.how_to_use) is str:
                        instructions = sample.how_to_use
                    elif type(sample.how_to_use) is list and len(sample.how_to_use)>0:
                        instructions = sample.how_to_use[0]
                    else:
                        instructions = ""

                    # Instructions can have multiple items, we only add the first one.
                    if "\n" in instructions:
                        instructions = instructions.split("\n")[0]
                    instructions = "        instructions: \"" + instructions.replace("\"", "\\\"") + "\""
                        
                    new_attributes += instructions

                    # Add the tags
                    tags = []
                    if type(sample.keywords) is list and len(sample.keywords)>0:
                        tags = sample.keywords
                        
                    if len(tags)>0:
                        new_attributes += ",\n        tags: new[] { "
                        for tag in tags:
                            new_attributes += "\"" + tag +"\", "
                        # Remove the trailing comma-space
                        new_attributes = new_attributes[:-2]
                        new_attributes += " }"

                    # Add the closing characters
                    new_attributes += ")]\n"

                    if oldcontent != new_attributes:
                        print(oldcontent)
                        print(new_attributes)

                    # Add the new attributes
                    lines.insert(start, new_attributes)

                    # Break and write the revised file.
                    break
                i=i+1
            f.close()

        # Rewrite the file with updated attributes.
        with open(path_to_source, "w") as file:
            file.seek(0)
            file.write(''.join(lines))
            file.close()
    except Exception as e:
        print(e)
        print("Error with sample: "+sample_dir)

def main():
    '''
    Usage: python process_metadata.py {path_to_samples (ends in src)} (optional)
        Location of script being run will be used for a relative path if path to samples is not specified.
    '''

    if len(sys.argv) < 2:
        # get the location of the samples relative to this script in the tools folder
        script_location = os.path.dirname(os.path.realpath(__file__))
        sample_root = os.path.abspath(os.path.join(script_location, "..", "..", "src"))
    else:
        sample_root = sys.argv[1]

    for platform in ["UWP", "WPF", "Android", "Forms", "iOS", "FormsAR", "WinUI"]:
        # make a list of samples, so that build_all_csproj.bat can be produced
        list_of_sample_dirs = []
        list_of_samples = {}
        skipped_categories = False
        for r, d, f in os.walk(get_platform_samples_root(platform, sample_root)):
            if not skipped_categories:
                skipped_categories = True
                continue
            
            d.sort()
            for sample_dir in d:
                # skip category directories
                sample = sample_metadata()
                path_to_readme = os.path.join(r, sample_dir, "readme.md")
                if not os.path.exists(path_to_readme):
                    print(f"skipping path; does not exist: {path_to_readme}")
                    continue
                sample.populate_from_readme(platform, path_to_readme)
                if platform == "FormsAR":
                    sample.category = "Augmented reality"
                sample.populate_snippets_from_folder(platform, path_to_readme)

                # read existing packages from metadata
                path_to_json = os.path.join(r, sample_dir, "readme.metadata.json")
                if os.path.exists(path_to_json):
                    metadata_based_sample = sample_metadata()
                    metadata_based_sample.populate_from_json(path_to_json)
                sample.flush_to_json(path_to_json)

                # update attributes in the sample code files
                update_attribute(sample, os.path.join(r, sample_dir))

                list_of_sample_dirs.append(sample_dir)

                # track samples in each category to enable TOC generation
                if sample.category in list_of_samples.keys():
                    list_of_samples[sample.category].append(sample)
                else:
                    list_of_samples[sample.category] = [sample]
        # write out samples TOC
        if platform != "FormsAR":
            write_samples_toc(get_platform_samples_root(platform, sample_root), get_relative_path_to_samples_from_platform_root(platform), list_of_samples)
    return

if __name__ == "__main__":
    main()