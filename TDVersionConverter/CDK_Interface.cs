using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;

namespace TDVersionExplorer
{
    internal static partial class TDConvert
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate ushort CDKLoadAppDelegate(string file);
        private static CDKLoadAppDelegate CDKLoadApp = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate ushort CDKLoadAppASCIIDelegate(string file);
        private static CDKLoadAppASCIIDelegate CDKLoadAppASCII = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool CDKReleaseAppDelegate(ushort houtline);
        private static CDKReleaseAppDelegate CDKReleaseApp = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool CDKOutlineSaveDelegate(ushort houtline, string file);
        private static CDKOutlineSaveDelegate CDKOutlineSave = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate bool CDKOutlineSaveASCIIDelegate(ushort houtline, string file);
        private static CDKOutlineSaveASCIIDelegate CDKOutlineSaveASCII = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool CDKOutlineSaveAsTextDelegate(ushort houtline, string file, bool indented);
        private static CDKOutlineSaveAsTextDelegate CDKOutlineSaveAsText = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool CDKOutlineSaveAsTextUTFDelegate(ushort houtline, string file, int flags);
        private static CDKOutlineSaveAsTextUTFDelegate CDKOutlineSaveAsTextUTF = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate bool CDKOutlineSaveAsTextASCIIDelegate(ushort houtline, string file, bool indented);
        private static CDKOutlineSaveAsTextASCIIDelegate CDKOutlineSaveAsTextASCII = null;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate bool CDKSetUTF8OptionDelegate(int loadOrSave, bool bEnable);
        private static CDKSetUTF8OptionDelegate CDKSetUTF8Option = null;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool TryGetFunctionDelegate<T>(string dllPath, string functionName, ref T functionDelegate) where T : Delegate
        {
            // Check if the delegate is already set
            if (functionDelegate != null)
            {
                Logger.LogDebug($"TryGetFunctionDelegate OK (already set): {functionName}");
                return true;
            }

            // Use GetModuleHandle to retrieve the handle to the already-loaded DLL
            IntPtr hModule = GetModuleHandle(dllPath);

            try
            {
                if (hModule == IntPtr.Zero)
                {
                    hModule = LoadLibrary(dllPath);
                    if (hModule == IntPtr.Zero)
                    {
                        Logger.LogError($"Error TryGetFunctionDelegate: Failed to load DLL: {dllPath}");
                        return false; // Return false when DLL load fails
                    }
                    else
                        Logger.LogDebug($"TryGetFunctionDelegate LoadLibrary({dllPath}) -> OK");
                }

                // Get the address of the specified function
                IntPtr pFunction = GetProcAddress(hModule, functionName);

                if (pFunction == IntPtr.Zero)
                {
                    Logger.LogError($"Error TryGetFunctionDelegate: Failed to get function address for {functionName}");
                    return false; // Return false if function address is not found
                }

                // Create a delegate for the function dynamically
                functionDelegate = Marshal.GetDelegateForFunctionPointer<T>(pFunction);
                Logger.LogDebug($"TryGetFunctionDelegate OK: {dllPath}\t{functionName}");
                return true; // Return true when delegate is successfully set
            }
            catch (AccessViolationException ex)
            {
                Logger.LogErrorEx($"Error TryGetFunctionDelegate:", ex);
                return false; // Return false on AccessViolationException
            }
            catch (Exception ex)
            {
                Logger.LogErrorEx($"Error TryGetFunctionDelegate:", ex);
                return false; // Return false on any other exception
            }
        }
    }
}
