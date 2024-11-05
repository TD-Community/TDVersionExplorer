# TDVersionExplorer

**TDVersionExplorer** is a .NET application developed for analyzing and managing Gupta Team Developer (TD) files. It serves two main purposes: analyzing the contents of a folder for TD-related files and converting files between different TD versions.

## Key Features

- Analyze TD files to identify their version, format, encoding, and other attributes.
- Convert TD source files between different versions, including both upgrades to newer versions and backports to older ones.
- Filter and view specific types of TD files, such as DLLs, EXEs, and source files.
- Display detailed file attributes like TD Version, file type, outline format, encoding, and bitness.

---

## 1. Analyzing TD Files

TDVersionExplorer allows users to analyze a folder's contents to quickly understand which Gupta Team Developer files it contains and retrieve key information about each file.

### Analysis Details

- **Supported File Types:** Includes TD sources, DLLs, EXEs, Dynalibs and reports (QRP).
- **Attributes Identified:** 
  - **TD Version:** Detects the TD version associated with each file, supporting versions from TD 1.0 through TD 7.x.
  - **Version Code:** Shows the exact version code, providing finer granularity within versions.
  - **File Type:** Indicates the file type.
  - **Outline Format:** Identifies the outline format of the file (TEXT, INDENTED TEXT, NORMAL or COMPILED).
  - **Bitness:** Specifies whether the (binary) file is for x86 or x64 architectures.
  - **Encoding:** Displays the text encoding of each file (e.g., UTF8/ASCII or UTF16).

### Analysis Options

- **Folder Selection:** Choose the source folder for analysis.
- **Filter Options:** Filter results to show all files or specific types, such as TD DLL/EXE files, TD sources, Dynalibs, or reports.
- **Version Filtering:** Limit displayed results by specific TD versions.
- **Log Level:** Set the logging level for detailed debugging or general info.

## 2. Converting TD Files

TDVersionExplorer also supports converting source files between different TD versions. This includes the ability to upgrade source files to newer versions or backport them to older versions as needed.

### Conversion Details

- **Source Conversion:** Converts only source files, not DLLs, EXEs, or other compiled binaries.
- **Bidirectional Support:** Allows conversions both from older to newer TD versions and from newer to older TD versions (backporting).
- **Outline Format Options:** Select the output format (e.g., NORMAL, TEXT or INDENTED TEXT) for the converted file.
- **Text Encoding Options:** Specify the text encoding format for the conversion (e.g., UTF8 or UTF16).
- **Rename File Extension:** Optionally rename the file extension based on common TD extension rules (APP vs APT).


---

## Additional Information

- **Log Level Control:** Set the logging level to capture debug or standard info during analysis or conversion.
- **Open Log Feature:** Easily access logs for tracking actions and identifying potential issues.

---