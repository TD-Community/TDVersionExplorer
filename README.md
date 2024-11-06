# TDVersionExplorer

**TDVersionExplorer** is a .NET application developed for analyzing and converting Gupta Team Developer (TD) files. It serves two main purposes: analyzing the contents of a folder for TD-related files and converting files between different TD versions.

## Key Features

- Analyze TD files (apt/app/apl/exe/dll/apd/qrp) to identify their version, file type, outline format, encoding, and bitness.
- Convert TD source files between different versions, including both upgrades to newer versions and backports to older ones.

---

## 1. Analyzing TD Files

TDVersionExplorer allows to analyze a folder's contents to quickly understand which Gupta Team Developer files it contains and retrieve key information about each file.
This is done by checking key parts of the file contents. Both text and binary files are inspected.
Having this feature, installations and folders containing sources can quickly be inspected to check TD specific files.

### Analysis Details

- **Supported File Types:** Includes TD sources, DLLs, EXEs, Dynalibs and reports (QRP).
- **Attributes Identified:** 
  - **TD Version:** Detects the TD version associated with each file, supporting versions from TD 1.0 through TD 7.x.
  - **Version Code:** Shows the exact version code, providing finer granularity within versions.
  - **File Type:** Indicates the file type.
  - **Outline Format:** Identifies the outline format of the file (TEXT, INDENTED TEXT, NORMAL or COMPILED).
  - **Bitness:** Specifies whether the (binary) file is in x86 or x64 architectures.
  - **Encoding:** Displays the text encoding of each source file (e.g., UTF8/ASCII or UTF16).

### Analysis Options

- **Folder Selection:** Choose the source folder for analysis.
- **Filter Options:** Filter results to show all files or specific types, such as TD DLL/EXE files, TD sources, Dynalibs, or reports.
- **Version Filtering:** Limit displayed results by specific TD versions.


## 2. Converting TD Files

TDVersionExplorer also supports converting source files between different TD versions. This includes the ability to upgrade source files to newer versions or backport them to older versions as needed.
Only files identified as TD sources with outline format TEXT, INDENTED TEXT and NORMAL (only x86) can be converted.

### Conversion key features

- **Source Conversion:** Converts only source files, not DLLs, EXEs, or other compiled binaries.
- **Bidirectional Support:** Allows conversions both from older to newer TD versions and from newer to older TD versions (backporting).
- **Outline Format Options:** Select the output format (e.g., NORMAL, TEXT or INDENTED TEXT) for the converted file.
- **Text Encoding Options:** Specify the text encoding format for the conversion (e.g., UTF8 or UTF16).
- **Rename File Extension:** Optionally rename the file extension based on common TD extension rules (APP vs APT).
- No need for any TD version to be installed
- Supports conversion between any TD versions (ansi and unicode)

### What is conversion?

Conversion is the process of changing the internal outline format such that it can be opened in the desired TD IDE version.
It does not mean that it automatically ports TD features or "rewrites" code to be compatible with the desired destination TD version.
In fact, TDVersionExplorer does the same developers do when wanting their sources to be opened in another TD version:

- For older TD versions to newer version: open the source in TD IDE or CDK and let the file be converted
- For newer TD versions to an older version: open the source in a text editor, change the outline version and encoding and then open the changed file in the desired TD IDE version

These manual actions can be cumbersome, specially when backporting. The TD IDE and CDK could show outline error messageboxes which have to be closed manually.
Even the automatic conversion from older to newer versions can be time consuming by opening each file manually up until all files are processed.

TDVersionExplorer automates these conversion scenarios.

### How does conversion work in TDVersionExplorer?

TDVersionExplorer uses the original Team Developer CDK's to open sources and save them back to disk.
You might have wondered why the main TDVersionExplorer executable is fairly big in size. The reason is that it contains all released TD version CDK's as resources.
So to convert TD sources, no TD installations have to be present on the system. It will use the provided internal TD version CDK runtime to perform the conversion.

Depending on the source TD version and destination version, TDVersionExplorer will automatically extract the needed CDK into the working (temp) folder and will dynamically call the CDK to open and save the source file.

For backporting, meaning conversion from new to old TD version (eg TD 7.5 to TD 4.2) the same process will be executed as developers do when backporting:

- Save the original source as text format
- Change the internal TD version number in the file
- Save the file in the needed encoding (UTF8/ANSI or UTF16)
- Open the file in the destination TD version and let it save to convert the outline internals and dismissing any popping messageboxes

As you probably know, backporting can be time consuming as the backported file when opened in the TD IDE may show multiple messageboxes indicating outline items are not allowed or unknown.
By clicking OK until all errors are dismissed the file is opened and can then be saved to the desired version format.

TDVersionExplorer will also automate this. So all messageboxes will be automatically closed when popping up. This will be much faster and less RSI inducing.

### Conversion options

These options are available:
- Force conversion:
  
When the source file is the same as the destionation file (eg TD version, outline format and encoding) by default the conversion will be skipped.
This option will force conversion even when the source and destination are the same.

- Full CDK errors:
  
During conversion, the CDK may display messageboxes showing outline errors. When you want to have all of the errors saved to .err file, enable this option.
This may slow down the conversion significantly. Keep this option disabled to minimize CDK errors for performance.

- Rename file extension

When converting a NORMAL (app) file to TEXT and visaversa, this option will automatically change the file extension to .apt or .app.
Libraries (.apl) will not be renamed.

- KEEP ORIGINAL options

For TD version, outline format and encoding the option KEEP ORIGINAL is available.
This will mean that the destination file will take the same attribute as the original. So for example, KEEP ORIGINAL for outline format will mean that if a source is saved in TEXT format, the destination file will also be in TEXT format.
This option gives the opportinity to convert multiple files from one TD version to another but keeing the original format or encoding.

### Limitations

Because TDVersionExplorer uses the original TD CDK's, it will encounter the same issues which might occur when converting manually.
Currupted sources may crash the conversion process. When the TD IDE is unable to load a source, TDVersionExplorer will not either.

To avoid TDVersionExplorer crashing, the actiual conversion using the CDK is done by a separate process (TDVersionConverter.exe).
Each TD version will have it's own instance of this process running. So when it crashes, the main application will not crash but restart the process when needed.
This separate process is also needed because TDVersionExplorer is unable to load multiple TD CDK's at once in the same process.

At this time only x86 sources can be converted. No support (yet) to convert between NORMAL x64 format and NORMAL x86 format.

Also sources saved as COMPILED can not be converted. First save COMPILED sources to TEXT format and then convert.

---

## Additional Information

- **Log Level Control:** Set the logging level to capture debug or standard info during analysis or conversion.
- **Open Log Feature:** Easily access logs for tracking actions and identifying potential issues.

---
