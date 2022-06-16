#!/usr/bin/env python3
import urllib.parse
import sys
from pathlib import Path

import os

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

    for path in Path(wpf_path).rglob('*.cs'):
        try:
            with open(path, 'r') as f:
                lines = f.readlines()
                i = 0
                task_count = 0

                # Use an indexed while loop so we can delete sections of lines
                while i < len(lines):
                    line = lines[i]

                    # Check if the line is the start of the attributes
                    if "System.Threading.Tasks" in line:
                        task_count +=1
                        if task_count>1:
                            print(path)
                    i=i+1
                f.close()
        except:
            print("error with file: "+str(path))

if __name__ == "__main__":
    main()