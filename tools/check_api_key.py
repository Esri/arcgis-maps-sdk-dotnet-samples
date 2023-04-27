#!/usr/local/bin/python3

'''
Name:        check_api_key.py

Purpose:
This script parses files staged for commit to ensure
that they do not contain an API key.

Notes:

    The Qt repo has a more thorough API key checking script that also looks for the API key setter. 
    We are not including this for now but it can be added in the future. 

    Allow Commit:
            "AAPK" is not found
'''

import sys
import argparse

#-------------------------------------------------------------------------------
# Global Variables
#-------------------------------------------------------------------------------

content = []

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

    print(0) # ALLOW, API key not found anywhere
    return

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