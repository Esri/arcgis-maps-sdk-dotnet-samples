import os, sys
from PIL import Image

def check_file(file):
    image = Image.open(file)
    if (image.size != (800, 600)):
        print(f"{file}: expected 800x600, actually was {image.size}")

def check_directory(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.jpg') or file.endswith('.jpeg'):
                check_file(os.path.join(directory, root, file))

def main():
    if (len(sys.argv) != 2):
        print("Usage: python3 screenshot_check.py C:\\SamplesDotNET\\src\\platform\\viewer.project\\Samples")
        return
    base_path = sys.argv[1]

    check_directory(base_path)

    # Iterate through all the subdirectories, checking for files
    return

if __name__ == "__main__":
    main()