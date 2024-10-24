using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace TDVersionExplorer
{
    public static class TDVersionRepository
    {
        // Dictionary to store TD versions, with NormalVersion as the key.
        public static Dictionary<TDVersion, TDVersionInfo> Versions { get; }

        // Dictionary to store installed TD versions
        public static Dictionary<string, InstalledTDVersionInfo> InstalledVersions = new Dictionary<string, InstalledTDVersionInfo>();

        // Constructor to initialize the dictionary with data.
        static TDVersionRepository()
        {
            Versions = new Dictionary<TDVersion, TDVersionInfo>
            {
                { TDVersion.QRP_TD10_TD30,  new TDVersionInfo(TDVersion.QRP_TD10_TD30, "1.0.3") },
                { TDVersion.QRP_TD31,       new TDVersionInfo(TDVersion.QRP_TD31, "1.0.4") },
                { TDVersion.QRP_TD40,       new TDVersionInfo(TDVersion.QRP_TD40, "1.0.5") },
                { TDVersion.QRP_TD41_TD42,  new TDVersionInfo(TDVersion.QRP_TD41_TD42, "1.0.6") },
                { TDVersion.QRP_TD51_TDXX,  new TDVersionInfo(TDVersion.QRP_TD51_TDXX, "1.0.7") },
                { TDVersion.UNKNOWN,        new TDVersionInfo(TDVersion.UNKNOWN, "0.0.0") },
                { TDVersion.TD10,           new TDVersionInfo(TDVersion.TD10, "4.0.26") },
                { TDVersion.TD11,           new TDVersionInfo(TDVersion.TD11, "4.0.26") },
                { TDVersion.TD15,           new TDVersionInfo(TDVersion.TD15, "4.0.27") },
                { TDVersion.TD20_TD21,      new TDVersionInfo(TDVersion.TD20_TD21, "4.0.28") },
                { TDVersion.TD30,           new TDVersionInfo(TDVersion.TD30, "4.0.31") },
                { TDVersion.TD31,           new TDVersionInfo(TDVersion.TD31, "4.0.32") },
                { TDVersion.TD40,           new TDVersionInfo(TDVersion.TD40, "4.0.34") },
                { TDVersion.TD41_TD42,      new TDVersionInfo(TDVersion.TD41_TD42, "4.0.35") },
                { TDVersion.TD51,           new TDVersionInfo(TDVersion.TD51, "4.0.37") },
                { TDVersion.TD52,           new TDVersionInfo(TDVersion.TD52, "4.0.39") },
                { TDVersion.TD60,           new TDVersionInfo(TDVersion.TD60, "4.0.41") },
                { TDVersion.TD61,           new TDVersionInfo(TDVersion.TD61, "4.0.47") },
                { TDVersion.TD62,           new TDVersionInfo(TDVersion.TD62, "4.0.50") },
                { TDVersion.TD63,           new TDVersionInfo(TDVersion.TD63, "4.0.52") },
                { TDVersion.TD70,           new TDVersionInfo(TDVersion.TD70, "4.0.53") },
                { TDVersion.TD71,           new TDVersionInfo(TDVersion.TD71, "4.0.54") },
                { TDVersion.TD72,           new TDVersionInfo(TDVersion.TD72, "4.0.55") },
                { TDVersion.TD73,           new TDVersionInfo(TDVersion.TD73, "4.0.56") },
                { TDVersion.TD74,           new TDVersionInfo(TDVersion.TD74, "4.0.57") },
                { TDVersion.TD750,          new TDVersionInfo(TDVersion.TD750, "4.0.58") },
                { TDVersion.TD75,           new TDVersionInfo(TDVersion.TD75, "4.0.59") }
                // Add more TD versions here...
            };
        }

        public static void GetInstalledTDVersions()
        {
            InstalledVersions = new Dictionary<string, InstalledTDVersionInfo>();

            string[] companyNames = { "Gupta", "Unify", "Centura" };

            foreach (string company in companyNames)
            {
                string baseRegistryKey = $@"SOFTWARE\{company}";

                using (RegistryKey companyKey = Registry.CurrentUser.OpenSubKey(baseRegistryKey))
                {
                    if (companyKey == null) continue;

                    foreach (string subKeyName in companyKey.GetSubKeyNames())
                    {
                        if (subKeyName.StartsWith("SQLWindows"))
                        {
                            using (RegistryKey versionKey = companyKey.OpenSubKey(subKeyName))
                            {
                                if (versionKey == null) continue;

                                string installLocation = versionKey.GetValue("InstallLocation") as string;
                                if (string.IsNullOrEmpty(installLocation) || !Directory.Exists(installLocation)) continue;

                                
                                // Extract X.Y version (e.g., 7.0)
                                string versionString = subKeyName.Split(' ')[1];
                                string xyVersion = versionString.Replace(".", "");  // Convert "7.0" to "70"
                                string executableName = $"cbi{xyVersion}.exe";
                                string fullPath = Path.Combine(installLocation, executableName).ToLower();
                                string versionStr = $"TD{xyVersion}";

                                if (File.Exists(fullPath) && !InstalledVersions.ContainsKey(fullPath))
                                {
                                    bool isX64 = versionKey.ToString().Contains("x64");
                                    TDBitness bitness = TDBitness.x86;

                                    if (isX64)
                                        bitness = TDBitness.x64;

                                    if (!Enum.TryParse(versionStr, out TDVersion matchedVersion))
                                    {
                                        if (versionStr == "TD20" || versionStr == "TD21")
                                            matchedVersion = TDVersion.TD20_TD21;
                                        if (versionStr == "TD41" || versionStr == "TD42")
                                            matchedVersion = TDVersion.TD41_TD42;
                                    }

                                    InstalledTDVersionInfo versionInfo = new InstalledTDVersionInfo()
                                    {
                                        VersionStr = versionStr,
                                        TDVersion = matchedVersion,
                                        Bitness = bitness,
                                        InstallPath = fullPath
                                    };
                                    InstalledVersions.Add(fullPath, versionInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static TDVersionInfo GetByTDVersion(TDVersion tdversion)
        {
            if (Versions.TryGetValue(tdversion, out TDVersionInfo foundVersion))
            {
                return foundVersion;
            }

            // Return the 'UNKNOWN' version if not found.
            return Versions[TDVersion.UNKNOWN];
        }

        public static TDVersionInfo GetByOutlineVersion(string outlineVersion)
        {
            foreach (var version in Versions.Values)
            {
                if (version.OutlineVersion == outlineVersion)
                {
                    return version;
                }
            }
            // Return the 'UNKNOWN' version if not found.
            return Versions[TDVersion.UNKNOWN];
        }

        public static int GetAvailableTDInstalls(TDVersion tdversion, TDBitness bitness, out List<InstalledTDVersionInfo> installations)
        {
            installations = new List<InstalledTDVersionInfo>();

            foreach (InstalledTDVersionInfo version in InstalledVersions.Values)
            {
                if (version.TDVersion == tdversion)
                {
                    if (bitness == TDBitness.UNKNOWN || (version.Bitness == bitness && version.TDVersion == tdversion))
                        installations.Add(version);
                }
            }
            return installations.Count;
        }
    }

    public enum TDVersion
    {
        UNKNOWN = 0,
        KEEP_ORIGINAL = 1,
        QRP_TD10_TD30 = 0x0067,
        QRP_TD31 = 0x0068,
        QRP_TD40 = 0x0069,
        QRP_TD41_TD42 = 0x006A,
        QRP_TD51_TDXX = 0x006B,
        TD10 = 0x01F9,
        TD11 = 0x01FA,
        TD15 = 0x01FB,
        TD20_TD21 = 0x01FC,
        //TD20 = 0x01FC,
        //TD21 = 0x01FC,
        TD30 = 0x01FF,
        TD31 = 0x0200,
        TD40 = 0x0202,
        TD41_TD42 = 0x0203,
        //TD41 = 0x0203,
        //TD42 = 0x0203,
        TD51 = 0x0206,
        TD52 = 0x0209,
        TD60 = 0x020B,
        TD61 = 0x0211,
        TD62 = 0x0214,
        TD63 = 0x0216,
        TD70 = 0x0217,
        TD71 = 0x0218,
        TD72 = 0x0219,
        TD73 = 0x021A,
        TD74 = 0x021B,
        TD750 = 0x021C,
        TD75 = 0x021D
    }

    public enum TDOutlineFormat
    {
        UNKNOWN = 0,
        NORMAL = 1,
        TEXT = 2,
        TEXTINDENTED = 3,
        COMPILED = 4,
        KEEP_ORIGINAL = 5
    }

    public enum TDFileType
    {
        UNKNOWN = 0,
        SOURCE = 1,
        DYNALIB = 2,
        QRP = 3,
        EXE = 4,
        DLL = 5
    }

    public enum TDBitness
    {
        UNKNOWN = 0,
        x86 = 1,
        x64 = 2
    }

    public enum TDEncoding
    {
        UNKNOWN = 0,
        UTF8_ASCII = 1,
        UTF16 = 2,
        KEEP_ORIGINAL = 3
    }

    public class TDVersionInfo
    {
        public TDVersion NormalVersion { get; set; }
        public string OutlineVersion { get; set; }

        public TDVersionInfo(TDVersion normalVersion, string outlineVersion)
        {
            NormalVersion = normalVersion;
            OutlineVersion = outlineVersion;
        }
        public TDVersionInfo()
        {
            NormalVersion = TDVersion.UNKNOWN;
            OutlineVersion = "";
        }
    }

    public class InstalledTDVersionInfo
    {
        public string VersionStr { get; set; }
        public string InstallPath { get; set; }
        public TDVersion TDVersion { get; set; }
        public TDBitness Bitness { get; set; }
    }

    internal enum BitnessByteValue
    {
        UNKNOWN = 0,
        APD_x86 = 0x0017,
        APD_x64 = 0x0037,
        SRC_x86_Normal = 0x2,
        SRC_x86_Compiled = 0x3,
        SRC_x64_Normal = 0x0022,
        SRC_x64_Compiled = 0x0023,
        EXE_x86 = 0x000F,
        EXE_x64 = 0x002F,
    }
}
