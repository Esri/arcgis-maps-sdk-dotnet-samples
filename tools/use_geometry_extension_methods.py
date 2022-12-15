import os

WinUI_path = "C:/arcgis-runtime-samples-dotnet/src/WinUI/ArcGIS.WinUI.Viewer/Samples"
WPF_path = "C:/arcgis-runtime-samples-dotnet/src/WPF/WPF.Viewer/Samples"
MAUI_path = "C:/arcgis-runtime-samples-dotnet/src/MAUI/Maui.Samples/Samples"
framework_paths = [WinUI_path, WPF_path, MAUI_path]

cs_files = []

for framework_path in framework_paths:
    sample_groups = os.listdir(framework_path)
    for sample_group in sample_groups:
        sample_group_path = framework_path + '/' + sample_group
        samples = os.listdir(sample_group_path)
        for sample in samples:
            sample_path = sample_group_path + '/' + sample
            sample_files = os.listdir(sample_path)
            for file in sample_files:
                file_path = sample_path + '/' + file
                if (file[-2] == 'c' and file[-1] == 's'): 
                    cs_files.append(file_path)
print(len(cs_files))