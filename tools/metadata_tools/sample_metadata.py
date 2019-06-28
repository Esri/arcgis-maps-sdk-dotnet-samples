import json
import os
from distutils.dir_util import copy_tree
from shutil import copyfile

class sample_metadata:
    def __init__(self):
        self.formal_name = ""
        self.friendly_name = ""
        self.sample_unique_id = ""
        self.category = ""
        self.nuget_packages = []
        self.keywords = []
        self.relevant_api = []
        self.since = ""
        self.images = []
        self.source_files = []
        self.redirect_from = []
        self.offline_data = ""
        self.description = ""
        self.how_to_use = []
        self.how_it_works = ""
        self.use_case = ""
        self.data_statement = ""
        self.additional_info = ""
        self.ignore = False
    
    def populate_from_folder(self, folder_path):
        # check for readme in folder

        # check for json in folder


        return
    
    def populate_from_json(self, path_to_json):
        # formal name is the name of the folder containing the json
        pathparts = os.path.split(path_to_json)
        self.formal_name = pathparts[-2]

        # open json file
        with open(path_to_json, 'r') as json_file:
            data = json.load(json_file)
            self.friendly_name = data["title"]
            self.sample_unique_id = data["sample_unique_id"]
            # note: category can also be derived from folder structure
            self.category = data["category"]
            self.nuget_packages = data["packages"]
            self.keywords = data["keywords"]
            self.relevant_api = data["relevant_apis"]
            self.images = data["images"]
            self.source_files = data["snippets"]
            self.redirect_from = data["redirect_from"]
            self.description = data["description"]
            self.ignore = data["ignore"]

        return
    
    def populate_from_readme(self, path_to_readme):
        # formal name is the name of the folder containing the json
        pathparts = sample_metadata.splitall(path_to_readme)
        self.formal_name = pathparts[-2]

        # category is the name of the folder containing the sample folder
        self.category = pathparts[-3]

        # read the readme content into a string
        readme_file = open(path_to_readme, "r")
        readme_contents = readme_file.read()
        readme_file.close()

        # break into sections
        readme_parts = readme_contents.split("\n\n") # a blank line is two newlines

        # extract human-readable name
        self.friendly_name = readme_parts[0].strip("#").strip()
        
        if len(readme_parts) < 5: # old style readme
            # Take just the first description paragraph
            self.description = readme_parts[1]
            self.images = sample_metadata.extract_image_from_image_string(readme_parts[2])
            return
        else:
            self.description = readme_parts[1]
            self.images = sample_metadata.extract_image_from_image_string(readme_parts[2])

            # Read through and add the rest of the sections
            examined_readme_part_index = 2
            current_heading = ""
            para_part_accumulator = []

            while examined_readme_part_index < len(readme_parts):
                current_part = readme_parts[examined_readme_part_index]
                examined_readme_part_index += 1
                if not current_part.startswith("#"):
                    para_part_accumulator.append(current_part)                    
                    continue
                else:
                    # process existing heading, skipping if nothing to add
                    if len(para_part_accumulator) != 0:
                        self.populate_heading(current_heading, para_part_accumulator)
                    # get started with new heading
                    current_heading = current_part
                    para_part_accumulator = []
            # do the last segment
            if current_heading != "" and len(para_part_accumulator) > 0:
                self.populate_heading(current_heading, para_part_accumulator)

        return

    def flush_to_readme(self, path_to_readme):
        # read in readme template
        template_text = ""

        # replace the name

        # replace the image

        # replace or remove 'How to use the sample' - how_to_use

        # replace or remove 'How it works' - how_it_works

        # replace or remove 'Relevant API' - relevant_api

        # replace or remove 'Offline data' - offline_data

        # replace or remove 'About the data' - data_statement

        # replace or remove 'Additional information' - additional_info

        # replace or remove 'Tags' - keywords

        return
    
    def flush_to_json(self, path_to_json):

        data = {}

        data["title"] = self.friendly_name
        data["sample_unique_id"] = self.sample_unique_id
        data["category"] = self.category
        data["keywords"] = self.keywords
        data["relevant_apis"] = self.relevant_api
        data["images"] = self.images
        data["snippets"] = self.source_files
        data["redirect_from"] = self.redirect_from
        data["description"] = self.description
        data["ignore"] = self.ignore

        with open(path_to_json, 'w+') as json_file:
            json.dump(data, json_file, indent=4, sort_keys=True)

        return
    
    def emit_standalone_solution(self, platform, sample_dir, output_root, shared_project_path):
        '''
        Produces a standalone sample solution for the given sample
        platform: one of: Android, iOS, UWP, WPF, XFA, XFI, XFU
        output_root: output folder; should not be specific to the platform
        sample_dir: path to the folder containing the sample's code
        '''
        # create output dir
        output_dir = os.path.join(output_root, platform, self.formal_name)

        if not os.path.exists(output_dir):
            os.makedirs(output_dir)

        # copy template files over - find files in template
        script_dir = os.path.split(os.path.realpath(__file__))[0]
        template_dir = os.path.join(script_dir, "templates", "solutions", platform)
        copy_tree(template_dir, output_dir)
        
        # copy sample files over
        copy_tree(sample_dir, output_dir)

        # copy any out-of-dir files over (e.g. Android layouts, download manager)
        if len(self.source_files) > 0:
            for file in self.source_files:
                if ".." in file:
                    source_path = os.path.join(sample_dir, file)
                    dest_path = os.path.join(output_dir, os.path.split(file)[1])
                    copyfile(source_path, dest_path)

        # TODO

        # accumulate list of source, xaml, axml, and resource files

        # generate list of replacements
        replacements = {}
        replacements["$$project$$"] = self.formal_name
        replacements["$$embedded_resources$$"] = "" # TODO
        replacements["$$source_files$$"] = "" # TODO
        replacements["$$xaml_files$$"] = "" # TODO
        replacements["$$axml_files$$"] = "" # TODO

        # rewrite files in output - replace template fields
        sample_metadata.rewrite_files_in_place(output_dir, replacements)
        return
    
    def rewrite_files_in_place(source_dir, replacements_dict):
        for r, d, f in os.walk(source_dir):
            for sample_dir in d:
                sample_metadata.rewrite_files_in_place(os.path.join(r, sample_dir), replacements_dict)
            for sample_file_name in f:
                sample_file = os.path.join(r, sample_file_name)
                extension = os.path.splitext(sample_file)[1]
                if extension in [".cs", ".xaml", ".sln", ".md", ".csproj", ".shproj", ".axml"]:
                    # open file, read into string
                    original_contents_handle = open(sample_file, "r")
                    original_contents = original_contents_handle.read()
                    original_contents_handle.close()
                    # make replacements
                    new_content = original_contents
                    for tag in replacements_dict.keys():
                        new_content = new_content.replace(tag, replacements_dict[tag])
                    # write out new file
                    if new_content != original_contents:
                        os.remove(sample_file)
                        with open(sample_file, 'w') as rewrite_handle:
                            rewrite_handle.write(new_content)

    
    def splitall(path):
        ## Credits: taken verbatim from https://www.oreilly.com/library/view/python-cookbook/0596001673/ch04s16.html
        allparts = []
        while 1:
            parts = os.path.split(path)
            if parts[0] == path:  # sentinel for absolute paths
                allparts.insert(0, parts[0])
                break
            elif parts[1] == path: # sentinel for relative paths
                allparts.insert(0, parts[1])
                break
            else:
                path = parts[0]
                allparts.insert(0, parts[1])
        return allparts
    
    def extract_image_from_image_string(image_string) -> str:
        '''
        Takes an image string in the form of ![alt-text](path_toImage.jpg)
        or <img src="path_toImage.jpg" width="350"/>
        and returns 'path_toImage.jpg'
        '''

        image_string = image_string.strip()

        if image_string.startswith("!"): # Markdown-style string
            # find index of last )
            close_char_index = image_string.rfind(")")

            # find index of last (
            open_char_index = image_string.rfind("(")

            # return original string if it can't be processed further
            if close_char_index == -1 or open_char_index == -1:
                return image_string

            # read between those chars
            substring = image_string[open_char_index + 1:close_char_index]
            return substring
        else: # HTML-style string
            # find index of src="
            open_match_string = "src=\""
            open_char_index = image_string.rfind(open_match_string)
            
            # return original string if can't be processed further
            if open_char_index == -1:
                return image_string

            # adjust open_char_index to account for search string
            open_char_index += len(open_match_string)
            
            # read from after " to next "
            close_char_index = image_string.find("\"", open_char_index)
            
            # read between those chars
            substring = image_string[open_char_index:close_char_index]
            return substring
    
    def populate_heading(self, heading_part, body_parts):
        '''
        param: heading_part - string starting with ##, e.g. 'Use case'
        param: body_parts - list of constituent strings
        output: determines which field the content belongs in and adds appropriately
                e.g. lists will be turned into python list instead of string
        '''

        # normalize string for easier decisions
        heading_parts = heading_part.strip("#").strip().lower().split()

        # use case
        if "use" in heading_parts and "case" in heading_parts:
            content = "\n".join(body_parts)
            self.use_case = content
            return

        # how to use
        if "use" in heading_parts and "how" in heading_parts:
            content = "\n".join(body_parts)
            self.how_to_use = content
            return

        # how it works
        if "works" in heading_parts and "how" in heading_parts:
            content = "\n".join(body_parts)
            self.how_it_works = content
            return
        
        # relevant API
        if "api" in heading_parts or "apis" in heading_parts:
            api_strings = []
            lines = body_parts[0].split("\n")
            cleaned_lines = []
            for line in lines:
                cleaned_lines.append(line.strip("*").strip("-").strip())
            self.relevant_api = cleaned_lines
            return

        # offline data
        if "offline" in heading_parts:
            content = "\n".join(body_parts)
            self.offline_data = content
            return

        # about the data
        if "data" in heading_parts and "about" in heading_parts:
            content = "\n".join(body_parts)
            self.data_statement = content
            return

        # additional info
        if "additional" in heading_parts:
            content = "\n".join(body_parts)
            self.additional_info = content
            return

        # tags
        if "tags" in heading_parts:
            tags = body_parts[0].split(",")
            cleaned_tags = []
            for tag in tags:
                cleaned_tags.append(tag.strip())
            self.keywords = cleaned_tags
            return