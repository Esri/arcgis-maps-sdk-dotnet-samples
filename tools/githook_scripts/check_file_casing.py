#!/usr/bin/env python3
import os

def check_file_names(sample_folder):

    files_to_check = []

    for subdir, dirs, files in os.walk(sample_folder):
        for file in files:
            if (file.endswith(".cs") or file.endswith(".xaml")):
                files_to_check.append(file)
  
    for file in files_to_check:
      if file.endswith(".cs"):
          file_cs = file.split(".")[0]
          for file in files_to_check:
              if file.endswith(".xaml"):
                file_xaml = file.split(".")[0]
                if file_cs.lower() == file_xaml.lower() and file_cs != file_xaml:
                    print(f'Error mismatching file casings {file_cs}.xaml.cs, {file_xaml}.xaml - {sample_folder}')
                    return 1
    return 0

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

def main():
        
        script_location = os.path.dirname(os.path.realpath(__file__))
        repo_root = os.path.abspath(os.path.join(script_location, "..", "..", "src"))
        errors_found = 0
        platforms = ["MAUI", "WPF", "WinUI"]
        for platform in platforms:
            samples_folder = get_platform_samples_root(platform, repo_root)
            for category in os.listdir(samples_folder):
              category_folder = os.path.join(samples_folder, category)
              for sample in os.listdir(category_folder):
                sample_folder = os.path.join(category_folder, sample)
                errors_found = errors_found + check_file_names(sample_folder)
        if (errors_found == 0):
            print(errors_found)

if __name__=="__main__":
    main()