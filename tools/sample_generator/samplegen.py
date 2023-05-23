import shutil
from datetime import datetime
import os
import sys

# Platforms
Platforms = ["WPF", "MAUI", "WinUI"]

def get_platform_root(platform, sample_root):
    '''
    Gets the root directory for each platform
    '''
    if (platform == "WPF"):
        return os.path.join(sample_root, "WPF", "WPF.Viewer")
    if (platform == "WinUI"):
        return os.path.join(sample_root, "WinUI", "ArcGIS.WinUI.Viewer")
    if (platform == "MAUI"):
        return os.path.join(sample_root, "MAUI", "Maui.Samples")
    return ""

def get_proj_file(platform, sample_root):
    '''
    Gets the full path to the csproj/vbproj/projitems file for the specified platform
    '''
    basepath = get_platform_root(platform, sample_root)
    if (platform == "WPF"):
        return os.path.join(basepath, "ArcGIS.WPF.Viewer.NetFramework.csproj")
    if (platform == "WinUI"):
        return os.path.join(basepath, "ArcGIS.WinUI.Viewer.csproj")
    if (platform == "MAUI"):
        return os.path.join(basepath, "ArcGIS.Samples.Maui.csproj")
    return ""

def get_csproj_style_path(category_list, sample_name, file_name):
    '''
    Gets the path in the csproj style, consisting of the categories and file name
    e.g. Samples\Data\EditAndSyncFeatures\EditAndSyncFeatures.jpg
    '''
    components = ["Samples", category_list.title().replace(' ', ''), sample_name, file_name]
    return '\\'.join(components)

def build_csproj_line(category_list, sample_name, platform, entry_type):
    '''
    Returns a string that is a valid csproj/vbproj/projitems entry for a screenshot, metadata.json, codebehind, or xaml entry
    Accepted entry_type values are: metadata, screenshot, code, xaml
    '''

    # First, build the file name
    filename = ""
    if (entry_type == "screenshot"):
        filename = sample_name + ".jpg"
        if (platform == "MAUI"):
            filename = filename.lower()
    elif (entry_type == "code" and platform in ["MAUI", "WPF"]):
        filename = sample_name + ".xaml.cs"
    elif (entry_type == "code"):
        filename = sample_name + ".cs"
    elif (entry_type == "xaml"):
        filename = sample_name + ".xaml"

    # Next, get the msbuild-formatted relative project path
    filepath = get_csproj_style_path(category_list, sample_name, filename) 
    
    # Next, build the start and end tags
    start_tag = ""
    end_tag = ""
    if (platform == "WPF"):
        if (entry_type in ["screenshot"]):
            start_tag = '<Content Include="'
            end_tag = '">\n      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n    </Content>'
        elif (entry_type == "xaml"):
            start_tag = '<Page Include="'
            end_tag = '">\n      <Generator>MSBuild:Compile</Generator>\n      <SubType>Designer</SubType>\n    </Page>'
        elif (entry_type == "code"):
            start_tag = '<Compile Include="'
            end_tag = '">\n      <DependentUpon>' + sample_name + '.xaml' + '</DependentUpon>\n    </Compile>'
    # Return the full string
    return start_tag + filepath + end_tag

def perform_csproj_replace(platforms, root, category_list, sample_name):
    for platform in platforms:
        if platform == "WinUI":
            continue
        path = get_proj_file(platform, root)
        new_contents = []
        with open(path, 'r+') as fd:
            file_contents = fd.readlines()
            for line in file_contents:
                # build the new entry
                if "<!-- Screenshots -->" in line:
                    newtext = build_csproj_line(category_list, sample_name, platform, "screenshot")
                elif "<!-- Sample XAML -->" in line:
                    newtext = build_csproj_line(category_list, sample_name, platform, "xaml")
                elif "<!-- Sample Code -->" in line:
                    newtext = build_csproj_line(category_list, sample_name, platform, "code")
                else:
                    new_contents.append(line.rstrip())
                    continue
                # insert the entry
                new_contents.append(line.rstrip())
                new_contents.append('    ' + newtext)
            # rewrite file
        with open(path, 'w') as fd:
            fd.write('\n'.join(new_contents))

def perform_copy_rewrite(source, destination, replacements):
    contents = ""
    with open(source, 'r+') as fd:
        contents = fd.read()
        for entry in replacements.keys():
            contents = contents.replace(entry, replacements[entry])
    with open(destination, 'w') as fd:
        fd.write(contents)


def orchestrate_file_copy(platforms, root, category_list, sample_name, replacements):
    # get the directory of the python file - it is where the templates are
    template_root = os.path.dirname(os.path.realpath(__file__))
    template_root = os.path.join(template_root, "templates", "default")
    for platform in platforms:
        dest_root = os.path.join(get_platform_root(platform, root), "Samples", category_list.title().replace(' ', ''), sample_name) 
        source = ""
        dest = ""
        # copy the code files
        source = os.path.join(template_root, platform + '.xaml.cs')
        dest = os.path.join(dest_root, sample_name + '.xaml.cs')
        perform_copy_rewrite(source, dest, replacements)
        source = os.path.join(template_root, platform + '.xaml')
        dest = os.path.join(dest_root, sample_name + '.xaml')
        perform_copy_rewrite(source, dest, replacements)
        # copy the image
        source = os.path.join(template_root, "sample_name.jpg")
        if(platform == "MAUI"):
            dest = os.path.join(dest_root, sample_name.lower() + '.jpg')
        else:
            dest = os.path.join(dest_root, sample_name + '.jpg')
        shutil.copyfile(source, dest)
        # copy the readme
        source = os.path.join(template_root, "readme.md")
        dest = os.path.join(dest_root, "readme.md")
        shutil.copyfile(source, dest)
        # copy the metadata
        source = os.path.join(template_root, "readme.metadata.json")
        dest = os.path.join(dest_root, "readme.metadata.json")
        perform_copy_rewrite(source, dest, replacements)
        if(platform == "MAUI"):
            lowercase_screenshot_name = sample_name.lower() + ".jpg"
            uppercase_screenshot_name = sample_name + ".jpg"
            contents = ""
            with open(dest, 'r+') as fd:
                contents = fd.read()
                contents = contents.replace(uppercase_screenshot_name, lowercase_screenshot_name)
            with open(dest, 'w') as fd:
                fd.write(contents)

def ensure_category_present(platforms, root, category_list):
    '''
    Ensures the category folder exists, by making it if it doesn't
    Note: category_list will someday be a list. for now it is the name of a single category
    '''
    for platform in platforms:
        plat_root = get_platform_root(platform, root)
        category_folder = os.path.join(plat_root, "Samples", category_list.title().replace(' ', ''))
        if not os.path.exists(category_folder):
            os.makedirs(category_folder)

def create_sample_directory(platforms, root, category_list, sample_name):
    for platform in platforms:
        plat_root = get_platform_root(platform, root)
        category_folder = os.path.join(plat_root, "Samples", category_list.title().replace(' ', ''))
        sample_folder = os.path.join(category_folder, sample_name)
        if not os.path.exists(sample_folder):
            os.makedirs(sample_folder)

def get_unfriendly_sample_name(friendly_name):
    '''
    Return the unfriendly equivalent to the given friendly name
    '''
    unfriendly_name = []
    last_space = False # last character was a space, capitalize next
    for character in friendly_name:
        if character not in [' ', ',', '(', ')', '\n', '\t', '-']:
            if last_space:
                unfriendly_name.append(character.upper())
            else:
                unfriendly_name.append(character)
            last_space = False
        else:
            last_space = True
    return ''.join(unfriendly_name)

def get_offline_data_attribute(list_of_item_ids) -> str:
    '''
    Take a list of item IDs, create the offline data attribute
    '''
    base_string = '[ArcGIS.Samples.Shared.Attributes.OfflineData($marker)]'
    inner_replacement = ""
    for itemId in list_of_item_ids:
        if inner_replacement != "":
            inner_replacement = inner_replacement + ", "
        inner_replacement = inner_replacement + '"' + itemId + '"'
    return base_string.replace("$marker", inner_replacement)


def new_sample_main(full_directory):
    print(full_directory)
    # Ask for the name of the sample
    friendly_name = input("Enter the friendly name of the sample: ")
    sample_name = input("Enter the 'unfriendly' (space-free) type name of the sample (or blank to autogenerate): ")
    if (sample_name == ""):
        # Print the new sample name
        sample_name = get_unfriendly_sample_name(friendly_name)
    print("Sample will be created as: " + sample_name)

    # Ask for the category
    category_string = input("Enter the sample category: ")

    # Make a folder string for that category
    category_path = category_string.title().replace(' ', '')

    # Ask for description
    sample_description = input("Enter the (brief, 1-line) sample description: ")

    # Ask for map or scene
    is_scene = input("Is this a scene sample? (defaults to no) y/n: ")

    # perform replacement in csproj files
    perform_csproj_replace(Platforms, full_directory, category_path, sample_name)

    # create the category directory if not already present
    ensure_category_present(Platforms, full_directory, category_path)

    # create the sample directory
    create_sample_directory(Platforms, full_directory, category_path, sample_name)

    # collect offline item ids
    itemIds = []
    while(True):
        currentId = input("Enter any item IDs, one at a time. Leave blank for no more items: ")
        if currentId == "":
            break
        else:
            itemIds.append(currentId)

    # Build replacements dictionary
    Replacements = dict()
    Replacements["sample_year"] = str(datetime.today().year)
    Replacements["friendly_name"] = friendly_name
    Replacements["sample_name"] = sample_name
    Replacements["sample_category"] = category_string
    Replacements["sample_description"] = sample_description
    if (is_scene == "y" or is_scene == "Y"):
        Replacements["Geo_View"] = "SceneView"
    else:
        Replacements["Geo_View"] = "MapView"
    Replacements["[offline_data_attr]"] = get_offline_data_attribute(itemIds)

    # create templated files
    orchestrate_file_copy(Platforms, full_directory, category_path, sample_name, Replacements) 

def copy_with_rename(platforms, root, old_cat, new_cat, old_name, new_name, Replacements):
    for platform in platforms:
        file_list = [old_name + ".cs", old_name + ".xaml.cs", old_name + ".xaml", "readme.md", "readme.metadata.json"]
        screenshot_file = old_name + ".jpg"
        if (platform == "MAUI"):
            screenshot_file = screenshot_file.lower()
        file_list.append(screenshot_file)
        for filename in file_list:
            plat_root = get_platform_root(platform, root)
            old_path = os.path.join(plat_root, "Samples", old_cat.title().replace(' ', ''), old_name, filename)
            new_path = os.path.join(plat_root, "Samples", new_cat.title().replace(' ', ''), new_name, filename.replace(old_name, new_name))
            old_content = ""
            if not os.path.isfile(old_path):
                continue
            with open(old_path, 'r+') as fd:
                if not filename.endswith(".jpg"):
                    old_content = fd.read().replace(old_name, new_name)
                    if (platform == "MAUI" and (filename.endswith(".json") or filename.endswith(".md"))):
                        old_content = old_content.replace(old_name.lower() + ".jpg", new_name.lower() + ".jpg")
                    with open(new_path, 'w') as fd:
                        fd.write(old_content)
                else:
                    if (platform == "MAUI"):
                         new_path = os.path.join(plat_root, "Samples", new_cat.title().replace(' ', ''), new_name, new_name.lower() + ".jpg")
                    shutil.copyfile(old_path, new_path)
            # remove the copied file
            os.remove(old_path)
def delete_sample_folder(platforms, root, category, sample_name):
    for platform in platforms:
        plat_root = get_platform_root(platform, root)
        path = os.path.join(plat_root, "Samples", category.title().replace(' ', ''), sample_name)
        # delete any markdown files
        for entry in os.listdir(path):
            if entry.lower().endswith(".md"):
                os.remove(os.path.join(path,entry))
        # delete sample folder if empty (avoid deleting sample folders with unaccounted for files)
        if os.path.isdir(path) and len(os.listdir(path)) < 1:
            shutil.rmtree(path)
        # delete category folder if empty
        cat_path = os.path.join(plat_root, "Samples", category.title().replace(' ', ''))
        if len(os.listdir(cat_path)) < 1:
            shutil.rmtree(cat_path)
    return

def move_sample_csproj(platforms, root, old_cat, new_cat, old_name, new_name):
    for platform in platforms:
        csproj = get_proj_file(platform, root)
        with open(csproj, 'r+') as fd:
            contents = fd.readlines()
            new_contents = []
            for line in contents:
                if old_name in line:
                    # only replace category once to avoid affecting the name of the sample
                    newline = line.replace(old_name, new_name).replace(old_cat, new_cat, 1)
                    new_contents.append(newline.rstrip())
                else:
                    new_contents.append(line.rstrip())
            # rewrite file
            fd.seek(0)
            fd.write('\n'.join(new_contents))
    return

def rename_sample_main(full_directory):
    # Ask for the name of the old sample
    old_name = input("Please enter the name of the old sample, as it appears in the file system: ")

    # Ask for the category of the old sample
    old_cat = input("Please enter the category of the old sample, as it appears in the file system: ")

    # Ask for the name of the new sample
    new_name = input("Please enter the name of the new sample, as it will appear in the file system: ")

    # Ask for the category of the new sample
    new_cat = input("Please enter the new category of the sample, as it will appear in the file system: ")

    # Create the new category directory if needed
    ensure_category_present(Platforms, full_directory, new_cat) 

    # Create the new sample directory
    create_sample_directory(Platforms, full_directory, new_cat, new_name)

    # Copy the files to the new directory with changes
    replacements = dict()
    replacements[old_name] = new_name
    replacements[old_cat] = new_cat
    copy_with_rename(Platforms, full_directory, old_cat, new_cat, old_name, new_name, replacements)

    # Delete existing sample folder and category folder if empty
    delete_sample_folder(Platforms, full_directory, old_cat, old_name)

    # Update the csproj file
    move_sample_csproj(Platforms, full_directory, old_cat, new_cat, old_name, new_name)
    return
def rename_sample_alt(full_directory, command):
    # Ask for full details
    if (command == None):
        command = input("Enter the old and new category and name in format old_cat,old_name-new_cat,new_name: ")
    old, new = command.split("-")
    old_cat, old_name = old.split(",")
    old_cat, old_name = old_cat.strip(), old_name.strip()
    new_cat, new_name = new.split(",")
    new_cat, new_name = new_cat.strip(), new_name.strip()
    ensure_category_present(Platforms, full_directory, new_cat)
    create_sample_directory(Platforms, full_directory, new_cat, new_name)
    replacements = dict()
    replacements[old_name] = new_name
    copy_with_rename(Platforms, full_directory, old_cat, new_cat, old_name, new_name, replacements)
    delete_sample_folder(Platforms, full_directory, old_cat, old_name)
    move_sample_csproj(Platforms, full_directory, old_cat, new_cat, old_name, new_name)
    return

def main():
    # Ask if the user wants to rename a sample or copy
    if len(sys.argv) < 3:
        print("Usage: samplegen.py /path/to/samples/directory -[mode] (note: dir should end with 'src')")
        print("Mode is -[r]ename to rename a sample (guided), -[n]ew to create new, -c to rename (one-line-entry)")
        return
    op = sys.argv[2]
    if 'r' in op:
        rename_sample_main(sys.argv[1])
    elif 'c' in op:
        if len(sys.argv) > 3:
            rename_sample_alt(sys.argv[1], sys.argv[3])
        else:
            rename_sample_alt(sys.argv[1], None)
    else:
        new_sample_main(sys.argv[1])

if __name__=="__main__":
    main()
