using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TDVersionExplorer
{
    internal static partial class TDConvert
    {
        public static bool UseLocalConverter = true;      // When true, no new process is started. All debugging in the same source
        public static string MyNamedPipe = string.Empty;

        public static ConverterResult ExecuteConversion(ConvertParameters convertParams)
        {
            TDFileBase.ShowNamedPipeServers = (convertParams.debugMode & DebugMode.SHOW_SERVERS) == DebugMode.SHOW_SERVERS;

            ConverterResult result = Convert(convertParams);

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static ConverterResult Convert(ConvertParameters convertParams)
        {
            Logger.LogDebug($"Convert with params:\n\n{convertParams.ToStr()}");

            TDFileBase TDFile = new TDFileBase();

            if (!TDFile.AnalyseFile(convertParams.source))
                return ConverterResult.ERROR_ANALYZE;

            // Ok. Issue here. When source version is TD10 or TD11 and format is NORMAL we can not convert as we do not have those CDK's.
            // Solution is to take TD15 as source. So force that here

            if (TDFile.TDOutLineFormat == TDOutlineFormat.NORMAL && (TDFile.TDVersionInfo.NormalVersion == TDVersion.TD10 || TDFile.TDVersionInfo.NormalVersion == TDVersion.TD11))
            {
                TDFile.TDVersionInfo.NormalVersion = TDVersion.TD15;
                Logger.LogDebug($"Convert: TD10/TD11 sources in normal format will be converted by TD15 CDK");
            }

            // When KEEPORIGINAL version, take the version of the original file
            if (convertParams.DestVersion == TDVersion.KEEP_ORIGINAL)
            {
                convertParams.DestVersion = TDFile.TDVersionInfo.NormalVersion;
                Logger.LogDebug($"Convert: Destination version KEEP_ORIGINAL set to {convertParams.DestVersion}");
            }
                
            string TDVersionStr = convertParams.DestVersion.ToString();

            // Combined TD versions (having _): take the later one
            // Eg TD41_TD42 uses the same TD version codes. We can just take the most recent runtime then. So here that would be TD42
            int lastIndex = TDVersionStr.LastIndexOf('_');

            if (lastIndex != -1)
            {
                TDVersionStr = TDVersionStr.Substring(lastIndex + 1);
            }

            string DestinationFileName = convertParams.OriginalFileName;
            bool FileSaved = false;

            if (convertParams.renameExtension)
            {
                // Rename the extension based on the outline format. Libraries (apl) will not be renamed.
                switch (convertParams.DestFormat)
                {
                    case TDOutlineFormat.NORMAL:
                    case TDOutlineFormat.COMPILED:
                        if (!Path.GetExtension(DestinationFileName).Equals(".apl", StringComparison.OrdinalIgnoreCase))
                        {
                            DestinationFileName = Path.ChangeExtension(DestinationFileName, ".app");
                            Logger.LogDebug($"Convert: Rename extension is enabled. Set to .app");
                        }
                        break;
                    default:
                        if (!Path.GetExtension(DestinationFileName).Equals(".apl", StringComparison.OrdinalIgnoreCase))
                        {
                            DestinationFileName = Path.ChangeExtension(DestinationFileName, ".apt");
                            Logger.LogDebug($"Convert: Rename extension is enabled. Set to .apt");
                        }
                        break;
                }
            }
            
            try
            {
                if (!Directory.Exists(convertParams.destinationfolder))
                {
                    Logger.LogDebug($"Convert: Destination folder created: {convertParams.destinationfolder}");
                    Directory.CreateDirectory(convertParams.destinationfolder);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error Convert:", ex);
                return ConverterResult.ERROR_CREATEFOLDER;
            }

            // When KEEPORIGINAL format, take the format of the original file
            if (convertParams.DestFormat == TDOutlineFormat.KEEP_ORIGINAL)
            {
                convertParams.DestFormat = TDFile.TDOutLineFormat;
                Logger.LogDebug($"Convert: Destination format KEEP_ORIGINAL set to {convertParams.DestFormat}");
            }
                
            // When KEEPORIGINAL encoding, take the encoding of the original file
            if (convertParams.DestEncoding == TDEncoding.KEEP_ORIGINAL)
            {
                if (TDFile.TDEncoding == TDEncoding.UNKNOWN)
                {
                    // Probably NORMAL format. Assume the correct destination encoding
                    if (convertParams.DestVersion < TDVersion.TD51)
                    {
                        convertParams.DestEncoding = TDEncoding.UTF8_ASCII;
                        Logger.LogDebug($"Convert: Normal format. Assume destination encoding KEEP_ORIGINAL set to {convertParams.DestEncoding}. Destination version is < {TDVersion.TD51}");
                    }
                    else
                    {
                        convertParams.DestEncoding = TDEncoding.UTF16;
                        Logger.LogDebug($"Convert: Normal format. Assume destination encoding KEEP_ORIGINAL set to {convertParams.DestEncoding}. Destination version is > {TDVersion.TD41_TD42}");
                    }
                }
                else
                {
                    convertParams.DestEncoding = TDFile.TDEncoding;
                    Logger.LogDebug($"Convert: Destination encoding KEEP_ORIGINAL set to {convertParams.DestEncoding}");
                }
            }
                
            if (TDFile.TDOutLineFormat == TDOutlineFormat.TEXT || TDFile.TDOutLineFormat == TDOutlineFormat.TEXTINDENTED)
            {
                // Fix encoding based on destination TDVersion. Force the correct encoding
                if (convertParams.DestVersion < TDVersion.TD51)
                {
                    convertParams.DestEncoding = TDEncoding.UTF8_ASCII;
                    Logger.LogDebug($"Convert: Destination encoding forced to {convertParams.DestEncoding}. Destination version is < {TDVersion.TD51}");
                }
                else if (convertParams.DestVersion > TDVersion.TD41_TD42 && convertParams.DestVersion < TDVersion.TD72)
                {
                    convertParams.DestEncoding = TDEncoding.UTF16;
                    Logger.LogDebug($"Convert: Destination encoding forced to {convertParams.DestEncoding}. Destination version is > {TDVersion.TD41_TD42} and < {TDVersion.TD72}");
                }
            }

            if (TDFile.TDVersionInfo.NormalVersion == convertParams.DestVersion &&
                TDFile.TDOutLineFormat == convertParams.DestFormat &&
                TDFile.TDEncoding == convertParams.DestEncoding)
            {
                // Seems original file is already in the desired destination attributes
                if (!convertParams.forceConversion)
                {
                    Logger.LogDebug($"Convert: Source and destination attributes are the same. Do not convert");
                    return ConverterResult.ALREADYPORTED;
                }
                Logger.LogDebug($"Convert: Source and destination attributes are the same. Force is enabled so do convert");
            }

            if (!string.IsNullOrEmpty(convertParams.alternativeFileName))
            {
                DestinationFileName = convertParams.alternativeFileName;
                Logger.LogDebug($"Convert: Alternative filename is set. Use for destination: {DestinationFileName}");
            }

            if (TDFile.TDVersionInfo.NormalVersion != convertParams.DestVersion)
            {
                if (TDFile.TDOutLineFormat != TDOutlineFormat.NORMAL && (TDFile.TDVersionInfo.NormalVersion == TDVersion.TD10 || TDFile.TDVersionInfo.NormalVersion == TDVersion.TD11))
                {
                    // Ok. We have a very old TD version not having CDK. So try to convert immediately using new TD version
                    // Do nothing here. Just continue with CDK load & save
                    Logger.LogDebug($"Old TD version not having CDK. So try to convert immediately using destination TD version");
                }
                else if (convertParams.DestVersion < TDFile.TDVersionInfo.NormalVersion)
                {
                    // Backporting from new to older TD version
                    Logger.LogDebug($"Backporting detected: {convertParams.DestVersion} < {TDFile.TDVersionInfo.NormalVersion}");
                    return SaveToTextIntermediate(TDFile, convertParams, true);
                }
                else if ((!string.IsNullOrEmpty(convertParams.alternativeFileName) || TDFile.TDVersionInfo.NormalVersion != convertParams.DestVersion) && (TDFile.TDOutLineFormat == TDOutlineFormat.COMPILED || TDFile.TDOutLineFormat == TDOutlineFormat.NORMAL))
                {
                    Logger.LogDebug($"Alternative file or NORMAL format detected. Use intermediate step...");
                    // Normal/Compiled always first convert to TEXT and then to dest version
                    return SaveToTextIntermediate(TDFile, convertParams, false);
                }
            }
            else
                Logger.LogDebug($"Source and destination versions are the same: {convertParams.DestVersion}. Just start conversion.");

            string TDInstallation = TDFileBase.GetTempTDRuntimeFolder(convertParams.DestVersion);
            string cdk_dll = $"cdki{TDVersionStr.Substring(TDVersionStr.Length - 2)}.dll";

            if (!System.IO.File.Exists(TDInstallation + cdk_dll))
            {
                Logger.LogError($"Error Convert: TD runtime file not found {TDInstallation + cdk_dll}");
                return ConverterResult.ERROR_RUNTIMENOTFOUND;
            }

            EnvironmentVariableTarget scope = EnvironmentVariableTarget.Machine;
            string name = "PATH";
            string oldValue = string.Empty;

            // Now set the PATH variable. Put TD runtime location as first item in PATH
            try
            {
                oldValue = Environment.GetEnvironmentVariable(name, scope);
                Environment.SetEnvironmentVariable(name, $"{TDInstallation};{oldValue}");
                Logger.LogDebug($"Runtime folder added to PATH: {TDInstallation}");
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error Convert:", ex);
                // continue...
            }
            
            int optionFlagUTF = 0;
            bool HasRegValue = false;
            ushort houtline = 0;
            bool ok = false;

            if (convertParams.DestVersion > TDVersion.TD41_TD42)
            {
                Logger.LogDebug($"Using CDK UNICODE. Destination version is {convertParams.DestVersion}");
                try
                {
                    if (!TryGetFunctionDelegate(cdk_dll, "CDKLoadApp", ref CDKLoadApp))
                        return ConverterResult.ERROR_CDKLOAD;

                    if (!TryGetFunctionDelegate(cdk_dll, "CDKOutlineSave", ref CDKOutlineSave))
                        return ConverterResult.ERROR_CDKLOAD;

                    if (convertParams.DestVersion > TDVersion.TD74)
                    {
                        if (!TryGetFunctionDelegate(cdk_dll, "CDKOutlineSaveAsText", ref CDKOutlineSaveAsTextUTF))
                            return ConverterResult.ERROR_CDKLOAD;
                    }
                    else
                    {
                        if (!TryGetFunctionDelegate(cdk_dll, "CDKOutlineSaveAsText", ref CDKOutlineSaveAsText))
                            return ConverterResult.ERROR_CDKLOAD;

                        if (convertParams.DestVersion > TDVersion.TD72 && convertParams.DestVersion < TDVersion.TD75)
                        {
                            if (!TryGetFunctionDelegate(cdk_dll, "CDKSetUTF8Option", ref CDKSetUTF8Option))
                                return ConverterResult.ERROR_CDKLOAD;
                        }
                    }

                    if (!TryGetFunctionDelegate(cdk_dll, "CDKReleaseApp", ref CDKReleaseApp))
                        return ConverterResult.ERROR_CDKLOAD;

                    // TD72 up to TD74 could use registry to determine how to save sources (UTF8 or UTF16)
                    if (convertParams.DestVersion > TDVersion.TD71 && convertParams.DestVersion < TDVersion.TD75 && convertParams.DestFormat != TDOutlineFormat.NORMAL)
                    {
                        HasRegValue = RegistryGetUTFOption(convertParams.DestVersion, out optionFlagUTF);

                        if (convertParams.DestEncoding == TDEncoding.UTF8_ASCII)
                            RegistrySetUTFOption(convertParams.DestVersion, 3);
                        else
                            RegistrySetUTFOption(convertParams.DestVersion, 2);
                    }

                    if ((convertParams.DestVersion > TDVersion.TD72 && convertParams.DestVersion < TDVersion.TD75) && (convertParams.DestFormat == TDOutlineFormat.TEXT || convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED))
                    {
                        // LoadOrSave:
                        //0->Enable or disable Save As UTF8 option
                        //1->Enable or disable Read As UTF8 option
                        //2->Enable or disable both options

                        ok = CDKSetUTF8Option(1, true);
                        Logger.LogDebug($"CDKSetUTF8Option ReadUTF8 enabled -> return = {ok}");

                        if (convertParams.DestEncoding == TDEncoding.UTF16)
                        {
                            ok = CDKSetUTF8Option(0, false);
                            Logger.LogDebug($"CDKSetUTF8Option SaveUTF16 > return = {ok}");
                        }
                        else
                        {
                            ok = CDKSetUTF8Option(0, true);
                            Logger.LogDebug($"CDKSetUTF8Option SaveUTF8 -> return = {ok}");
                        }
                    }

                    // Start the thread that closes message boxes
                    MessageBoxCloser.Start();
                    houtline = CDKLoadApp(TDFile.FileFullPath);
                    Logger.LogDebug($"CDKLoadApp({TDFile.FileFullPath}) -> hOutline = {houtline}");
                    MessageBoxCloser.Stop();

                    if (houtline > 0)
                    {
                        int flags = 0;

                        // TD75 and up has now flags as parameter 
                        if ((convertParams.DestVersion > TDVersion.TD74) && (convertParams.DestFormat == TDOutlineFormat.TEXT || convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED))
                        {
                            if (convertParams.DestEncoding == TDEncoding.UTF16 && convertParams.DestFormat == TDOutlineFormat.TEXT)
                                flags = 0;
                            else if (convertParams.DestEncoding == TDEncoding.UTF16 && convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED)
                                flags = 1;
                            else if (convertParams.DestEncoding == TDEncoding.UTF8_ASCII && convertParams.DestFormat == TDOutlineFormat.TEXT)
                                flags = 2;
                            else if (convertParams.DestEncoding == TDEncoding.UTF8_ASCII && convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED)
                                flags = 3;
                            Logger.LogDebug($"Use flags={flags} for outline format {convertParams.DestFormat} and encoding {convertParams.DestEncoding}");
                        }

                        if (convertParams.DestFormat == TDOutlineFormat.TEXT)
                        {
                            if (convertParams.DestVersion > TDVersion.TD74)
                                FileSaved = CDKOutlineSaveAsTextUTF(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), flags);
                            else
                                FileSaved = CDKOutlineSaveAsText(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), false);
                            Logger.LogDebug($"CDKOutlineSaveAsText TEXT ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }
                        else if (convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED)
                        {
                            if (convertParams.DestVersion > TDVersion.TD74)
                                FileSaved = CDKOutlineSaveAsTextUTF(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), flags);
                            else
                                FileSaved = CDKOutlineSaveAsText(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), true);
                            Logger.LogDebug($"CDKOutlineSaveAsText TEXTINDENTED ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }
                        else if (convertParams.DestFormat == TDOutlineFormat.NORMAL)
                        {
                            FileSaved = CDKOutlineSave(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName));
                            Logger.LogDebug($"CDKOutlineSave NORMAL ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }
                            
                        ok = CDKReleaseApp(houtline);
                        Logger.LogDebug($"CDKReleaseApp -> return = {ok}");

                        if (FileSaved && (convertParams.DestVersion > TDVersion.TD71 && convertParams.DestVersion < TDVersion.TD74) && (convertParams.DestFormat == TDOutlineFormat.TEXT || convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED))
                        {
                            if (!ConvertFileEncodingInPlace(Path.Combine(convertParams.destinationfolder, DestinationFileName), convertParams.DestEncoding))
                                return ConverterResult.ERROR_UTFCONVERSION;
                        }
                    }
                }
                catch (AccessViolationException ex)
                {
                    MessageBoxCloser.Stop();
                    Logger.LogErrorEx($"Error Convert:", ex);
                    return ConverterResult.ERROR_CALLCDK;
                }
                catch (Exception ex)
                {
                    MessageBoxCloser.Stop();
                    Logger.LogErrorEx($"Error Convert:", ex);
                    return ConverterResult.ERROR_CALLCDK;
                }

                if (convertParams.DestVersion > TDVersion.TD71 && convertParams.DestVersion < TDVersion.TD75)
                {
                    if (HasRegValue)
                        RegistrySetUTFOption(convertParams.DestVersion, optionFlagUTF);
                }
            }
            else
            {
                Logger.LogDebug($"Using CDK ASCII. Destination version is {convertParams.DestVersion}");
                try
                {
                    if (!TryGetFunctionDelegate(cdk_dll, "CDKLoadApp", ref CDKLoadAppASCII))
                        return ConverterResult.ERROR_CDKLOAD;

                    if (!TryGetFunctionDelegate(cdk_dll, "CDKOutlineSave", ref CDKOutlineSaveASCII))
                        return ConverterResult.ERROR_CDKLOAD;

                    if (!TryGetFunctionDelegate(cdk_dll, "CDKOutlineSaveAsText", ref CDKOutlineSaveAsTextASCII))
                        return ConverterResult.ERROR_CDKLOAD;

                    if (!TryGetFunctionDelegate(cdk_dll, "CDKReleaseApp", ref CDKReleaseApp))
                        return ConverterResult.ERROR_CDKLOAD;

                    // Start the thread that closes message boxes
                    MessageBoxCloser.Start();
                    houtline = CDKLoadAppASCII(TDFile.FileFullPath);
                    Logger.LogDebug($"CDKLoadApp({TDFile.FileFullPath}) -> hOutline = {houtline}");
                    MessageBoxCloser.Stop();

                    if (houtline > 0)
                    {
                        if (convertParams.DestFormat == TDOutlineFormat.TEXT)
                        {
                            FileSaved = CDKOutlineSaveAsTextASCII(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), false);
                            Logger.LogDebug($"CDKOutlineSaveAsText TEXT ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }
                        else if (convertParams.DestFormat == TDOutlineFormat.TEXTINDENTED)
                        {
                            FileSaved = CDKOutlineSaveAsTextASCII(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName), true);
                            Logger.LogDebug($"CDKOutlineSaveAsText TEXTINDENTED ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }
                        else if (convertParams.DestFormat == TDOutlineFormat.NORMAL)
                        {
                            FileSaved = CDKOutlineSaveASCII(houtline, Path.Combine(convertParams.destinationfolder, DestinationFileName));
                            Logger.LogDebug($"CDKOutlineSave NORMAL ({Path.Combine(convertParams.destinationfolder, DestinationFileName)}) -> return = {FileSaved}");
                        }

                        ok = CDKReleaseApp(houtline);
                        Logger.LogDebug($"CDKReleaseApp-> return = {ok}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxCloser.Stop();
                    Logger.LogErrorEx($"Error Convert:", ex);
                    return ConverterResult.ERROR_CALLCDK;
                }
            }

            try
            {
                Environment.SetEnvironmentVariable(name, oldValue);
                Logger.LogDebug($"PATH environment variable set back to original");
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error Convert:", ex);
                // continue...
            }
            
            if (!(houtline > 0))
            {
                Logger.LogError($"TD outline not loaded");
                return ConverterResult.ERROR_CDKLOAD;
            }

            if (FileSaved)
            {
                if (MessageBoxCloser.messageBoxTexts.Count > 0)
                {
                    string errFile = Path.Combine(convertParams.destinationfolder, DestinationFileName + ".err");
                    try
                    {
                        File.WriteAllLines(errFile, MessageBoxCloser.messageBoxTexts);
                        Logger.LogDebug($"Outline errors saved to : {errFile}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorEx($"Error saving .err file: {errFile}", ex);
                    }
                    return ConverterResult.CONVERTED_WITH_ERRORS;
                }
                return ConverterResult.CONVERTED;
            }
            else
                return ConverterResult.ERROR_CDKSAVE;
        }

        private static ConverterResult SaveToTextIntermediate(TDFileBase TDFile, ConvertParameters convertParams, bool backport)
        {
            // The purpose of this method is to convert using multiple steps:
            // 1) Copy or convert source file to intermediate file (TEXT format) in temp folder
            // 2) When backporting, change the outline version to the lowest one
            // 3) Start the real conversion from intermediate to the requested desination folder

            string InterFileName = "Intermediate.apt";
            string InterFileNameFullPath = TDFileBase.TempfolderBase + InterFileName;
            string InterFileNameLVFullPath = TDFileBase.TempfolderBase + "IntermediateLowerVersion.apt";
            bool LowerVersionInserted = false;

            Encoding IntermediateEncoding = Encoding.Unicode;
            Encoding DestEncoding;

            ConvertParameters convertParamsNew;

            if (TDFile.TDOutLineFormat == TDOutlineFormat.TEXT || TDFile.TDOutLineFormat == TDOutlineFormat.TEXTINDENTED)
            {
                // The source is in TEXT/INDENTEDTEXT format. So we can just copy the file
                try
                {
                    File.Copy(TDFile.FileFullPath, InterFileNameFullPath, overwrite: true);

                    // Get the current attributes
                    FileAttributes attributes = File.GetAttributes(InterFileNameFullPath);

                    // Remove the ReadOnly flag, if present
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        attributes &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(InterFileNameFullPath, attributes);
                    }
                    IntermediateEncoding = TDFile.encoding;
                    Logger.LogDebug($"Copied file to intermediate: {TDFile.FileFullPath} to {InterFileNameFullPath}");
                }
                catch (IOException ioEx)
                {
                    Logger.LogErrorEx($"Error SaveToTextIntermediate:", ioEx);
                    return ConverterResult.ERROR_COPYINTERMEDIATE;
                }
            }
            else if (TDFile.TDOutLineFormat == TDOutlineFormat.NORMAL || TDFile.TDOutLineFormat == TDOutlineFormat.COMPILED)
            {
                // File must first be converted from NORMAL to TEXT format before we can convert it to the destination format
                Logger.LogDebug($"File has NORMAL outline format. Start converting to TEXT intermediate.");
                convertParamsNew = new ConvertParameters()
                {
                    source = convertParams.source,
                    OriginalFileName = convertParams.OriginalFileName,
                    destinationfolder = TDFileBase.TempfolderBase.TrimEnd('\\'),
                    alternativeFileName = InterFileName,
                    DestVersion = TDFile.TDVersionInfo.NormalVersion,
                    DestFormat = TDOutlineFormat.TEXT,
                    DestEncoding = convertParams.DestEncoding,
                    forceConversion = convertParams.forceConversion,
                    renameExtension = convertParams.renameExtension,
                    debugMode = convertParams.debugMode,
                    loglevel = convertParams.loglevel
                };

                // Ok. Issue here. When dest version is TD10 or TD11 we can not convert as we do not have those CDK's.
                // Solution is to take TD15 as source and destination. So force that here

                if (convertParamsNew.DestVersion == TDVersion.TD10 || convertParamsNew.DestVersion == TDVersion.TD11)
                    convertParamsNew.DestVersion = TDVersion.TD15;

                if (MyNamedPipe == convertParamsNew.DestVersion.ToString() || UseLocalConverter)
                {
                    // We do not need to start another converter process. This one is already the correct converter
                    ConverterResult result = ExecuteConversion(convertParamsNew);
                    if (result < ConverterResult.CONVERTED)
                        return result;
                }
                else
                {
                    // Do the actual conversion by the dedicated converter, not his one
                    ConverterResult result = TDFileBase.ExecuteConverterProcess(convertParamsNew);
                    if (result < ConverterResult.CONVERTED)
                        return result;
                }

                TDFileBase TDFileTmp = new TDFileBase();
                if (!TDFileTmp.AnalyseFile(InterFileNameFullPath))
                    return ConverterResult.ERROR_ANALYZE;

                IntermediateEncoding = TDFileTmp.encoding;
            }

            if (backport)
            {
                Logger.LogDebug($"Backporting. Reading source and find TD version line to replace: {InterFileNameFullPath}");
                if (convertParams.DestEncoding == TDEncoding.UTF16)
                    DestEncoding = Encoding.Unicode;
                else
                    DestEncoding = Encoding.ASCII;
                Logger.LogDebug($"Backporting. Use these encodings for versionline replacement: {IntermediateEncoding} to {DestEncoding}");
                try
                {
                    string versionlineOldT = ".head 1 -  Outline Version - " + TDFile.TDVersionInfo.OutlineVersion;
                    string versionlineNewT = ".head 1 -  Outline Version - 4.0.26";
                    string versionlineOldTI = "\tOutline Version - " + TDFile.TDVersionInfo.OutlineVersion;
                    string versionlineNewTI = "\tOutline Version - 4.0.26";

                    // Open the ASCII file for reading
                    using (StreamReader reader = new StreamReader(InterFileNameFullPath, IntermediateEncoding))
                    {
                        // Open the destination file for writing with UTF-16 encoding
                        using (StreamWriter writer = new StreamWriter(InterFileNameLVFullPath, false, DestEncoding))
                        {
                            string line;

                            // Read and write each line one by one
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line == versionlineOldT)
                                {
                                    writer.WriteLine(versionlineNewT);
                                    LowerVersionInserted = true;
                                    Logger.LogDebug($"Found: {versionlineOldT}. Replaced with: {versionlineNewT}");
                                }
                                else if (line == versionlineOldTI)
                                {
                                    writer.WriteLine(versionlineNewTI);
                                    LowerVersionInserted = true;
                                    Logger.LogDebug($"Found: {versionlineOldTI}. Replaced with: {versionlineNewTI}");
                                }
                                else
                                    writer.WriteLine(line);
                            }
                            Logger.LogDebug($"Intermediate file saved as {InterFileNameLVFullPath}");
                        }
                    }
                }
                catch (IOException ioEx)
                {
                    Logger.LogErrorEx($"Error SaveToTextIntermediate:", ioEx);
                    return ConverterResult.ERROR_INSERTVERSIONLINE;
                }

                if (!LowerVersionInserted)
                {
                    Logger.LogError($"Error SaveToTextIntermediate: versionline not inserted");
                    return ConverterResult.ERROR_INSERTVERSIONLINE;
                }
            }
            else
                InterFileNameLVFullPath = InterFileNameFullPath;

            convertParamsNew = convertParams.Clone();
            convertParamsNew.source = InterFileNameLVFullPath;
            convertParamsNew.alternativeFileName = string.Empty;

            if (MyNamedPipe == convertParamsNew.DestVersion.ToString() || UseLocalConverter)
                return ExecuteConversion(convertParamsNew);
            else
                return TDFileBase.ExecuteConverterProcess(convertParamsNew);
        }

        static bool ConvertFileEncodingInPlace(string filePath, TDEncoding TDDestEncoding)
        {
            Encoding sourceEncoding;
            Encoding targetEncoding;
            TDFileBase TDFile = new TDFileBase();

            if (!TDFile.AnalyseFile(filePath))
                return false;

            if (TDFile.TDEncoding == TDEncoding.UTF16)
                sourceEncoding = Encoding.Unicode;
            else
                sourceEncoding = Encoding.ASCII;

            if (TDDestEncoding == TDEncoding.UTF16)
                targetEncoding = Encoding.Unicode;
            else
                targetEncoding = Encoding.ASCII;

            if (TDDestEncoding != TDFile.TDEncoding)
            {
                try
                {
                    // Read the entire content of the file into memory using the source encoding
                    string content;
                    using (StreamReader reader = new StreamReader(filePath, sourceEncoding))
                    {
                        content = reader.ReadToEnd();
                    }

                    // Overwrite the file with the new encoding
                    using (StreamWriter writer = new StreamWriter(filePath, false, targetEncoding))
                    {
                        writer.Write(content);
                    }
                    Logger.LogDebug($"ConvertFileEncodingInPlace {TDFile.TDEncoding} -> {TDDestEncoding} = OK");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEx($"Error ConvertFileEncodingInPlace:", ex);
                    return false;
                }
            }
            Logger.LogDebug($"ConvertFileEncodingInPlace {TDFile.TDEncoding} -> {TDDestEncoding} = file is already in destination encoding");
            return true;
        }

        private static bool RegistryGetUTFOption(TDVersion tdversion, out int option)
        {
            option = 0;
            string version = tdversion.ToString();
            string versionNr = version.Substring(version.Length - 1);

            // Define the registry path and the value name
            string registryPath = $@"Software\Gupta\SQLWindows 7.{versionNr}\Settings";
            string valueName = "PreferUTF8Encoding";

            try
            {
                // Retrieve the current value of the registry key
                object currentValue = Registry.GetValue(@"HKEY_CURRENT_USER\" + registryPath, valueName, null);

                if (currentValue != null && currentValue is int v)
                {
                    // Store the current value (casting to int because it's a DWORD)
                    option = v;
                    string flagname = string.Empty;

                    if (option == 0)
                        flagname = "save UTF16, read UTF16";
                    if (option == 1)
                        flagname = "save UTF8, read UTF16";
                    if (option == 2)
                        flagname = "save UTF16, read UTF8";
                    else if (option == 3)
                        flagname = "save UTF8, read UTF8";
                    else
                        flagname = "Unknown option";
                    Logger.LogDebug($"RegistryGetUTFOption option={option} ({flagname}) -> return = true");
                    return true;

                }
                Logger.LogDebug($"RegistryGetUTFOption no value found for PreferUTF8Encoding");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error RegistryGetUTFOption:", ex);
                return false;
            }
        }

        private static bool RegistrySetUTFOption(TDVersion tdversion, int option)
        {
            string version = tdversion.ToString();
            string versionNr = version.Substring(version.Length - 1);

            // Define the registry path and the value name
            string registryPath = $@"Software\Gupta\SQLWindows 7.{versionNr}\Settings";
            string valueName = "PreferUTF8Encoding";

            string flagname = string.Empty;

            if (option == 0)
                flagname = "save UTF16, read UTF16";
            else if (option == 1)
                flagname = "save UTF8, read UTF16";
            else if (option == 2)
                flagname = "save UTF16, read UTF8";
            else if (option == 3)
                flagname = "save UTF8, read UTF8";
            else
                flagname = "Unknown option";

            try
            {
                // Retrieve the current value of the registry key
                object currentValue = Registry.GetValue(@"HKEY_CURRENT_USER\" + registryPath, valueName, null);

                if (currentValue != null && currentValue is int)
                {
                    // Update the value
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryPath))
                    {
                        if (key != null)
                        {
                            key.SetValue(valueName, option, RegistryValueKind.DWord);
                            Logger.LogDebug($"RegistrySetUTFOption {valueName}={option} ({flagname}) -> return = true");
                            return true;
                        }
                    }
                }
                else
                {
                    // Create or set the new value
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryPath))
                    {
                        if (key != null)
                        {
                            key.SetValue(valueName, option, RegistryValueKind.DWord);
                            Logger.LogDebug($"RegistrySetUTFOption {valueName}={option} ({flagname}) -> return = true");
                            return true;
                        }
                    }
                }
                Logger.LogDebug($"RegistrySetUTFOption {valueName}={option} ({flagname}) -> return = false");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error RegistrySetUTFOption:", ex);
                return false;
            }
        }
    }

    // This class is used to run as thread. It will detect SqlWindows messageboxes while opening sources
    // Found messageboxes will automatically be closed so not blocking the conversion process.
    internal static class MessageBoxCloser
    {
        private static Thread _closeThread;
        private static bool _stopRequested = false;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDlgItem(IntPtr hWnd, int nIDDlgItem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const uint BM_CLICK = 0x00F5;  // Button click message
        private const int WM_COMMAND = 0x0111;
        private const int IDOK = 1;

        public static List<string> messageBoxTexts = new List<string>();

        public static void Start()
        {
            messageBoxTexts = new List<string>();
            _stopRequested = false;
            _closeThread = new Thread(CloseMessageBoxes)
            {
                IsBackground = true // Optional: Makes the thread stop when the app closes
            };
            _closeThread.Start();
            Logger.LogDebug($"MessageBoxCloser thread started");
        }

        public static void Stop()
        {
            _stopRequested = true;
            if (_closeThread != null && _closeThread.IsAlive)
            {
                Logger.LogDebug($"MessageBoxCloser thread stopped");
                _closeThread.Join(); // Wait for the thread to stop
            }
        }

        private static void CloseMessageBoxes()
        {
            while (!_stopRequested)
            {
                CloseSQLWindowsMessageBox();

                // Sleep for a short time to avoid busy-waiting
                Thread.Sleep(20);
            }
        }

        //private static void GetMsgBoxText( IntPtr hWndMessageBox)
        //{
        //    IntPtr hWndStatic = GetDlgItem(hWndMessageBox, 0xFFFF);

        //    if (hWndStatic != IntPtr.Zero)
        //    {
        //        // Prepare a StringBuilder to receive the text (up to 512 characters)
        //        StringBuilder sText = new StringBuilder(512);

        //        // Get the window text from the static control
        //        GetWindowText(hWndStatic, sText, sText.Capacity);
        //        messageBoxTexts.Add(sText.ToString().Replace("\t", ""));
        //    }
        //}

        private static void CloseSQLWindowsMessageBox()
        {
            IntPtr hWndMessageBox = IntPtr.Zero;
            uint currentProcessId = GetCurrentProcessId();
            IntPtr nextWindow = IntPtr.Zero;

            try
            {
                // Use FindWindowEx to search for all windows with class "#32770" (Dialog class)
                do
                {
                    nextWindow = FindWindowEx(IntPtr.Zero, nextWindow, "#32770", null);
                    if (nextWindow != IntPtr.Zero)
                    {
                        uint windowProcessId = 0;
                        GetWindowThreadProcessId(nextWindow, out windowProcessId);

                        if (windowProcessId == currentProcessId)
                        {
                            // Check if the dialog has the desired structure
                            IntPtr okButtonHandle = IntPtr.Zero;
                            if (IsDesiredMessageBoxStructure(nextWindow, ref okButtonHandle))
                            {
                                SendMessage(okButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                                return;
                            }
                        }
                    }
                } while (nextWindow != IntPtr.Zero);

                return;
            }
            catch
            {
                return;
            }
        }

        // Helper method to verify the structure of the message box using FindWindowEx
        private static bool IsDesiredMessageBoxStructure(IntPtr hWnd, ref IntPtr okButtonHandle)
        {
            int buttonCount = 0;
            int staticCount = 0;
            string msgtext = string.Empty;

            // Find the first child window
            IntPtr childWindow = IntPtr.Zero;

            while ((childWindow = FindWindowEx(hWnd, childWindow, null, null)) != IntPtr.Zero)
            {
                StringBuilder className = new StringBuilder(256);
                GetClassName(childWindow, className, className.Capacity);

                if (className.ToString() == "Button")
                {
                    buttonCount++;
                    // Retrieve the button text to determine if it's "OK" or "Cancel"
                    StringBuilder buttonText = new StringBuilder(256);
                    GetWindowText(childWindow, buttonText, buttonText.Capacity);

                    if (buttonText.ToString() == "OK")
                    {
                        okButtonHandle = childWindow;
                    }
                    else if (buttonText.ToString() != "Cancel")
                    {
                        return false; // Invalid if a non-expected button is found
                    }
                }
                else if (className.ToString() == "Static")
                {
                    if(string.IsNullOrEmpty(msgtext))
                    {
                        // Prepare a StringBuilder to receive the text (up to 512 characters)
                        StringBuilder sText = new StringBuilder(512);

                        // Get the window text from the static control
                        GetWindowText(childWindow, sText, sText.Capacity);

                        // Convert to string and trim whitespace
                        string textContent = sText.ToString().Trim();

                        // Check if the text has meaningful content (non-whitespace)
                        if (!string.IsNullOrEmpty(textContent))
                        {
                            msgtext = textContent.Replace("\t", "");
                        }
                    }

                    staticCount++;
                }

                // If more than the required count is detected, exit early
                if (buttonCount > 2 || staticCount > 2)
                {
                    return false;
                }
            }

            bool found = (buttonCount == 2 && staticCount == 2 && okButtonHandle != IntPtr.Zero);

            if (found && !string.IsNullOrEmpty(msgtext))
            {
                messageBoxTexts.Add(msgtext);
            }
            // Check that we found exactly two buttons (OK and Cancel) and two static controls
            return found;
        }
    }
}
