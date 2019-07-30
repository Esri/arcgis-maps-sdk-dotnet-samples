import os, sys
try:
    from PIL import Image
except:
    print("There was an error. Do you have Pillow installed?")
    exit()

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
        print("Usage: python screenshot_check.py C:\\SamplesDotNET\\src\\platform\\viewer.project\\Samples")
        return
    base_path = sys.argv[1]

    check_directory(base_path)
    return

if __name__ == "__main__":
    main()