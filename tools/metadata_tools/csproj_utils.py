'''
Functions for producing csproj-style XML for various kinds of content.
Used by generate_sample_solutions to update the .csproj files for individual projects.
'''

def get_csproj_xml_for_nuget_packages(nuget_version_list):
    '''
    param nuget_version_list: a list of dictionaries.
    keys are 'name' and 'version'
    '''
    output_xml = ""
    for package in nuget_version_list.keys():
        output_xml += f'<PackageReference Include="{package}">\n\t<Version>{nuget_version_list[package]}</Version>\n</PackageReference>'
    return output_xml

def get_csproj_xml_for_code_files(snippets_list, platform):
    '''
    snippets_list is a flat list of file names
    Doesn't process Android layouts
    '''

    xml_string = ""
    for file in snippets_list:
        # handle CS
        if file.startswith('../../../Controls'):
            stripped_name = file.strip("../../../").replace("/", "\\")
            cs_file_include = f'<Compile Include="{stripped_name}" />\n'
            xml_string += cs_file_include
        elif file.endswith(".cs"):
            cs_file_include = f'<Compile Include="{file}" />\n'
            xml_string += cs_file_include
        # handle XAML
        elif file.endswith(".xaml"):
            if platform == "UWP" or platform == "WPF":
                xml_string += f'<Page Include="{file}"><Generator>MSBuild:Compile</Generator></Page>\n'
            elif platform in ["XFA", "XFI", "XFU"]:
                xml_string += f'<EmbeddedResource Include="$(MSBuildThisFileDirectory){file}"><Generator>MSBuild:UpdateDesignTimeXaml</Generator></EmbeddedResource>\n'
    return xml_string

def get_csproj_xml_for_android_layout(snippets_list):
    xml_string = ""
    for file in snippets_list:
        # handle CS
        if file.endswith(".axml"):
            stripped_name = file.strip("../../../").replace("/", "\\")
            cs_file_include = f'<AndroidResource Include="{stripped_name}" />\n'
            xml_string += cs_file_include
        elif file.endswith(".xml"):
            stripped_name = file.strip("../../../").replace("/", "\\")
            cs_file_include = f'<AndroidResource Include="{stripped_name}" />\n'
            xml_string += cs_file_include
        elif "Attrs.xml" in file:
            stripped_name = file.strip("../../../").replace("/", "\\")
            cs_file_include = f'<AndroidResource Include="{stripped_name}"><Generator>MSBuild:UpdateGeneratedFiles</Generator></AndroidResource>\n'
            xml_string += cs_file_include
    return xml_string