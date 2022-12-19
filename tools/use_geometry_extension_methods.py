import os

# Location of samples for each type of framework.
WinUI_path = "C:/arcgis-runtime-samples-dotnet/src/WinUI/ArcGIS.WinUI.Viewer/Samples"
WPF_path = "C:/arcgis-runtime-samples-dotnet/src/WPF/WPF.Viewer/Samples"
MAUI_path = "C:/arcgis-runtime-samples-dotnet/src/MAUI/Maui.Samples/Samples"

framework_paths = [WinUI_path, WPF_path, MAUI_path]
cs_files = []

# Navigate each sample folder to find all .cs files.
for framework_path in framework_paths:
    sample_groups = os.listdir(framework_path)
    for sample_group in sample_groups:
        sample_group_path = framework_path + '/' + sample_group
        samples = os.listdir(sample_group_path)
        for sample in samples:
            sample_path = sample_group_path + '/' + sample
            sample_files = os.listdir(sample_path)
            for path in sample_files:
                file_path = sample_path + '/' + path
                if path[-2] == 'c' and path[-1] == 's': 
                    cs_files.append(file_path)

methods = ["Area", "AreaGeodetic", "Boundary", "Buffer", "BufferGeodetic", "Clip", "CombineExtents", "Contains",
"ConvexHull", "Crosses", "Cut", "Densify", "DensifyGeodetic", "Difference", "Disjoint", "Distance","Generalize",
"Intersection", "Intersections", "Intersects", "IsSimple", "Length", "LengthGeodetic", "NearestCoordinate", "NearestCoordinateGeodetic",
"NearestVertex", "NormalizeCentralMeridian", "Offset", "Overlaps", "Project", "Relate", "RemoveM", "RemoveZ", "RemoveZAndM", "Simplify",
"SymmetricDifference", "Touches", "Union", "Within", "WithM", "WithZ", "WithZAndM", "CreatePointAlong", "Extend", "FractionAlong",
"Reshape", "LabelPoint", "Union"]

error_count = 0

# Read every line of every file. Write where extension methods are now being used.
for path in cs_files:
    lines = ""
    with open(path, 'r') as f:
        try:
            lines = f.readlines()
            print(path)
            f.close
            i = 0
            while i < len(lines):
                line = lines[i]
                if "GeometryEngine" in line and not "//" in line and not "instructions: " in line: 
                    method_found = False
                    for m in methods:
                        if m in line:
                            method_found = True
                            break
                    if method_found:
                        print(line)
                        split1 = line.split("GeometryEngine")
                        print(split1)
                        split2 = split1[1].split("(")
                        print(split2)
                        if "," in split1[1]:
                            split3 = split2[1].split(", ")
                            line = line.replace(split3[0] + ', ', "")
                            print(line)
                            line = line.replace("GeometryEngine.", split3[0] + '.')
                            print(line)
                            lines[i] = line 
                i += 1
        except:
            error_count += 1
    with open(path, "w") as f:
        try:
            i = 0
            while i < len(lines):
                if lines[i] != f.readline(i):
                    f.seek(i)
                    f.write(lines[i])
            f.close()
        except:
            error_count += 1

print(error_count)