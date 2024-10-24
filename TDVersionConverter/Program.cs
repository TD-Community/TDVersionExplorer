using log4net.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TDVersionExplorer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);
            
            bool DebugArgs = false;
            TDConvert.UseLocalConverter = false;
            //TDFileBase.UseNamedPipes = false;

            if (DebugArgs)
            {                
                args = new string[18];
                args[0] = "-s";
                //args[1] = "K:\\Github\\TDVersionExplorer\\TDSampleFiles\\TD75_Text.apt";
                //args[1] = "K:\\Github\\TDVersionExplorer\\TDSampleFiles\\TD15_Compiled.app";
                args[1] = "K:\\Github\\TDVersionExplorer\\TDSampleFiles\\TD15_Normal.app";
                //args[1] = "K:\\GIT\\RIS641\\Sources\\qdocint_merged.apt";
                //args[1] = "K:\\Github\\TDVersionExplorer\\TDSampleFiles\\TD75_x86_Normal.app";
                args[2] = "-v";
                args[3] = $"{TDVersion.TD75}";
                args[4] = "-d";
                args[5] = "c:\\temp\\";
                args[6] = "-o";
                args[7] = "KEEP_ORIGINAL";
                args[8] = "-e";
                args[9] = "KEEP_ORIGINAL";
                args[10] = "-a";
                args[11] = "";
                args[12] = "-n";
                args[13] = "";
                args[14] = "-f";
                args[15] = "0";
                args[16] = "-m";
                args[17] = "3";
            }

            ConvertParameters convertParams = ProcessArguments(args, string.Empty);
            Logger.SetLogFile();
            Logger.SetLogLevel(convertParams.loglevel);
            GlobalContext.Properties["ServerName"] = $"{convertParams.DestVersion}";

            Logger.LogInfo($"TDVersionConverter.exe started");

            if (TDFileBase.UseNamedPipes)
                StartNamedPipeServer(convertParams);
            else
            {
                ConverterResult result = TDConvert.ExecuteConversion(convertParams);

                Environment.Exit((int)result);
            }
        }

        static ConvertParameters ProcessArguments(string[] args, string pipecommand)
        {
            if (string.IsNullOrEmpty(pipecommand))
            {
                var parsedArgs = ParseArgs(args);

                if (!parsedArgs.TryGetValue("-s", out string source))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);

                if (!(parsedArgs.TryGetValue("-v", out string tdversionstr)))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);
                if (!(Enum.TryParse<TDVersion>(tdversionstr, true, out TDVersion tdversion)))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);

                if (!parsedArgs.TryGetValue("-d", out string destinationfolder))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);

                if (!parsedArgs.TryGetValue("-o", out string formatstr))
                    formatstr = "KEEP_ORIGINAL";
                if (!(Enum.TryParse<TDOutlineFormat>(formatstr, true, out TDOutlineFormat outlineformat)))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);

                if (!parsedArgs.TryGetValue("-e", out string encodingstr))
                    encodingstr = "KEEP_ORIGINAL";
                if (!(Enum.TryParse<TDEncoding>(encodingstr, true, out TDEncoding encoding)))
                    Environment.Exit((int)ConverterResult.ERROR_INVALIDARG);

                parsedArgs.TryGetValue("-a", out string alternativestr);

                parsedArgs.TryGetValue("-n", out string originalstr);
                if (string.IsNullOrEmpty(originalstr))
                    originalstr = Path.GetFileName(source);

                parsedArgs.TryGetValue("-f", out string forceconversionstr);
                bool forceconversion = (forceconversionstr == "1");

                parsedArgs.TryGetValue("-r", out string renameextensionstr);
                bool renameExtension = (renameextensionstr == "1");

                if (!parsedArgs.TryGetValue("-m", out string debugmodestr))
                    debugmodestr = "NONE";
                if (!(Enum.TryParse<DebugMode>(debugmodestr, true, out DebugMode debugMode)))
                    debugMode = DebugMode.NONE;

                parsedArgs.TryGetValue("-l", out string loglevelstr);
                if (string.IsNullOrEmpty(loglevelstr))
                    loglevelstr = "OFF";


                return new ConvertParameters()
                {
                    source = source,
                    OriginalFileName = originalstr,
                    destinationfolder = destinationfolder.TrimEnd('\\'),
                    DestVersion = tdversion,
                    DestFormat = outlineformat,
                    DestEncoding = encoding,
                    alternativeFileName = alternativestr,
                    forceConversion = forceconversion,
                    renameExtension = renameExtension,
                    debugMode = debugMode,
                    loglevel = loglevelstr
                };
            }
            else
            {
                ConvertParameters convertParams = new ConvertParameters();
                convertParams.FromPipeMsg(pipecommand);
                if (string.IsNullOrEmpty(convertParams.OriginalFileName))
                    convertParams.OriginalFileName = Path.GetFileName(convertParams.source);
                return convertParams;
            }
        }

        static Dictionary<string, string> ParseArgs(string[] args)
        {
            var parsedArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-") && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    parsedArgs[args[i].ToLower()] = args[i + 1]; // Convert key to lowercase
                    i++; // Skip the next argument since it's the value
                }
                else if (args[i].StartsWith("-") && (i + 1 == args.Length || args[i + 1].StartsWith("-")))
                {
                    parsedArgs[args[i].ToLower()] = null; // Flag without a value
                }
            }

            return parsedArgs;
        }

        static void StartNamedPipeServer(ConvertParameters convertParams)
        {
            string pipename = convertParams.DestVersion.ToString();
            string previousloglevel = convertParams.loglevel;

            TDFileBase.ShowNamedPipeServers = (convertParams.debugMode & DebugMode.SHOW_SERVERS) == DebugMode.SHOW_SERVERS;

            if(TDFileBase.ShowNamedPipeServers)
            {
                IntPtr consoleHandle = GetConsoleWindow();

                if (consoleHandle != IntPtr.Zero)
                {
                    if (TDFileBase.ShowNamedPipeServers)
                    {
                        FreeConsole();
                        AllocConsole();  // Allocate a new console for this process
                    }
                }
            }
            
            Console.Title = $"TDVersionConverter - {pipename}";
            while (true)
            {
                NamedPipeServerStream pipeServer = new NamedPipeServerStream($"{pipename}", PipeDirection.InOut,10);
                TDConvert.MyNamedPipe = pipename;

                try
                {
                    Logger.LogInfo($"=================== Pipe {pipename}: WaitForConnection ===================");
                    Console.WriteLine($"=================== Pipe {pipename}: WaitForConnection ===================");
                    pipeServer.WaitForConnection();

                    // Send ACK
                    byte[] ack = Encoding.UTF8.GetBytes("ACK");
                    Logger.LogDebug($"Client connected. Write ack...");
                    Console.WriteLine("Client connected. Write ack...");
                    pipeServer.Write(ack, 0, ack.Length);

                    // Read client command
                    byte[] buffer = new byte[1024];
                    Console.WriteLine("Reading command from client...");
                    Logger.LogDebug($"Reading command from client...");
                    int bytesRead = pipeServer.Read(buffer, 0, buffer.Length);
                    string clientCommand = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Command received:\n{clientCommand}");
                    Logger.LogDebug($"Command received:\n{clientCommand}");

                    ConvertParameters convertParamsNew = new ConvertParameters();
                    convertParamsNew = ProcessArguments(new string[0], clientCommand);
                    if (previousloglevel != convertParamsNew.loglevel)
                    {
                        Logger.SetLogLevel(convertParamsNew.loglevel);
                        previousloglevel = convertParamsNew.loglevel;
                    }

                    // Execute command
                    Console.WriteLine($"Executing command...");
                    Logger.LogDebug($"Executing command...");
                    ConverterResult conversionresult = TDConvert.ExecuteConversion(convertParamsNew);
                    string result = $"{conversionresult}";

                    // Send result
                    byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                    Console.WriteLine($"Executed. Write back result...{result}");
                    Logger.LogInfo($"Executed. Result = {result}");
                    pipeServer.Write(resultBytes, 0, resultBytes.Length);

                    pipeServer.Disconnect();
                    pipeServer.Close();
                    pipeServer.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEx($"Error:", ex);
                }
            }
        }

        // Import the User32.dll function to show or hide windows
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Import the Kernel32.dll function to get the console window handle
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();
    }
}
