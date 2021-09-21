import sys
import subprocess
import os

def main():
    '''
    Usage: python sample_sync.py 
    '''

    script_location = os.path.dirname(os.path.realpath(__file__))
    readme_script_folder = os.path.abspath(os.path.join(script_location, "readme_copy"))
    readme_script_path = os.path.join(readme_script_folder, "readme_copy.py")
    metadata_script_folder = os.path.abspath(os.path.join(script_location, "metadata_tools"))
    metadata_script_path = os.path.join(metadata_script_folder, "process_metadata.py")

    print("Copying readmes")
    subprocess.run(["python", readme_script_path])

    print("Updating metadata")
    subprocess.run(["python", metadata_script_path])

    return

if __name__ == "__main__":
    main()