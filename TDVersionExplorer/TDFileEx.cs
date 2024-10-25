﻿using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;

namespace TDVersionExplorer
{
    internal class TDFileEx:TDFileBase
    {
        public ConverterResult StartConvert(ConvertParameters convertParams)
        {
            TDFileBase.ShowNamedPipeServers = (convertParams.debugMode & DebugMode.SHOW_SERVERS) == DebugMode.SHOW_SERVERS;
            
            // When KEEPORIGINAL version, take the version of the original file
            if (convertParams.DestVersion == TDVersion.KEEP_ORIGINAL)
                convertParams.DestVersion = this.TDVersionInfo.NormalVersion;

            // No support to port to TD10 and TD11 as we do not have those CDK's
            if (convertParams.DestVersion < TDVersion.TD15)
            {
                converterResult = ConverterResult.ERROR_CDKNOTSUPPORTED;
                Logger.LogError($"Error StartConvert. CDK version not supported : {convertParams.DestVersion}");
                return converterResult;
            }

            Logger.LogInfo($"********* Start Conversion to {convertParams.DestVersion}: {convertParams.source} ********* ");

            string TDVersionStr = convertParams.DestVersion.ToString();

            // Combined TD versions (having _): take the later one (eg TD20_TD21)
            int lastIndex = TDVersionStr.LastIndexOf('_');

            if (lastIndex != -1)
                TDVersionStr = TDVersionStr.Substring(lastIndex + 1);

            if (this.TDVersionInfo.NormalVersion > TDVersion.TD11)
            {
                // First extract the source TD version
                if (!ExtractTDRuntime(this.TDVersionInfo.NormalVersion))
                {
                    converterResult = ConverterResult.ERROR_EXTRACTRUNTIME;
                    return converterResult;
                }
            }

            // Extract the destination TD version
            if (!ExtractTDRuntime(convertParams.DestVersion))
            {
                converterResult = ConverterResult.ERROR_EXTRACTRUNTIME;
                return converterResult;
            }

            if (this.TDOutLineFormat == TDOutlineFormat.NORMAL && (this.TDVersionInfo.NormalVersion == TDVersion.TD10 || this.TDVersionInfo.NormalVersion == TDVersion.TD11))
            {
                // Special case: TD10/TD11 in normal format must be converted by TD15 first (no older CDK's available).
                // Force TD15 to be extracted
                if (!ExtractTDRuntime(TDVersion.TD15))
                {
                    converterResult = ConverterResult.ERROR_EXTRACTRUNTIME;
                    return converterResult;
                }
            }

            converterResult = ExecuteConverterProcess(convertParams);

            Logger.LogInfo($"********* Conversion result: {converterResult} for {convertParams.source} *********\n");

            return converterResult;
        }

        private static bool ExtractTDRuntime(TDVersion version)
        {
            string TempRuntimeFolder = GetTempTDRuntimeFolder(version);
            string resourceFile = version.ToString() + ".zip";
            string resourceFullname = $"TDVersionExplorer.TDRuntime." + resourceFile;
            string FullPathZipFile = Path.Combine(TempRuntimeFolder, resourceFile);

            if (File.Exists(FullPathZipFile))
            {
                // ZIP is there, assume runtime is extracted. Maybe this needs revision as it should check the cdk dll is present
                Logger.LogDebug($"ExtractTDRuntime -> true. ZIP is already present : {FullPathZipFile}");
                return true;
            }

            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Get the list of embedded resources
            string[] resourceNames = assembly.GetManifestResourceNames();

            // Extract ZIP resource needed
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.StartsWith(resourceFullname, StringComparison.OrdinalIgnoreCase))
                {
                    return ExtractZIP(resourceName, resourceFile, TempRuntimeFolder);
                }
            }

            Logger.LogError($"Error ExtractTDRuntime: Resource not found : {resourceFullname}");
            return false;
        }

        private static bool ExtractZIP(string fullresourcename, string zipfile, string Tempfolder)
        {
            if (!Directory.Exists(Tempfolder))
            {
                try
                {
                    // If the folder does not exist, create it
                    Directory.CreateDirectory(Tempfolder);
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEx($"Error ExtractZIP: {zipfile}", ex);
                    return false;
                }
                
            }
            // Get the temporary directory to extract the ZIP
            string FullPathZipFile = Path.Combine(Tempfolder, zipfile);

            // Check if the ZIP is already extracted
            if (!File.Exists(FullPathZipFile))
            {
                Logger.LogDebug($"ExtractZIP. File not present in temp location. So copy/extract resource to : {FullPathZipFile}");
                try
                {
                    // Extract the embedded ZIP to the temp path
                    using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullresourcename))
                    {
                        if (resourceStream == null)
                        {
                            Logger.LogError($"Error ExtractZIP. Resource not found: {fullresourcename}");
                            return false;
                        }

                        using (var fileStream = new FileStream(FullPathZipFile, FileMode.Create, FileAccess.Write))
                        {
                            resourceStream.CopyTo(fileStream);
                            Logger.LogDebug($"ZipFile copied from resource: {fullresourcename} to {FullPathZipFile}");
                        }
                    }

                    ZipFile.ExtractToDirectory(FullPathZipFile, Tempfolder);
                    Logger.LogDebug($"ZipFile extracted: {FullPathZipFile}");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEx($"Error ExtractZIP: {zipfile}", ex);
                    return false;
                }
            }
            {
                Logger.LogDebug($"ZipFile already present. No need to extract: {FullPathZipFile}");
            }
            return true;
        }
    }

    internal class ConvertSettings
    {
        public TDVersion DestVersion;
        public TDOutlineFormat DestFormat;
        public TDEncoding DestEncoding;
        public bool forceConversion;
        public bool renameExtension;
        public string destinationfolder;
        public DebugMode debugMode;
        public string loglevel;
    }
}