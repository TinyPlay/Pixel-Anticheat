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
#define DEBUG
#undef DEBUG

#define DEBUG_VERBOSE
#undef DEBUG_VERBOSE

#define DEBUG_PARANIOD
#undef DEBUG_PARANIOD

namespace PixelAnticheat.Editor
{
	using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using PixelAnticheat.SecuredTypes;
    using PixelAnticheat.Editor.Common;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using Debug = UnityEngine.Debug;
    #if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
    using System.Diagnostics;
    #endif
    
    /// <summary>
    /// Act Processor
    /// </summary>
    internal class Postprocessor : AssetPostprocessor
    {
        private static readonly List<AllowedAssembly> allowedAssemblies = new List<AllowedAssembly>();
        private static readonly List<string> allLibraries = new List<string>();
        
		#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
		[UnityEditor.MenuItem("Pixel Anti-Cheat/Force Collect Data for Injection Detector")]
		private static void CallInjectionScan()
		{
			InjectionAssembliesScan(true); 
		}
		#endif
	    
	    // called by Unity
	    private static void OnPostprocessAllAssets(String[] mportedAssets, String[] deletedAssets, String[] movedAssets, String[] movedFromAssetPaths)
	    {
		    if (!EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL)) return;
		    if (!InjectionDetectorTargetCompatibleCheck()) return;

		    if (deletedAssets.Length > 0)
		    {
			    foreach (string deletedAsset in deletedAssets)
			    {
				    if (deletedAsset.IndexOf(EditorGlobalStuff.INJECTION_DATA_FILE) > -1 && !EditorApplication.isCompiling)
				    {
						#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
						Debug.LogWarning("Looks like Injection Detector data file was accidentally removed! Re-creating...\nIf you wish to remove " + EditorGlobalStuff.INJECTION_DATA_FILE + " file, just disable Injection Detecotr in the Anti-Cheat Options window.");
						#endif
					    InjectionAssembliesScan();
				    }
			    }
		    }
	    }
	    
	    // called by Unity
	    [DidReloadScripts]
	    private static void ScriptsWereReloaded()
	    {
		    EditorUserBuildSettings.activeBuildTargetChanged += OnBuildTargetChanged;
		    if (EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL))
		    {
			    InjectionAssembliesScan();
		    }
	    }
	    
	    private static void OnBuildTargetChanged()
	    {
		    InjectionDetectorTargetCompatibleCheck();
	    }

	    internal static void InjectionAssembliesScan()
	    {
		    InjectionAssembliesScan(false);
	    }
	    
	    internal static void InjectionAssembliesScan(bool forced)
		{
			if (!InjectionDetectorTargetCompatibleCheck() && !forced)
			{
				return;
			}

			#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			Stopwatch sw = Stopwatch.StartNew();
	#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Injection Detector Assemblies Scan\n");
			Debug.Log("Paths:\n" +

			          "Assets: " + EditorGlobalStuff.ASSETS_PATH + "\n" +
			          "Assemblies: " + EditorGlobalStuff.ASSEMBLIES_PATH + "\n" +
			          "Injection Detector Data: " + EditorGlobalStuff.INJECTION_DATA_PATH);
			sw.Start();
	#endif
			#endif

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Looking for all assemblies in current project...");
			sw.Start();
#endif
			allLibraries.Clear();
			allowedAssemblies.Clear();

			allLibraries.AddRange(EditorGlobalStuff.FindLibrariesAt(EditorGlobalStuff.ASSETS_PATH));
			allLibraries.AddRange(EditorGlobalStuff.FindLibrariesAt(EditorGlobalStuff.ASSEMBLIES_PATH));
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Total libraries found: " + allLibraries.Count);
			sw.Start();
#endif
			const string editorSubdir = "/editor/";
			string assembliesPathLowerCase = EditorGlobalStuff.ASSEMBLIES_PATH_RELATIVE.ToLower();
			foreach (string libraryPath in allLibraries)
			{
				string libraryPathLowerCase = libraryPath.ToLower();
#if (DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log("Checking library at the path: " + libraryPathLowerCase);
				sw.Start();
#endif
				if (libraryPathLowerCase.Contains(editorSubdir)) continue;
				if (libraryPathLowerCase.Contains("-editor.dll") && libraryPathLowerCase.Contains(assembliesPathLowerCase)) continue;

				try
				{
					AssemblyName assName = AssemblyName.GetAssemblyName(libraryPath);
					string name = assName.Name;
					int hash = EditorGlobalStuff.GetAssemblyHash(assName);

					AllowedAssembly allowed = allowedAssemblies.FirstOrDefault(allowedAssembly => allowedAssembly.name == name);

					if (allowed != null)
					{
						allowed.AddHash(hash);
					}
					else
					{
						allowed = new AllowedAssembly(name, new[] {hash});
						allowedAssemblies.Add(allowed);
					}
				}
				catch
				{
					// not a valid IL assembly, skipping
				}
			}

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			string trace = "Found assemblies (" + allowedAssemblies.Count + "):\n";

			foreach (AllowedAssembly allowedAssembly in allowedAssemblies)
			{
				trace += "  Name: " + allowedAssembly.name + "\n";
				trace = allowedAssembly.hashes.Aggregate(trace, (current, hash) => current + ("    Hash: " + hash + "\n"));
			}

			Debug.Log(trace);
			sw.Start();
#endif
			if (!Directory.Exists(EditorGlobalStuff.RESOURCES_PATH))
			{
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log("Creating resources folder: " + EditorGlobalStuff.RESOURCES_PATH);
				sw.Start();
#endif
				Directory.CreateDirectory(EditorGlobalStuff.RESOURCES_PATH);
			}

			EditorGlobalStuff.RemoveReadOnlyAttribute(EditorGlobalStuff.INJECTION_DATA_PATH);
			BinaryWriter bw = new BinaryWriter(new FileStream(EditorGlobalStuff.INJECTION_DATA_PATH, FileMode.Create, FileAccess.Write, FileShare.Read));
			int allowedAssembliesCount = allowedAssemblies.Count;

			int totalWhitelistedAssemblies = 0;

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Processing default whitelist");
			sw.Start();
			#endif

			string defaultWhitelistPath = EditorGlobalStuff.ResolveInjectionDefaultWhitelistPath();
			if (File.Exists(defaultWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(defaultWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();
				totalWhitelistedAssemblies = assembliesCount + allowedAssembliesCount;

				bw.Write(totalWhitelistedAssemblies);

				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}
			else
			{
				#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
				#endif
				bw.Close();
				Debug.LogError("Can't find " + EditorGlobalStuff.INJECTION_DEFAULT_WHITELIST_FILE + " file!\nPlease, report to " + EditorGlobalStuff.REPORT_EMAIL);
				return;
			}

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Processing user whitelist");
			sw.Start();
			#endif

			string userWhitelistPath = EditorGlobalStuff.ResolveInjectionUserWhitelistPath();
			if (File.Exists(userWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(userWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();

				bw.Seek(0, SeekOrigin.Begin);
				bw.Write(totalWhitelistedAssemblies + assembliesCount);
				bw.Seek(0, SeekOrigin.End);
				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Processing project assemblies");
			sw.Start();
			#endif

			for (int i = 0; i < allowedAssembliesCount; i++)
			{
				AllowedAssembly assembly = allowedAssemblies[i];
				string name = assembly.name;
				string hashes = "";

				for (int j = 0; j < assembly.hashes.Length; j++)
				{
					hashes += assembly.hashes[j];
					if (j < assembly.hashes.Length - 1)
					{
						hashes += EditorGlobalStuff.INJECTION_DATA_SEPARATOR;
					}
				}

				string line = SecuredString.EncryptDecrypt(name + EditorGlobalStuff.INJECTION_DATA_SEPARATOR + hashes, "TinyPlay");
				
				#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				Debug.Log("Writing assembly:\n" + name + EditorGlobalStuff.INJECTION_DATA_SEPARATOR + hashes);
				#endif
				bw.Write(line);
			}

			bw.Close();			 
			#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log("Assemblies scan duration: " + sw.ElapsedMilliseconds + " ms.");
			#endif

			if (allowedAssembliesCount == 0)
			{
				Debug.LogError("Can't find any assemblies!\nPlease, report to " + EditorGlobalStuff.REPORT_EMAIL);
			}

			AssetDatabase.Refresh();
			//EditorApplication.UnlockReloadAssemblies();
		}

		private static bool InjectionDetectorTargetCompatibleCheck()
		{
			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IPHONE || UNITY_ANDROID
			if (EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL) && !EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION))
			{
				EditorPrefs.SetBool(EditorGlobalStuff.PREFS_INJECTION, true);
			}

			return true;
			
			#else
			bool injectionEnabled = EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL);
			if (injectionEnabled && EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION))
			{
				Debug.LogWarning("Injection Detector is not available on selected platform (" + EditorUserBuildSettings.activeBuildTarget + ") and will be disabled!");
				EditorPrefs.SetBool(EditorGlobalStuff.PREFS_INJECTION, false);
				EditorGlobalStuff.CleanInjectionDetectorData();
			}
			return false;
			#endif
		}
    }
}