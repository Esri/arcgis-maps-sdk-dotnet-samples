#!/usr/bin/env python3
import sys
import os
import copy

excluded_samples = [
    ("ChangeBasemap", "WinUI")
]

def get_platform_samples_root(platform, sample_root):
    '''
    Gets the root directory for each platform
    '''
    if (platform == "WPF"):
        return os.path.join(sample_root, "WPF", "WPF.Viewer", "Samples")
    if (platform == "WinUI"):
        return os.path.join(sample_root, "WinUI", "ArcGIS.WinUI.Viewer", "Samples")
    if (platform == "MAUI"):
        return os.path.join(sample_root, "MAUI", "Maui.Samples", "Samples")
    raise AssertionError(None, None)
def replace_readmes(category, formal_name, sample_root):
    wpfcontent = None
    try:
        # Read the readme from the WPF version.
        wpf_path = os.path.join(get_platform_samples_root("WPF", sample_root), category, formal_name, ("readme.md"))
        wpf_file = open(wpf_path, "r")
        wpfcontent = wpf_file.read()
        wpf_file.close()
    except OSError as e:
        print(f"File: {formal_name} Error: {e.strerror} WPF read error")

    if wpfcontent is None:
        return

    # Loop through the other platforms.
    plats = ["MAUI", "WinUI"]
    for platform in plats:
        # Skip local server for non WinUI platforms.
        if not platform == "WinUI" and category == "LocalServer":
            continue

        # Skip excluded samples
        if (formal_name, platform) in excluded_samples:
            continue

        # Copy the original WPF text into a new string
        platformcontent = copy.copy(wpfcontent)

        # Fix the guide doc url for the platform
        platformcontent = platformcontent.replace("wpf/guide", str.lower(platform)+"/guide")
        platformcontent = platformcontent.replace("wpf/sample-code/", str.lower(platform)+"/sample-code/")

        # Ensure MAUI image name is lowercase
        if platform == "MAUI":
            platformcontent = platformcontent.replace(formal_name+".jpg", str.lower(formal_name)+".jpg")

        # For other changes that need to be made.
        #platformcontent = platformcontent.replace("oldlink", "newlink")

        # Change `click` to `tap` for mobile platforms
        if  not platform == "WinUI":
            platformcontent = platformcontent.replace("click ", "tap ")
            platformcontent = platformcontent.replace("Click ", "Tap ")
            platformcontent = platformcontent.replace("clicked ", "tapped ")
            platformcontent = platformcontent.replace("Clicked ", "Tapped ")

        try:
            # Write the WPF readme to other platform
            platform_path = os.path.join(get_platform_samples_root(platform, sample_root), category, formal_name, ("readme.md"))
            with open(platform_path, "r+") as file:
                file.seek(0)
                file.write(platformcontent)
                file.truncate()
        except OSError as e:
            print(f"File: {formal_name} Error: {e.strerror} Platform: {platform}")
def main():
    if len(sys.argv) == 4:
        # Get the user arguments.
        category = sys.argv[1]
        formal_name = sys.argv[2]        
        sample_root = sys.argv[3]
        replace_readmes(category, formal_name, sample_root)
    elif len(sys.argv) <= 2:

        if len(sys.argv) == 1:
            # get the location of the samples relative to this script in the tools folder
            script_location = os.path.dirname(os.path.realpath(__file__))
            sample_root = os.path.abspath(os.path.join(script_location, "..", "..", "src"))
        else:
            sample_root = sys.argv[1]
        for category in os.listdir(get_platform_samples_root("WPF", sample_root)):
            for sample in os.listdir( os.path.join(get_platform_samples_root("WPF", sample_root), category) ):
                replace_readmes(category, sample, sample_root)
    else:
        print("Usage for single sample: python readme_copy.py {category} {formal name of sample} {path_to_samples (ends in src)}")
        print("Usage for all samples: python readme_copy.py {path_to_samples (ends in src)}")
        return

if __name__=="__main__":
    main()