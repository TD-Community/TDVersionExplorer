![Language](https://img.shields.io/badge/Gupta_Team_Developer-SqlWindows_TD%201.0%20and%20up-red?style=plastic&labelColor=blue)

## TDVersionExplorer

TDVersionExplorer is a .NET (4.8) application developed to analyze and convert Gupta Team Developer (TD) files.  
Its main functions include analyzing folders for TD-related files and converting files between different TD versions.

### Key Features

- **Analyze TD Files**: Supports analysis of various TD file types (APT, APP, APL, EXE, DLL, APD, QRP), identifying attributes such as version, file type, outline format, encoding, and bitness.
- **Convert TD Source Files**: Allows upgrades to newer TD versions or backports to older versions, supporting seamless version transitions.


![Screenshot of the application](Images/TDVersionExplorer_Screenshot.png)

---

### 1. Analyzing TD Files

TDVersionExplorer provides a way to scan a folder’s contents to quickly identify Gupta Team Developer files and retrieve key information about each.  
This is achieved by inspecting specific parts of file contents, regardless of whether they are text or binary files.  
This feature helps inspect installations or folders containing source files to locate TD-specific files easily.

#### Analysis Details

- **Supported File Types**: TD sources, DLLs, EXEs, Dynalibs, and reports (QRP).
- **Attributes Identified**:
  - **TD Version**: Detects the TD version associated with each file, supporting versions from TD 1.0 through TD 7.x.
  - **Version Code**: Displays precise version codes for greater detail.
  - **File Type**: Identifies the file type.
  - **Outline Format**: Recognizes the outline format (TEXT, INDENTED TEXT, NORMAL, or COMPILED).
  - **Bitness**: Specifies whether the file (for binary files) is x86 or x64.
  - **Encoding**: Shows the encoding of text files (e.g., UTF8/ASCII or UTF16).

#### Analysis Options

- **Folder Selection**: Choose the folder to analyze.
- **Filter Options**: Filter results by file type, such as TD DLL/EXE files, TD sources, Dynalibs, or reports.
- **Version Filtering**: Restrict results to specific TD versions.

#### Example TD Files

TDVersionExplorer includes an extensive list of sample TD files from each TD version, from TD 1.0 up to TD 7.x.  
These are stored in the `TDSampleFiles` subfolder, with examples of source files in different formats, compiled executables, DLLs, Dynalibs, and QRP files for each version. 

Selecting the "TD Sample Files" checkbox in the main form automatically sets this folder for analysis, providing a quick way to examine TD file details and conduct tests.

---

### 2. Converting TD Files

TDVersionExplorer also supports converting source files between TD versions, whether upgrading to newer versions or backporting to older ones.  
Only TD source files with outline formats of TEXT, INDENTED TEXT, or NORMAL (x86 only) are eligible for conversion.

#### Conversion Key Features

- **Source Conversion Only**: Converts only source files, excluding DLLs, EXEs, and other binaries.
- **Bidirectional Conversion**: Supports both upgrades to newer versions and backports to older versions.
- **Outline Format Options**: Choose the output format for the converted file (NORMAL, TEXT, or INDENTED TEXT).
- **Text Encoding Options**: Specify encoding for the conversion, such as UTF8 or UTF16.
- **Rename File Extension**: Optionally change file extensions based on TD conventions (e.g., APP vs APT).
- **Independent of TD Installation**: Conversion does not require any TD version to be installed.
- **Supports All Versions**: Handles conversions between all TD versions, including both ANSI and Unicode.

---

### Understanding Conversion

Conversion is the process of adjusting the internal outline format to make a file compatible with the desired TD IDE version.  
It does not automatically adjust or rewrite code for compatibility with specific TD versions.  
TDVersionExplorer mimics the manual steps developers typically follow:

- **Upgrading to Newer Versions**: Open the source file in the TD IDE or CDK and allow it to be converted.
- **Backporting to Older Versions**: Open the source file in a text editor, adjust the outline version and encoding, and open the modified file in the desired TD IDE version.

These manual actions, especially backporting, can be tedious. The TD IDE and CDK often display error messages for outline issues, which must be dismissed manually.  
TDVersionExplorer automates these processes, making conversion faster and less prone to repetitive strain.

---

### How Conversion Works in TDVersionExplorer

TDVersionExplorer uses Team Developer CDK libraries to open and save source files.  
This explains why the main TDVersionExplorer executable is relatively large—it includes all necessary TD version CDKs as embedded resources.  
This design allows conversion without needing a local TD installation.

Depending on the source and destination versions, TDVersionExplorer automatically extracts the required CDK into a temporary folder and uses it to open and save files.

For backporting, TDVersionExplorer replicates the developer process by:
1. Saving the source file in text format.
2. Adjusting the internal TD version number.
3. Setting the correct encoding (UTF8/ANSI or UTF16).
4. Opening the file in the target TD version, letting it adjust the outline format, and automatically dismissing any message boxes.

Backporting can be time-consuming due to message boxes that appear when incompatible outline items are found. TDVersionExplorer automates these steps, closing message boxes automatically and significantly reducing manual effort.

---

### Conversion Options

- **Force Conversion**: Normally, conversion is skipped if the source and destination match in version, format, and encoding. This option forces conversion even if they are identical.
- **Full CDK Errors**: Enables saving of all CDK-generated errors to an `.err` file, though this may slow down conversion.
- **Rename File Extension**: Automatically renames file extensions based on conversion rules (e.g., converting from NORMAL to TEXT or vice versa changes `.app` to `.apt`). Libraries (`.apl`) are not renamed.
- **KEEP ORIGINAL Options**: Allows preserving original attributes for TD version, outline format, and encoding during conversion. This option is useful for bulk conversions while maintaining the original format or encoding.

---

### Limitations

As TDVersionExplorer relies on original TD CDKs, it faces the same limitations as manual conversions:
- **Corrupted Files**: Damaged files may crash the CDK during conversion. If the TD IDE cannot load a file, TDVersionExplorer won’t be able to either.
- **Separate Process for CDK Operations**: To avoid crashing the main application, each conversion is handled in a separate process (`TDVersionConverter.exe`). Each TD version has a dedicated process, allowing for restarts if necessary. This design is also needed because multiple CDK versions cannot load simultaneously in the same process.
- **Architecture and Format Restrictions**: Only x86 files can be converted; NORMAL x64 formats are not yet supported. COMPILED sources are also unsupported—save them as TEXT format before converting.

---

### Additional Information

- **Log Level Control**: Set the log level to capture either standard information or debug-level details during analysis and conversion.
- **Open Log Feature**: Easily access logs for reviewing actions and identifying any issues.
- **Working (temp) folder: the TD CDK libraries and temp files are stored in %temp%\TDVersionExplorer

### Possible issues

As TDVersionExplorer uses TD CDK libraries, which is a small subset of the TD runtime, it might occur that referenced dll's like vc runtime could be missing.  
For most CDK archives which are included as resources in the main executable the needed dependancies are included.  

When conversion fails due to errors ERROR_CDKLOAD or ERROR_CALLCDK, this is an indication the needed dependancies are not yet present.  
To get it working, copy the complete TD runtime installation to the subfolder in the temp folder mentioned above to see the conversion works. If so, please report this so the missing files can be added in the officisl archive.

## TD Community Forum
Join the TD Community Forum for everything related to Gupta Team Developer for questions, answers and info.

https://forum.tdcommunity.net

If you like this project and want to enhance/improve it please do so.
Any help is appreciated. Changes to this project can be done by pull request.
Like to be an official contributor, contact me to be added as contributor of this project.

Find me as Dave Rabelink on the forum mentioned above.
