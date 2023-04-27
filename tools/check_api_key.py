#!/usr/local/bin/python3

'''
Name:        check_api_key.py

Purpose:
This script parses files staged for commit to ensure
that they do not contain an API key.

Notes:

    .NET: Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "APIKeyString"

    Block Commit:
        * Anything that contains AAPK
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "APIKeyString" - API key is set by string
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = apiKey - API key is a variable with a value set elsewhere
            .. apiKey = "APIKeyString"
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = (1"asdf"df*) - API key is invalid, though may still contain sensitive information

    Allow Commit:
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = "" - empty string
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = apiKey
            .. apiKey = "" - empty string
            .. apiKey is not found
        * Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey not found
'''
import re
import sys
import argparse

#-------------------------------------------------------------------------------
# Global Variables
#-------------------------------------------------------------------------------

content = []

net_apiKey_argument_regex = r"Esri\.ArcGISRuntime\.ArcGISRuntimeEnvironment\.ApiKey[\s]*\=[\s]*([\sa-zA-Z0-9_\"\']*)"
# REGEX explanation: Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = {0+ spaces}{0+ alphanumeric characters}{0+ underscores}{0+ quotes}

valid_variable_regex = r"[_a-zA-Z][_a-zA-Z0-9]*[a-zA-Z0-9]"
# Starts with an alphabetical character or underscore, contains zero or more alphabetical characters or underscores then ends with an alphanumeric character

#-------------------------------------------------------------------------------
# Functions
#-------------------------------------------------------------------------------

def read_file(args):
    global content

    # Check if file was passed
    if not args.input:
        print(0)
        return

    source = args.input

    # try to open input file
    try:
        with open(source, 'r', encoding='utf-8') as file:
            content = file.readlines()
    except:
        # This file was most likely deleted.
        # Regardless, IO errors are not API keys and this should pass.
        print(0)
        return

    # for each line, parse line
    for i in range(len(content)):
        if "AAPK" in content[i]:
            print(i+1) # BLOCK anything with AAPK to be overly cautious
            return
        
        if "Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey =" in content[i]:
            ApiKey_argument = re.search(net_apiKey_argument_regex, content[i]).group(1)
            argument_value = check_argument(ApiKey_argument, i)+1
            if argument_value > 0:
                print(argument_value)
                return
            continue

    print(0) # ALLOW, API key not found anywhere
    return

#-------------------------------------------------------------------------------

def check_argument(ApiKey_argument: str, i: int) -> int: # returns 0 if ALLOW, else line_num if BLOCK.
    if not ApiKey_argument:
        return -1 # ALLOW, Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey is not set

    elif (ApiKey_argument[0] == '"' and ApiKey_argument[-1] == '"') or ApiKey_argument[0] == "'" and ApiKey_argument[-1] == "'":
        if len(ApiKey_argument) == 2 or ApiKey_argument[1:-1] == "YOUR_API_KEY":
            return -1 # ALLOW, API key is an empty string or Citra requested snippet
        return i # BLOCK, API key is a string

    if not re.match(valid_variable_regex, ApiKey_argument):
        return i # BLOCK, API key not a valid variable, though may still be sensitive information. For instance "-AAPK{...}"

    else:
        # The API key is set via a variable so we now need to find the value of that variable
        return find_value(ApiKey_argument)

    # We return i+1 to indicate the line number where the API key is defined, because line numbers are not zero indexed

#-------------------------------------------------------------------------------

def find_value(var) -> int:
    for i in range(len(content)):
        if re.search(var+r" *=", content[i]):
            try:
                if re.search(r" *= *"+var, content[i]):
                    continue
                pattern = r"= *([\"|\']?[\w]*[\"|\']?);?"
                # captures any quote surrounded string, after a "=" and zero or more spaces
                ApiKey_argument = re.search(pattern, content[i]).group(1)
            except:
                return -1

            return check_argument(ApiKey_argument, i)
            # We again check this value to see if is null, a string, or another variable

    return -1
    # The variable was not found or defined (using '=' at least), ALLOW the commit in this case

#-------------------------------------------------------------------------------
# main process
#-------------------------------------------------------------------------------
def parse_command_line():

    parser = argparse.ArgumentParser(description="Format include lines in source code.")
    parser.add_argument("input", default=None, help="File to parse")
    args = parser.parse_args()

    return args

#-------------------------------------------------------------------------------
def main_process():
    args = parse_command_line()
    read_file(args)

#-------------------------------------------------------------------------------
if __name__ == '__main__':
    main_process()