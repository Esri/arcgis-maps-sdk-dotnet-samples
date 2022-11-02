# Readme metadata tools

The following scripts are used to manage sample content:

* [sample_metadata.py](./sample_metadata.py) - Sample information model. Includes methods for reading a sample from metadata, rewriting metadata, and otherwise manipulating samples.
* [process_metadata.py](./process_metadata.py) - Tools for managing all metadata content. Features include the ability to read all samples and produce an updated TOC, the ability to read source readme content and update existing samples, and tools for keeping readmes and metadata.json files in sync.

## Requirements

* **requests** - available on [pip](https://pypi.org/project/requests/). This is used by [sample_metadata.py](./sample_metadata.py) to read information about offline data items from ArcGIS Online.
* **Python 3.7+** - tested on Python 3.7. The scripts make extensive use of newer Python features, like f strings.

## Running process_metadata.py

This script has three modes:

* **toc** - reads all sample metadata and produces an up-to-date TOC with sample name, link, and description.
* **improve** - attempts to take a readme from another source. If it is better than what is in the samples repo (has more content), its contents are inserted into the samples readme and metadata.
* **sync** - reads each readme and rewrites the associated json metadata as needed.

### toc

Usage: `python process_metadata.py toc {path_to_samples_repo}\src`

### improve

Usage: `python process_metadata.py improve {path_to_samples}\src {path_to_common}`

The common readme path should contain folders. The content will only be considered if the folder name matches the sample name _exactly_.

### sync

Usage: `python process_metadata.py sync {path_to_samples}\src`

This will read each sample's readme, populate the information model, then write out json.

Note: currently this implementation is naive; if there is something special about the existing json (maybe it uses a non-Runtime package), it will be indiscriminately overwritten.