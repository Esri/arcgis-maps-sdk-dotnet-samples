#!/usr/bin/env python3

import argparse
from pathlib import Path
import subprocess as sp

def main():
    msg = 'Entry point of the docker to run the sample-sync script.'
    parser = argparse.ArgumentParser(description=msg)
    parser.add_argument('-s', '--string', help='A JSON array of file paths.')
    args = parser.parse_args()
    
    print("** Starting sample sync **")
    code2 = sp.call(f'python3 /sample_sync.py',  shell=True)


if __name__ == '__main__':
    main()