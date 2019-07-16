

def get_csproj_xml_for_nuget_packages(nuget_version_list):
    '''
    param nuget_version_list: a list of dictionaries.
    keys are 'name' and 'version'
    '''
    output_xml = ""
    for package in nuget_version_list.keys():
        output_xml += f'<PackageReference Include="{package}">\n\t<Version>{nuget_version_list[package]}</Version>\n</PackageReference>'
    return output_xml

def get_csproj_xml_for_code_files(snippets_list):
    '''
    snippets_list is a flat list of file names
    Doesn't process Android layouts
    '''

    xml_string = ""
    for file in snippets_list:
        # handle CS
        if file.endswith(".cs"):
            cs_file_include = f'<Compile Include="{file}" />\n'
            xml_string += cs_file_include
        # handle XAML
        elif file.endswith(".xaml"):
            xaml_file_include = f'<Page Include="{file}"><Generator>MSBuild:Compile</Generator></Page>\n'
            xml_string += xaml_file_include
    return xml_string

def get_csproj_xml_for_android_layout(snippets_list):
    xml_string = ""
    for file in snippets_list:
        # handle CS
        if file.endswith(".axml"):
            stripped_name = file.strip("../../../").replace("/", "\\")
            cs_file_include = f'<AndroidResource Include="{stripped_name}" />\n'
            xml_string += cs_file_include
    return xml_string