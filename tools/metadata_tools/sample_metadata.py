import json
import os
import re

class sample_metadata:
    
    def reset_props(self):
        self.formal_name = ""
        self.friendly_name = ""
        self.category = ""
        self.keywords = []
        self.relevant_api = []
        self.since = ""
        self.images = []
        self.source_files = []
        self.redirect_from = []
        self.offline_data = []
        self.description = ""
        self.how_to_use = []
        self.how_it_works = ""
        self.use_case = ""
        self.data_statement = ""
        self.Additional_info = ""
        self.ignore = False

    def __init__(self):
        self.reset_props()
    
    def populate_from_json(self, path_to_json):
        # formal name is the name of the folder containing the json
        pathparts = sample_metadata.splitall(path_to_json)
        self.formal_name = pathparts[-2]

        # open json file
        with open(path_to_json, 'r') as json_file:
            data = json.load(json_file)
            keys = data.keys()
            for key in ["category", "keywords", "images", "redirect_from", "description", "ignore"]:
                if key in keys:
                    setattr(self, key, data[key])
            if "title" in keys:
                self.friendly_name = data["title"]
            if "relevant_apis" in keys:
                self.relevant_api = data["relevant_apis"]
            if "snippets" in keys:
                self.source_files = data["snippets"]

        return
    
    def populate_from_readme(self, platform, path_to_readme):
        # formal name is the name of the folder containing the json
        pathparts = sample_metadata.splitall(path_to_readme)
        self.formal_name = pathparts[-2]

        # populate redirect_from; it is based on a pattern
        real_platform = platform
        redirect_string = f"/net/latest/{real_platform.lower()}/sample-code/{self.formal_name.lower()}.htm"
        self.redirect_from.append(redirect_string)

        if self.formal_name == "DisplayDeviceLocation":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/display-device-location/")

        if self.formal_name == "ToggleBetweenFeatureRequestModes":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/servicefeaturetablenocache.htm")
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/servicefeaturetablemanualcache.htm")
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/servicefeaturetablecache.htm")

        if self.formal_name == "DisplayFeatureLayers":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/featurelayergeopackage.htm")
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/featurelayergeodatabase.htm")
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/featurelayershapefile.htm")

        if self.formal_name == "GenerateGeodatabaseReplica":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/generategeodatabase.htm")

        if self.formal_name == "DisplayClusters":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/displayclusters.htm")

        if self.formal_name == "ConfigureClusters":
            self.redirect_from.append(f"/net/{real_platform.lower()}/sample-code/configureclusters.htm")

        # category is the name of the folder containing the sample folder
        self.category = pathparts[-3]

         # Correct category metadata for categories with spaces
        self.category = self.category.replace("LocalServer", "Local Server").replace("NetworkAnalysis", "Network analysis").replace("UtilityNetwork", "Utility network").replace("AugmentedReality", "Augmented reality")

        # read the readme content into a string
        readme_contents = ""
        try:
            readme_file = open(path_to_readme, "r")
            readme_contents = readme_file.read()
            readme_file.close()
        except Exception as err:
            # not a sample, skip
            print(f"Error populating sample from readme - {path_to_readme} - {err}")
            return

        # break into sections
        readme_parts = readme_contents.split("\n\n") # a blank line is two newlines

        # extract human-readable name
        title_line = readme_parts[0].strip()
        if not title_line.startswith("#"):
            title_line = title_line.split("#")[1]
        self.friendly_name = title_line.strip("#").strip()
        
        if len(readme_parts) < 3:
            # can't handle this, return early
            return
        if len(readme_parts) < 5: # old style readme
            # Take just the first description paragraph
            self.description = readme_parts[1]
            self.images.append(sample_metadata.extract_image_from_image_string(readme_parts[2]))
            return
        else:
            self.description = readme_parts[1]
            self.images.append(sample_metadata.extract_image_from_image_string(readme_parts[2]))

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

    def flush_to_json(self, path_to_json):

        data = {}

        data["title"] = self.friendly_name
        data["category"] = self.category
        data["keywords"] = self.keywords
        data["relevant_apis"] = self.relevant_api
        data["images"] = self.images
        data["snippets"] = self.source_files
        data["redirect_from"] = self.redirect_from
        data["description"] = self.description
        data["ignore"] = self.ignore
        data["offline_data"] = self.offline_data
        data["formal_name"] = self.formal_name

        with open(path_to_json, 'w+') as json_file:
            json.dump(data, json_file, indent=4, sort_keys=True)

        return

    def populate_snippets_from_folder(self, platform, path_to_readme):
        '''
        Take a path to a readme file
        Populate the snippets from: any .xaml, .cs files in the directory; 
        '''
        # populate files in the directory
        sample_dir = os.path.split(path_to_readme)[0]
        for file in os.listdir(sample_dir):
            if os.path.splitext(file)[1] in [".xaml", ".cs"]:
                self.source_files.append(file)        
        # order the source files such that the .cs file appears first
        self.source_files.sort(reverse=True)

    def populate_snippets_from_class(self, platform, path_to_readme):
        '''
        Take a path to a readme file
        Populate the snippets from the sample .cs file;
        '''
        # populate from .cs files in the directory
        sample_dir = os.path.split(path_to_readme)[0]
        additionalFiles = []
        for file in os.listdir(sample_dir):
            if os.path.splitext(file)[1] in [".cs"]:
                class_contents = ""
                try:
                    class_file = open(os.path.join(sample_dir, file), "r")
                    class_contents = class_file.readlines()
                    class_file.close()
                except Exception as err:
                    print(f"Error populating metadata from sample - {file} - {err}")
                    return
                # Loop through lines in the class file to check for any additional files such as helpers or converters.
                for line in class_contents:
                    if "ArcGIS.Samples.Shared.Attributes.ClassFile" in line:
                        additional_file_paths = re.findall("\"([a-zA-Z0-9.\\\/]*)\"", line)
                        # We are only interested in adding files that are not contained within the same folder as our class files as these are
                        # added in `populate_snippets_from_folder`. Here we check for \\ and / characters in the file path and then reconstruct the path
                        # as required.
                        for additional_file_path in additional_file_paths:
                            if "\\" in additional_file_path:
                                additional_file_path_string = str(additional_file_path)
                                corrected_path = additional_file_path_string.replace("\\\\", "/")
                                additionalFiles.append("../../../" + corrected_path)
                            elif "/" in additional_file_path:
                                additionalFiles.append("../../../" + additional_file_path)
                        break

        additionalFiles.sort()
        for additionalFile in additionalFiles:
            self.source_files.append(additionalFile)

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
            content = "\n\n".join(body_parts)
            self.use_case = content
            return

        # how to use
        if "use" in heading_parts and "how" in heading_parts:
            content = "\n\n".join(body_parts)
            self.how_to_use = content
            return

        # how it works
        if "works" in heading_parts and "how" in heading_parts:
            step_strings = []
            lines = body_parts[0].split("\n")
            cleaned_lines = []
            for line in lines:
                if not line.strip().startswith("*"): # numbered steps
                    line_parts = line.split('.')
                    cleaned_lines.append(".".join(line_parts[1:]).strip())
                else: # sub-bullets
                    cleaned_line = line.strip().strip("*").strip()
                    cleaned_lines.append(f"***{cleaned_line}")
            self.how_it_works = cleaned_lines
            return
        
        # relevant API
        if "api" in heading_parts or "apis" in heading_parts:
            lines = body_parts[0].split("\n")
            cleaned_lines = []
            for line in lines:
                # removes nonsense formatting
                cleaned_line = line.strip("*").strip("-").split("-")[0].strip("`").strip().strip("`").replace("::", ".")
                cleaned_lines.append(cleaned_line)
            self.relevant_api = list(dict.fromkeys(cleaned_lines))
            self.relevant_api.sort()
            return

        # offline data
        if "offline" in heading_parts:
            content = "\n".join(body_parts)
            # extract any guids - these are AGOL items
            regex = re.compile('[0-9a-f]{8}[0-9a-f]{4}[1-5][0-9a-f]{3}[89ab][0-9a-f]{3}[0-9a-f]{12}', re.I)
            matches = re.findall(regex, content)

            self.offline_data = list(dict.fromkeys(matches))
            return

        # about the data
        if "data" in heading_parts and "about" in heading_parts:
            content = "\n\n".join(body_parts)
            self.data_statement = content
            return

        # additional info
        if "additional" in heading_parts:
            content = "\n\n".join(body_parts)
            self.Additional_info = content
            return

        # tags
        if "tags" in heading_parts:
            tags = body_parts[0].split(",")
            cleaned_tags = []
            for tag in tags:
                cleaned_tags.append(tag.strip())
            cleaned_tags.sort()
            self.keywords = cleaned_tags
            return