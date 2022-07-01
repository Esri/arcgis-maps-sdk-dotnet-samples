def safe_read_contents(path_to_file):
    '''
    Reads a file, returns contents as text.
    Handles annoying unicode situations.
    '''
    original_contents = ""
    try:
        with open(path_to_file, "r") as handle:
            original_contents = handle.read()
    except UnicodeDecodeError:
        try:
            with open(path_to_file, "r", encoding='utf-8') as handle:
                original_contents = handle.read()
        except UnicodeDecodeError:
            try:
                with open(path_to_file, "r", encoding='utf-16') as handle:
                    original_contents = handle.read()
            except:
                print(path_to_file)

    return original_contents

def safe_write_contents(path_to_file, new_content):
    '''
    Writes a string to a file, regardless of encoding
    '''
    try:
        with open(path_to_file, 'w+') as rewrite_handle:
            rewrite_handle.write(new_content)
    except UnicodeEncodeError:
        try:
            with open(path_to_file, 'w+', encoding="utf-8") as rewrite_handle:
                rewrite_handle.write(new_content)
        except UnicodeEncodeError:
            try:
                with open(path_to_file, 'w+', encoding="utf-16") as rewrite_handle:
                    rewrite_handle.write(new_content)
            except:
                print("Error writing file: "+path_to_file)