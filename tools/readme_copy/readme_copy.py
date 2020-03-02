#!/usr/bin/env python3
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
def replace_readmes(category, formal_name, sample_root):
    try:
        # Read the readme from the WPF version.
        wpf_path = os.path.join(get_platform_samples_root("WPF", sample_root), category, formal_name, ("readme.md"))
        wpf_file = open(wpf_path, "r")
        wpflines = wpf_file.readlines()
        wpf_file.close()
        print(f"{formal_name} read from WPF")

        # Loop through the other platforms.
        plats = ["UWP", "Android", "iOS", "Forms"]
        for platform in plats:
            platform_path = os.path.join(get_platform_samples_root(platform, sample_root), category, formal_name, ("readme.md"))
            
            # Write the WPF readme to other platform
            with open(platform_path, "r+") as file:
                file.seek(0)
                file.writelines(wpflines)
                file.truncate()
                print(f"{formal_name} written to {platform}")
    except OSError as e:
        print(e.errno)
def main():
    if len(sys.argv) < 4:
        print("Usage: python readme_copy.py {category} {formal name of sample} {path_to_samples (ends in src)}")
        return
    # Get the user arguments.
    category = sys.argv[1]
    formal_name = sys.argv[2]        
    sample_root = sys.argv[3]

    for category in os.listdir(get_platform_samples_root("WPF", sample_root)):
        for sample in os.listdir( os.path.join(get_platform_samples_root("WPF", sample_root), category) ):
            replace_readmes(category, sample, sample_root)

    replace_readmes(category, formal_name, sample_root)
    
if __name__=="__main__":
    main()