from sample_metadata import *

import os

def main():
    # simple readme example
    path_to_sample_dir = "C:\\SamplesDotNET\\src\\WPF\\ArcGISRuntime.WPF.Viewer\\Samples\\Symbology\\FeatureLayerExtrusion"
    path_to_readme = os.path.join(path_to_sample_dir, "readme.md")
    path_to_json = os.path.join(path_to_sample_dir, "readme.metadata.json")

    print(f"Reading from {path_to_sample_dir}")

    # new readme example
    path_to_sample_dir = "C:\\SamplesDotNET\\src\\WPF\\ArcGISRuntime.WPF.Viewer\\Samples\\Network Analysis\\RouteAroundBarriers"
    path_to_readme = os.path.join(path_to_sample_dir, "readme.md")
    path_to_json = os.path.join(path_to_sample_dir, "readme.metadata.json")

    print(f"Reading from {path_to_sample_dir}")

    sample = sample_metadata()

    sample.populate_from_readme(path_to_readme)
    sample.flush_to_json(path_to_json)
    return

if __name__ == "__main__":
    main()