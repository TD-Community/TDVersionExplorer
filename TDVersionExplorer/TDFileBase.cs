                                                                                                 using System;using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace TDVersionExplorer
{
    public enum ConverterResultCode
    {
        UNKNOWN = 0,
        CONVERTED = 1,
        ALREADYPORTED = 2,
        CONVERTED_WITH_ERRORS = 3,
        ERROR_ANALYZE = -1,
        ERROR_CREATEFOLDER = -2,
        ERROR_RUNTIMENOTFOUND = -3,
        ERROR_CDKSAVE = -4,
        ERROR_COPYINTERMEDIATE = -6,
        ERROR_INSERTVERSIONLINE = -7,
        ERROR_CDKNOTSUPPORTED = -8,
        ERROR_EXTRACTRUNTIME = -9,
        ERROR_CDKLOAD = -10,
        ERROR_CALLCDK = -11,
        ERROR_STARTCONVERTERPROC = -12,
        ERROR_INVALIDARG = -13,
        ERROR_NAMEDPIPE = -14,
        ERROR_UTFCONVERSION = -15
    }

    [Flags]
    public enum ConverterAttribs
    {
        NONE = 0,
        SHOW_SERVERS = 2,
        FORCE_CONVERSION = 4,
        RENAME_EXTENSION = 8,
        LOGLEVEL_DEBUG = 16,
        CDK_FULL_ERRORS = 32,
        ISBACKPORT = 64
    }

    public class ConverterParam
    {
        public string source = string.Empty;
        public string destinationfolder = string.Empty;
        public string alternativeFileName = string.Empty;
        public string OriginalFileName = string.Empty;
        public TDVersion DestVersion = new TDVersion();
        public TDOutlineFormat DestFormat = new TDOutlineFormat();
        public TDEncoding DestEncoding = new TDEncoding();
        public ConverterAttribs attributes = new ConverterAttribs();

        public string ToStr()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0,-16} {1}", "Source:", $"{source}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "Destfolder:", $"{destinationfolder}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "Orig file:", $"{OriginalFileName}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "DestVersion:", $"{DestVersion}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "DestFormat:", $"{DestFormat}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "DestEncoding:", $"{DestEncoding}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "Attributes:", $"{attributes}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "Alternative:", $"{alternativeFileName}"));
            return sb.ToString();
        }

        public string ToParamStrForProcess()
        {
            string parameters = " ";    // start with a space

            parameters += $" -s \"{source}\"";
            parameters += $" -n \"{OriginalFileName}\"";
            parameters += $" -v \"{DestVersion}\"";
            parameters += $" -o \"{DestFormat}\"";
            parameters += $" -e \"{DestEncoding}\"";
            parameters += $" -a \"{alternativeFileName}\"";
            parameters += $" -d \"{destinationfolder.TrimEnd('\\')}\"";
            parameters += $" -c \"{(int)attributes}\"";

            return parameters;
        }

        // Method to create a shallow copy
        public ConverterParam Clone()
        {
            return (ConverterParam)this.MemberwiseClone();
        }

        public string ToPipeMsg()
        {
            return $"{source}|{destinationfolder}|{alternativeFileName}|{OriginalFileName}|{DestVersion}|{DestFormat}|{DestEncoding}|{(int)attributes}";
        }

        public void FromPipeMsg(string serialized)
        {
            var parts = serialized.Split('|');
            source = parts[0];
            destinationfolder = parts[1];
            alternativeFileName = parts[2];
            OriginalFileName = parts[3];
            DestVersion = (TDVersion)Enum.Parse(typeof(TDVersion), parts[4]);
            DestFormat = (TDOutlineFormat)Enum.Parse(typeof(TDOutlineFormat), parts[5]);
            DestEncoding = (TDEncoding)Enum.Parse(typeof(TDEncoding), parts[6]);
            attributes = (ConverterAttribs)Enum.Parse(typeof(ConverterAttribs), parts[7]);
        }

        // 1) Set an attribute (add attribute to existing ones)
        public void SetAttribute(ConverterAttribs attribute)
        {
            attributes |= attribute;
        }

        // 2) Remove an attribute
        public void RemoveAttribute(ConverterAttribs attribute)
        {
            attributes &= ~attribute;
        }

        // 3) Clear all attributes (set to NONE)
        public void ClearAttributes()
        {
            attributes = ConverterAttribs.NONE;
        }

        // 4) Check if an attribute is set
        public bool IsAttributeSet(ConverterAttribs attribute)
        {
            return (attributes & attribute) != 0;
        }
    }

    public class ConverterResult
    {
        public ConverterResultCode resultCode = ConverterResultCode.UNKNOWN;
        public string msg = string.Empty;
        public string errFile = string.Empty;

        public string ToStr()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0,-16} {1}", "ResultCode:", $"{resultCode}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "Msg:", $"{msg}"));
            sb.AppendLine(string.Format("{0,-16} {1}", "ErrFile:", $"{errFile}"));

            return sb.ToString();
        }

        // Method to create a shallow copy
        public ConverterParam Clone()
        {
            return (ConverterParam)this.MemberwiseClone();
        }

        public string ToPipeMsg()
        {
            return $"{resultCode}|{msg}|{errFile}";
        }

        public void FromPipeMsg(string serialized)
        {
            var parts = serialized.Split('|');
            resultCode = (ConverterResultCode)Enum.Parse(typeof(ConverterResultCode), parts[0]);
            msg = parts[1];
            errFile = parts[2];
        }
    }

    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));
        private static readonly string logFile = Path.Combine(TDFileBase.TempfolderBase, "TDVersionExplorer.log");

        public static void LogInfo(string message)
        {
            log.Info(message);
        }

        public static void LogErrorEx(string message, Exception ex)
        {
            log.Error(message, ex);
        }

        public static void LogError(string message)
        {
            log.Error(message);
        }

        public static void LogDebug(string message)
        {
            log.Debug(message);
        }

        public static bool OpenLogFile()
        {
            if (File.Exists(logFile))
            {
                Process.Start(new ProcessStartInfo(logFile) { UseShellExecute = true });
                return true;
            }
            return false;
        }

        public static void SetLogFile()
        {
            string logDirectory = TDFileBase.TempfolderBase;

            // Ensure the directory exists
            if (!Directory.Exists(logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(logDirectory);
                }
                catch
                {
                    //
                }
            }

            // Retrieve the current repository (log4net configuration)
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());

            // Find the RollingFileAppender from the configured appenders
            var appenders = ((Hierarchy)logRepository).Root.Appenders;
            foreach (var appender in appenders)
            {
                if (appender is RollingFileAppender rollingFileAppender)
                {
                    // Set the new file path with the dynamic filename
                    rollingFileAppender.File = logFile;
                    rollingFileAppender.ActivateOptions();  // Apply changes
                }
            }
        }

        public static void DeleteLogFile()
        {
            if (File.Exists(logFile))
            {
                try
                {
                    File.Delete(logFile);
                }
                catch
                {
                    //
                }
            }
        }

        public static void SetLogLevel(string levelName)
        {
            // Get the hierarchy for the logger repository
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            // Set the level for the root logger to OFF
            hierarchy.Root.Level = hierarchy.LevelMap[levelName];

            // Apply the changes
            hierarchy.RaiseConfigurationChanged(EventArgs.Empty);
        }
    }

    public class TDFileBase
    {
        public static readonly string TempfolderBase = Path.GetTempPath() + @"TDVersionExplorer\";

        public TDVersionInfo TDVersionInfo = new TDVersionInfo();
        public TDFileType TDFileType = TDFileType.UNKNOWN;
        public TDOutlineFormat TDOutLineFormat = TDOutlineFormat.UNKNOWN;
        public TDEncoding TDEncoding = TDEncoding.UNKNOWN;
        public TDBitness TDBitness = TDBitness.UNKNOWN;
        public Encoding encoding;
        public string FileName = string.Empty;
        public string FileFullPath = string.Empty;
        public string TDOutlineVersionStr = string.Empty;

        public bool Initialised = false;
        public bool CanBeConverted = false;
        public ConverterResult converterResult = new ConverterResult { resultCode = ConverterResultCode.ERROR_UTFCONVERSION };

        public static bool UseNamedPipes = true;
        public static bool ShowNamedPipeServers = false;
        private static int PreviousShowNamedPipeServers = -1;

        public bool AnalyseFile(string file)
        {
            string sVersionLine = "";
            this.FileName = Path.GetFileName(file);
            this.FileFullPath = file;
            this.CanBeConverted = false;

            try
            {
                using (FileStream hFile = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[50];
                    int bytesRead = hFile.Read(buffer, 0, 50);
                    if (bytesRead == 50)
                    {
                        // First check if file is a QRP
                        if (buffer[6] == 0x47 && buffer[7] == 0x54 && buffer[8] == 0x49 && buffer[9] == 0x47 && buffer[10] == 0x52)
                        {
                            int QRPVersion = buffer[14];
                            this.TDVersionInfo = TDVersionRepository.GetByTDVersion((TDVersion)QRPVersion);
                            this.TDFileType = TDFileType.QRP;
                            this.Initialised = true;
                            this.CanBeConverted = false;
                            return true;
                        }
                        else if (StartsWith(buffer, "MGDR"))
                        {
                            this.TDOutLineFormat = TDOutlineFormat.NORMAL;
                            // Extract the version using a helper method (CStructGetWord equivalent)
                            int NormalVersion = BitConverter.ToUInt16(buffer, 4);
                            this.TDVersionInfo = TDVersionRepository.GetByTDVersion((TDVersion)NormalVersion);

                            // Extract the type (string) from the buffer at offset 12, length 5 (CStructGetString equivalent)
                            string sType = Encoding.ASCII.GetString(buffer, 12, 4);

                            BitnessByteValue bitnessValue;

                            if (sType == "RUNO")
                            {
                                this.TDFileType = TDFileType.DYNALIB;
                                this.TDOutLineFormat = TDOutlineFormat.UNKNOWN;

                                // Check the bitness based on the word at offset 8
                                bitnessValue = (BitnessByteValue)BitConverter.ToUInt16(buffer, 8);
                                switch (bitnessValue)
                                {
                                    case BitnessByteValue.APD_x86:
                                        this.TDBitness = TDBitness.x86;
                                        break;
                                    case BitnessByteValue.APD_x64:
                                        this.TDBitness = TDBitness.x64;
                                        break;
                                    default:
                                        this.TDBitness = TDBitness.UNKNOWN;
                                        break;
                                }
                                return true;
                            }
                            else
                                this.TDFileType = TDFileType.SOURCE;

                            // Check for other cases based on the word at offset 8
                            bitnessValue = (BitnessByteValue)BitConverter.ToUInt16(buffer, 8);
                            switch (bitnessValue)
                            {
                                case BitnessByteValue.SRC_x86_Normal:
                                    this.TDOutLineFormat = TDOutlineFormat.NORMAL;
                                    this.TDBitness = TDBitness.x86;
                                    this.CanBeConverted = true;
                                    break;
                                case BitnessByteValue.SRC_x86_Compiled:
                                    this.TDOutLineFormat = TDOutlineFormat.COMPILED;
                                    this.TDBitness = TDBitness.x86;
                                    break;
                                case BitnessByteValue.SRC_x64_Normal:
                                    this.TDOutLineFormat = TDOutlineFormat.NORMAL;
                                    this.TDBitness = TDBitness.x64;
                                    break;
                                case BitnessByteValue.SRC_x64_Compiled:
                                    this.TDOutLineFormat = TDOutlineFormat.COMPILED;
                                    this.TDBitness = TDBitness.x64;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (StartsWith(buffer, "MZ"))
                        {
                            bool found = CheckTDExecutable(file, out TDFileType filetype, out TDBitness bitnessTmp, out TDVersion tdversion);
                            if (found)
                            {
                                this.TDVersionInfo = TDVersionRepository.GetByTDVersion(tdversion);
                                this.TDBitness = bitnessTmp;
                                this.TDFileType = filetype;
                            }
                            return true;
                        }
                        else
                        {
                            // Check for BOM and adjust reading position based on the encoding
                            this.encoding = DetectEncoding(buffer, bytesRead);
                            // Convert the first few bytes (after BOM, if present) to a string
                            string text = "";

                            if (this.encoding == Encoding.UTF8 || this.encoding == Encoding.ASCII)
                            {
                                text = this.encoding.GetString(buffer, 0, bytesRead);
                                this.TDEncoding = TDEncoding.UTF8_ASCII;
                            }
                            else if (this.encoding == Encoding.Unicode || this.encoding == Encoding.BigEndianUnicode)  // UTF-16 LE or BE
                            {
                                text = this.encoding.GetString(buffer, 2, bytesRead - 2);  // Skip BOM (2 bytes for UTF-16)
                                this.TDEncoding = TDEncoding.UTF16;
                            }

                            if (text.StartsWith(".head 0 +"))
                            {
                                this.TDOutLineFormat = TDOutlineFormat.TEXT;
                                sVersionLine = ".head 1 -  Outline Version - ";
                            }
                            else if (text.StartsWith("Application Description:"))
                            {
                                this.TDOutLineFormat = TDOutlineFormat.TEXTINDENTED;
                                sVersionLine = "\tOutline Version - ";
                            }
                            else
                            {
                                this.TDEncoding = TDEncoding.UNKNOWN;
                                Initialised = true;
                                return true;
                            }

                            this.TDFileType = TDFileType.SOURCE;

                            using (FileStream fileLines = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                using (StreamReader reader = new StreamReader(fileLines, encoding))
                                {
                                    string sLine;
                                    while ((sLine = reader.ReadLine()) != null)
                                    {
                                        if (sLine.StartsWith(sVersionLine))
                                        {
                                            this.TDOutlineVersionStr = sLine.Replace(sVersionLine, "");
                                            this.TDVersionInfo = TDVersionRepository.GetByOutlineVersion(TDOutlineVersionStr);
                                            this.Initialised = true;
                                            this.CanBeConverted = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Initialised = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error AnalyseFile: {file}", ex);
                return false;
            }
        }

        public static string GetTempTDRuntimeFolder( TDVersion version)
        {
            string TDVersionStr = version.ToString();

            return PathAddBackslash( Path.Combine(TempfolderBase, TDVersionStr));
        }

        public static string PathAddBackslash( string path )
        {
            if (!path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                path += "\\";

            return path;
        }

        public static ConverterResult ExecuteConverterProcess(ConverterParam convertParams)
        {
            Logger.LogDebug($"Begin ExecuteConverterProcess...");
            string workerExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TDVersionConverter.exe");
            string parameters = convertParams.ToParamStrForProcess();
            string pipeName = convertParams.DestVersion.ToString();

            if (UseNamedPipes)
            {
                bool DoShow = convertParams.IsAttributeSet(ConverterAttribs.SHOW_SERVERS);

                if (PreviousShowNamedPipeServers == -1)
                    PreviousShowNamedPipeServers = DoShow ? 1 : 0;
                else
                {
                    if (PreviousShowNamedPipeServers != (DoShow ? 1 : 0))
                        TerminateAllProcesses();
                    PreviousShowNamedPipeServers = DoShow ? 1 : 0;
                }

                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
                if (!NamedPipeExists(pipeClient, pipeName, 50))
                {
                    if (jobHandle == IntPtr.Zero)
                        CreateJobObject();

                    // Define the process start info
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = workerExePath, // Use the full path to the worker executable
                        Arguments = parameters, // Add any arguments needed for the worker process
                        RedirectStandardOutput = false, // Redirect the standard output (optional)
                        RedirectStandardError = false,  // Redirect standard error (optional)
                        UseShellExecute = false, // Required for redirection
                        CreateNoWindow = !DoShow
                    };

                    Process process = new Process
                    {
                        StartInfo = startInfo
                    };

                    Logger.LogDebug($"Starting {convertParams.DestVersion}: {workerExePath}\n{parameters}");
                    try
                    {
                        process.Start();
                        Logger.LogDebug($"Process started: {workerExePath}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorEx($"Error starting Process {workerExePath}:",ex);
                        return new ConverterResult
                        {
                            resultCode = ConverterResultCode.ERROR_STARTCONVERTERPROC
                        };
                    }

                    if (jobHandle != null)
                    {
                        // Register process so when this process ends the started process is also killed
                        AssignProcessToJobObject(jobHandle, process.Handle);
                        Logger.LogDebug($"AssignProcessToJobObject - process added");
                    }
                        
                    bool NamedPipeFound = false;

                    for (int attempt = 1; attempt <= 3; attempt++)
                    {
                        NamedPipeFound = NamedPipeExists(pipeClient, pipeName, 1000);

                        if (NamedPipeFound)
                            break; 
                    }

                    if (!NamedPipeFound)
                    {
                        Logger.LogError($"NamedPipe {convertParams.DestVersion} not found");
                        return new ConverterResult
                        {
                            resultCode = ConverterResultCode.ERROR_STARTCONVERTERPROC
                        };
                    }
                    else
                        Logger.LogDebug($"NamedPipe {convertParams.DestVersion} found");
                }

                try
                {
                    byte[] buffer = new byte[1024];
                    // Send command to server
                    byte[] commandBytes = Encoding.UTF8.GetBytes(convertParams.ToPipeMsg());
                    Logger.LogDebug($"Write to named pipe {convertParams.DestVersion}: {convertParams.ToPipeMsg()}");
                    pipeClient.Write(commandBytes, 0, commandBytes.Length);

                    // Read result from server
                    int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                    string piperesult = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Logger.LogDebug($"Read from named pipe {convertParams.DestVersion}: {piperesult}");

                    pipeClient.Close();
                    pipeClient.Dispose();

                    ConverterResult result = new ConverterResult();
                    result.FromPipeMsg(piperesult);
                    return result;
                }

                catch (Exception ex)
                {
                    Logger.LogErrorEx($"Error ExecuteConverterProcess:", ex);
                    return new ConverterResult
                    {
                        resultCode = ConverterResultCode.ERROR_NAMEDPIPE
                    };
                }
            }
            else
            {
                if (jobHandle == IntPtr.Zero)
                    CreateJobObject();

                // Define the process start info
                var startInfo = new ProcessStartInfo
                {
                    FileName = workerExePath, // Use the full path to the worker executable
                    Arguments = parameters, // Add any arguments needed for the worker process
                    RedirectStandardOutput = false, // Redirect the standard output (optional)
                    RedirectStandardError = false,  // Redirect standard error (optional)
                    UseShellExecute = false, // Required for redirection
                    CreateNoWindow = !ShowNamedPipeServers // Do not create a console window
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                if (jobHandle != null)
                    AssignProcessToJobObject(jobHandle, process.Handle);

                process.WaitForExit();

                // Get the exit code
                int exitCode = process.ExitCode;

                return new ConverterResult
                {
                    resultCode = (ConverterResultCode)exitCode
                };

            }
        }

        // Terminate all processes in the job object
        private static void TerminateAllProcesses()
        {
            TerminateJobObject(jobHandle, 0);
            jobHandle = IntPtr.Zero;
            Logger.LogDebug($"TerminateAllProcesses executed");
        }

        public static string GetRunningExecutable()
        {
            // Get the entry (executable) assembly
            Assembly entryAssembly = Assembly.GetEntryAssembly();

            // Get just the name of the executable assembly
            string entryAssemblyName = entryAssembly.GetName().Name;

            return entryAssemblyName;
        }

        private static bool StartsWith(byte[] buffer, string prefix)
        {
            // Convert the prefix string to a byte array
            byte[] prefixBytes = Encoding.ASCII.GetBytes(prefix);

            // Check if the buffer has enough bytes
            if (buffer.Length < prefixBytes.Length)
                return false;

            // Compare the bytes in the buffer with the prefix
            for (int i = 0; i < prefixBytes.Length; i++)
            {
                if (buffer[i] != prefixBytes[i])
                    return false;
            }

            return true;
        }

        private static bool CheckTDExecutable(string filePath, out TDFileType fileType, out TDBitness bitness, out TDVersion tdversion)
        {
            fileType = TDFileType.UNKNOWN;
            bitness = TDBitness.UNKNOWN;
            tdversion = TDVersion.UNKNOWN;

            // Check if file exists
            if (!File.Exists(filePath))
                return false;

            // Get the file size
            long fileSize = new FileInfo(filePath).Length;
            if (fileSize < 64) // Less than 64 bytes, cannot be a valid executable
                return false;

            const int bufferSize = 65536; // 64 KB buffer for optimized reads
            byte[] buffer = new byte[bufferSize];

            try
            {
                using (var reader = new BinaryReader(File.OpenRead(filePath), System.Text.Encoding.ASCII, false))
                {
                    // Read the first 64KB or the file size, whichever is smaller
                    int totalRead = (int)Math.Min(bufferSize, fileSize);
                    reader.Read(buffer, 0, totalRead);

                    // Check for "MZ" header (DOS header)
                    if (buffer[0] != 'M' || buffer[1] != 'Z')
                        return false; // Not a valid executable file

                    // Get the PE header location from offset 0x3C
                    int peHeaderOffset = BitConverter.ToInt32(buffer, 0x3C);
                    if (peHeaderOffset < 0 || peHeaderOffset >= totalRead - 4)
                        return false; // PE header outside of the range we read

                    // Check for "PE\0\0" at the PE header offset
                    if (buffer[peHeaderOffset] != 'P' || buffer[peHeaderOffset + 1] != 'E' ||
                        buffer[peHeaderOffset + 2] != 0 || buffer[peHeaderOffset + 3] != 0)
                        return false; // Not a valid PE file

                    // Read Characteristics at offset 0x16 from the start of the PE header to determine EXE or DLL
                    ushort characteristics = BitConverter.ToUInt16(buffer, peHeaderOffset + 0x16);
                    fileType = (characteristics & 0x2000) != 0 ? TDFileType.DLL : TDFileType.EXE;

                    // Search for "MGDR" after PE header, starting at offset peHeaderOffset + 0xF8
                    int mgdrPosition = -1;
                    for (int i = peHeaderOffset + 0xF8; i <= totalRead - 4; i++)
                    {
                        if (buffer[i] == 'M' && buffer[i + 1] == 'G' && buffer[i + 2] == 'D' && buffer[i + 3] == 'R')
                        {
                            mgdrPosition = i;
                            break;
                        }
                    }

                    if (mgdrPosition == -1)
                        return false; // "MGDR" not found

                    // Read the two bytes after "MGDR" for TD version
                    int mgdrValue = BitConverter.ToUInt16(buffer, mgdrPosition + 4);
                    if (!Enum.IsDefined(typeof(TDVersion), mgdrValue))
                        return false; // Invalid version

                    tdversion = (TDVersion)mgdrValue;

                    // Read one byte 4 positions after "MGDR" for bitness
                    if (mgdrPosition + 8 < totalRead) // Ensure we don't exceed the buffer size
                    {
                        byte bitnessByte = buffer[mgdrPosition + 8];
                        if (bitnessByte == (byte)BitnessByteValue.EXE_x86) // Assuming 0 indicates x86
                            bitness = TDBitness.x86;
                        else if (bitnessByte == (byte)BitnessByteValue.EXE_x64) // Assuming 1 indicates x64
                            bitness = TDBitness.x64;
                    }
                    else
                        return false; // Not enough data to read the bitness

                    return true; // Successfully checked executable requirements
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error CheckTDExecutable:", ex);
                return false;
            }
        }

        private static Encoding DetectEncoding(byte[] buffer, int bytesRead)
        {
            // Check for BOM (Byte Order Mark)
            if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                // UTF-8 BOM
                return Encoding.UTF8;
            }
            if (bytesRead >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                // UTF-16 LE BOM
                return Encoding.Unicode;
            }
            if (bytesRead >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                // UTF-16 BE BOM
                return Encoding.BigEndianUnicode;
            }

            // Default to ASCII/ANSI if no BOM is found
            // For ANSI, ASCII is used to read as 1-byte characters
            return Encoding.ASCII;
        }

        private static bool NamedPipeExists(NamedPipeClientStream pipeClient, string pipeName, int millisecs)
        {
            try
            {
                pipeClient.Connect(millisecs);
                // Read ACK from server
                byte[] buffer = new byte[256];
                int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
                string ack = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (ack == "ACK")
                {
                    Logger.LogDebug($"NamedPipeExists {pipeName} -> true");
                    return true;
                }
                Logger.LogDebug($"NamedPipeExists {pipeName} -> false");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Warning NamedPipeExists {pipeName}:", ex);
                return false;
            }
        }

        private static IntPtr jobHandle;

        private static void CreateJobObject( )
        {
            // Create a JobObject that will manage worker processes
            jobHandle = CreateJobObject(IntPtr.Zero, null);
            ConfigureJobObject(jobHandle);
            Logger.LogDebug($"CreateJobObject JobHandle={jobHandle}");
        }

        private static void ConfigureJobObject(IntPtr job)
        {
            // Configure the JobObject to terminate all child processes when the parent dies
            JOBOBJECT_BASIC_LIMIT_INFORMATION limits = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = 0x2000 // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            };
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimits = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = limits
            };

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedLimits, extendedInfoPtr, false);

            // Set the job limits for the JobObject
            SetInformationJobObject(job, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length);

            // No need to free extendedInfoPtr here, we leave it until the application exits
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateJobObject(IntPtr hJob, uint uExitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public int LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public int ActiveProcessLimit;
            public long Affinity;
            public int PriorityClass;
            public int SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        private enum JobObjectInfoType
        {
            BasicLimitInformation = 2,
            ExtendedLimitInformation = 9,
        }
    }
}
