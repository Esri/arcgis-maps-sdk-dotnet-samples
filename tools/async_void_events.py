#!/usr/bin/env python3
import urllib.parse
import sys
from pathlib import Path

import os

def replace(platform_path):
    plat_count = 0
    for path in Path(platform_path).rglob('*.cs'):
        try:
            i = 0
            task_count = 0
            with open(path, 'r') as f:
                lines = f.readlines()
                mode = 0
                # Use an indexed while loop so we can delete sections of lines
                while i < len(lines):
                    line = lines[i]

                    if mode == 0:
                        # Check if the line is the start of the attributes
                        if "async void" in line and "Args" in line:
                            spacing = line.split('p')[0]
                            mode = 1
                            trypresent = False
                    if mode == 1:
                        if "await" in line and not trypresent:
                                plat_count += 1
                                print("No try: "+str(os.path.basename(path)))
                                print(line.split("\n")[0])
                        if "try" in line:
                            trypresent = True
                        if line.startswith(spacing+"}"):
                            mode = 0
                            if not trypresent:
                                print("No try: "+str(os.path.basename(path)))
                    i=i+1
                f.close()


            with open(path, "w") as file:
                file.seek(0)
                file.write(''.join(lines))
                file.close()
        except:
            print("error with file: "+str(path))
    print(platform_path+ " instances: "+str(plat_count))
def main():
    '''
    Usage: python async_void_replace.py {path_to_samples (ends in src)} (optional)
        Location of script being run will be used for a relative path if path to samples is not specified.
    '''

   # get the location of the samples relative to this script in the tools folder
    script_location = os.path.dirname(os.path.realpath(__file__))
    sample_root = os.path.abspath(os.path.join(script_location, "..", "src"))

    wpf_path = os.path.join(sample_root, "WPF", "ArcGISRuntime.WPF.Viewer", "Samples")
    winUI_path = os.path.join(sample_root, "WinUI", "ArcGISRuntime.WinUI.Viewer", "Samples")
    forms_path = os.path.join(sample_root, "Forms", "Shared", "Samples")

    #replace(wpf_path)
    replace(winUI_path)
    #replace(forms_path)

if __name__ == "__main__":
    main() 