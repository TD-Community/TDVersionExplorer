﻿using log4net.Config;
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
        [DllImport("kernel32.dll")]
        private static extern uint SetErrorMode(uint uMode);

        private const uint SEM_NOGPFAULTERRORBOX = 0x0002;
        private const uint SEM_FAILCRITICALERRORS = 0x0001;

        static void Main(string[] args)
        {
            // Set the error mode to avoid Windows error dialogs
            SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);

            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);
            
            bool DebugArgs = false;
            TDConvert.UseLocalConverter = false;
            //TDFileBase.UseNamedPipes = false;

            if (DebugArgs)
            {                
                args = new string[16];
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
                args[14] = "-c";
                args[15] = "0";
            }

            if (args.Length == 0)
            {
                MessageBox.Show("This helper application can not be started standalone.\nStart TDVersionExplorer.exe instead.", "TDVersionConverter");
                Environment.Exit(0);
            }

            ConverterParam convertParams = ProcessArguments(args, string.Empty);

            Logger.SetLogFile();
            if (convertParams.IsAttributeSet(ConverterAttribs.LOGLEVEL_DEBUG))
                Logger.SetLogLevel("DEBUG");
            else
                Logger.SetLogLevel("INFO");
            GlobalContext.Properties["ServerName"] = $"{convertParams.DestVersion}";

            Logger.LogInfo($"TDVersionConverter.exe started");
            Logger.LogDebug($"TDVersionConverter.exe with params:\n\n{convertParams.ToStr(true)}");

            if (TDFileBase.UseNamedPipes)
                StartNamedPipeServer(convertParams);
            else
            {
                ConverterResult result = TDConvert.ExecuteConversion(convertParams);

                Environment.Exit((int)result.resultCode);
            }
        }

        static ConverterParam ProcessArguments(string[] args, string pipecommand)
        {
            if (string.IsNullOrEmpty(pipecommand))
            {
                var parsedArgs = ParseArgs(args);

                if (!(parsedArgs.TryGetValue("-d", out string tdversionstr)))
                    Environment.Exit((int)ConverterResultCode.ERROR_INVALIDARG);
                if (!(Enum.TryParse<TDVersion>(tdversionstr, true, out TDVersion tdversion)))
                    Environment.Exit((int)ConverterResultCode.ERROR_INVALIDARG);

                if (!parsedArgs.TryGetValue("-a", out string attributeStr))
                    Environment.Exit((int)ConverterResultCode.ERROR_INVALIDARG);
                if (!(Enum.TryParse<ConverterAttribs>(attributeStr, true, out ConverterAttribs attributes)))
                    Environment.Exit((int)ConverterResultCode.ERROR_INVALIDARG);

                return new ConverterParam()
                {
                    DestVersion = tdversion,
                    attributes = attributes
                };
            }
            else
            {
                ConverterParam convertParams = new ConverterParam();
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

        static void StartNamedPipeServer(ConverterParam convertParams)
        {
            string pipename = convertParams.DestVersion.ToString();
            bool logleveldebug = convertParams.IsAttributeSet(ConverterAttribs.LOGLEVEL_DEBUG);

            TDFileBase.ShowNamedPipeServers = convertParams.IsAttributeSet(ConverterAttribs.SHOW_SERVERS);

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

                    ConverterParam convertParamsNew = new ConverterParam();
                    convertParamsNew = ProcessArguments(new string[0], clientCommand);
                    if (logleveldebug != convertParamsNew.IsAttributeSet(ConverterAttribs.LOGLEVEL_DEBUG))
                    {
                        logleveldebug = convertParamsNew.IsAttributeSet(ConverterAttribs.LOGLEVEL_DEBUG);
                        if (logleveldebug)
                            Logger.SetLogLevel("DEBUG");
                        else
                            Logger.SetLogLevel("INFO");
                    }

                    // Execute command
                    Console.WriteLine($"Executing command...");
                    Logger.LogDebug($"Executing command...");
                    ConverterResult conversionresult = TDConvert.ExecuteConversion(convertParamsNew);
                    string result = conversionresult.ToPipeMsg();

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
