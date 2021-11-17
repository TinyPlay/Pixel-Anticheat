/*
 * Pixel Anti Cheat
 * ======================================================
 * This library allows you to organize a simple anti-cheat
 * for your game and take care of data security. You can
 * use it in your projects for free.
 *
 * Note that it does not guarantee 100% protection for
 * your game. If you are developing a multiplayer game -
 * never trust the client and check everything on
 * the server.
 * ======================================================
 * @developer       TinyPlay
 * @author          Ilya Rastorguev
 * @url             https://github.com/TinyPlay/Pixel-Anticheat
 */
namespace PixelAnticheat.Editor
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    
    internal class EditorGlobalStuff
    {
        internal const string WINDOWS_MENU_PATH = "Pixel Anti-Cheat/";
        
        internal const string PREFS_INJECTION_GLOBAL = "InjectorDetectionEnabledGlobal";
        internal const string PREFS_INJECTION = "InjectorDetectionEnabled";
        internal const string REPORT_EMAIL = "ceo@tpgames.ru";
        
        internal const string INJECTION_SERVICE_FOLDER = "InjectionDetectorData";
        internal const string INJECTION_DEFAULT_WHITELIST_FILE = "DefaultWhitelist.pixac";
        internal const string INJECTION_USER_WHITELIST_FILE = "UserWhitelist.pixac";
        internal const string INJECTION_DATA_FILE = "assmdb.pixac";
        internal const string INJECTION_DATA_SEPARATOR = ":";
        
        internal const string ASSEMBLIES_PATH_RELATIVE = "Library/ScriptAssemblies";
        
        internal static readonly string ASSETS_PATH = Application.dataPath;
        internal static readonly string RESOURCES_PATH = ASSETS_PATH + "/Resources/";
        internal static readonly string ASSEMBLIES_PATH = ASSETS_PATH + "/../" + ASSEMBLIES_PATH_RELATIVE;
        
        internal static readonly string INJECTION_DATA_PATH = RESOURCES_PATH + INJECTION_DATA_FILE;
        
        private static readonly string[] hexTable = Enumerable.Range(0, 256).Select(v => v.ToString("x2")).ToArray();
        
        /// <summary>
        /// Clean Injection Detector Data
        /// </summary>
        internal static void CleanInjectionDetectorData()
        {
            if (!File.Exists(INJECTION_DATA_PATH))
            {
                return;
            }

            RemoveReadOnlyAttribute(INJECTION_DATA_PATH);
            RemoveReadOnlyAttribute(INJECTION_DATA_PATH + ".meta");

            FileUtil.DeleteFileOrDirectory(INJECTION_DATA_PATH);
            FileUtil.DeleteFileOrDirectory(INJECTION_DATA_PATH + ".meta");

            RemoveDirectoryIfEmpty(RESOURCES_PATH);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        
        /// <summary>
        /// Resolve Injection Default Whitelist Path
        /// </summary>
        /// <returns></returns>
        internal static string ResolveInjectionDefaultWhitelistPath()
        {
            return ResolveInjectionServiceFolder() + "/" + INJECTION_DEFAULT_WHITELIST_FILE;
        }
        
        /// <summary>
        /// Resolve Inject User Whitelist Path
        /// </summary>
        /// <returns></returns>
        internal static string ResolveInjectionUserWhitelistPath()
        {
            return ResolveInjectionServiceFolder() + "/" + INJECTION_USER_WHITELIST_FILE;
        }
        
        /// <summary>
        /// Resolve Injection Service Folder
        /// </summary>
        /// <returns></returns>
        internal static string ResolveInjectionServiceFolder()
        {
            string result = "";
            string[] targetFiles = Directory.GetDirectories(ASSETS_PATH, INJECTION_SERVICE_FOLDER, SearchOption.AllDirectories);
            if (targetFiles.Length == 0)
            {
                Debug.LogError("Can't find " + INJECTION_SERVICE_FOLDER + " folder! Please report to " + REPORT_EMAIL);
            }
            else
            {
                result = targetFiles[0];
            }

            return result;
        }

        /// <summary>
        /// Find Libraries At
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal static string[] FindLibrariesAt(string dir)
        {
            string[] result = new string[0];

            if (Directory.Exists(dir))
            {
                result = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = result[i].Replace('\\', '/');
                }
            }

            return result;
        }

        /// <summary>
        /// Public Key token to String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string PublicKeyTokenToString(byte[] bytes)
        {
            string result = "";

            // AssemblyName.GetPublicKeyToken() returns 8 bytes
            for (int i = 0; i < 8; i++)
            {
                result += hexTable[bytes[i]];
            }

            return result;
        }
        
        /// <summary>
        /// Remove Directory if Empty
        /// </summary>
        /// <param name="directoryName"></param>
        private static void RemoveDirectoryIfEmpty(string directoryName)
        {
            if (Directory.Exists(directoryName) && IsDirectoryEmpty(directoryName))
            {
                FileUtil.DeleteFileOrDirectory(directoryName);
                if (File.Exists(Path.GetDirectoryName(directoryName) + ".meta"))
                {
                    FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(directoryName) + ".meta");
                }
            }
        }

        /// <summary>
        /// Is Directory Empty
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsDirectoryEmpty(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            return dirs.Length == 0 && files.Length == 0;
        }

        /// <summary>
        /// Get Assembly Hash
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        internal static int GetAssemblyHash(AssemblyName ass)
        {
            string hashInfo = ass.Name;

            byte[] bytes = ass.GetPublicKeyToken();
            if (bytes != null && bytes.Length == 8)
            {
                hashInfo += PublicKeyTokenToString(bytes);
            }

            // Jenkins hash function (http://en.wikipedia.org/wiki/Jenkins_hash_function)
            int result = 0;
            int len = hashInfo.Length;

            for (int i = 0; i < len; ++i)
            {
                result += hashInfo[i];
                result += (result << 10);
                result ^= (result >> 6);
            }
            result += (result << 3);
            result ^= (result >> 11);
            result += (result << 15);

            return result;
        }
        
        /// <summary>
        /// Remove Read Only Attribute
        /// </summary>
        /// <param name="path"></param>
        internal static void RemoveReadOnlyAttribute(string path)
        {
            if (File.Exists(path))
            {
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes = attributes & ~FileAttributes.ReadOnly;
                    File.SetAttributes(path, attributes);
                }
            }
        }
    }
}