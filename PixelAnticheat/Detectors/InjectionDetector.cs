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

#define DEBUG_PARANOID
#undef DEBUG_PARANOID

namespace PixelAnticheat.Detectors
{
    using System.IO;
    using System.Reflection;
    using PixelAnticheat.SecuredTypes;
    using Debug = UnityEngine.Debug;
    using System;
    using UnityEngine;
    using PixelAnticheat.Models;
    
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
    using System.Diagnostics;
    #endif
    
    /// <summary>
    /// Assembly Injection Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class InjectionDetector : BaseDetector
    {
        #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IPHONE || UNITY_ANDROID
        // Is Running
        private bool isRunning;
        
        // Private Params
        private bool signaturesAreNotGenuine;
        private AllowedAssembly[] allowedAssemblies;
        private string[] hexTable;
        
        // Prevent Direct Access
        private InjectionDetector() { }

        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if (isRunning || !enabled)
                return;
            
            #if UNITY_EDITOR
            if (!EditorPrefs.GetBool("InjectorDetectionEnabled", false))
            {
                Debug.LogWarning("Injection Detection is not enabled in Pixel Anti-Cheat Options!\nPlease, check readme for details.");
                return;
            }
            
            #if !DEBUG && !DEBUG_VERBOSE && !DEBUG_PARANOID
            if (Application.isEditor)
            {
                Debug.LogWarning("Injection Detection does not work in editor (check readme for details).");
                return;
            }
            #else
			Debug.LogWarning("Injection Detection works in debug mode. There WILL BE false positives in editor, it's fine!");
            #endif
            #endif
            
            if (allowedAssemblies == null)
            {
                LoadAndParseAllowedAssemblies();
            }

            if (signaturesAreNotGenuine)
            {
                OnInjectionDetected();
                return;
            }

            if (!FindInjectionInCurrentAssemblies())
            {
                // listening for new assemblies
                AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
                isRunning = true;
            }
            else
            {
                OnInjectionDetected();
            }
        }
        
        /// <summary>
        /// Stop Detector
        /// </summary>
        public override void StopDetector()
        {
            if (isRunning)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
                OnCheatingDetected?.RemoveAllListeners();
                isRunning = false;
            }
        }

        /// <summary>
        /// Pause Detector
        /// </summary>
        public override void PauseDetector()
        {
            isRunning = false;
            AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
        }

        /// <summary>
        /// Resume Detector
        /// </summary>
        public override void ResumeDetector()
        {
            isRunning = true;
            AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
        }

        /// <summary>
        /// Check if is Running
        /// </summary>
        /// <returns></returns>
        public override bool IsRunning()
        {
            return isRunning;
        }
        
        /// <summary>
        /// On Injection Detected
        /// </summary>
        private void OnInjectionDetected()
        {
            OnCheatingDetected?.Invoke(CheatingMessages.InjectionCheatingMessage);
            StopDetector();
        }

        /// <summary>
        /// On New Assembly Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnNewAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Debug.Log("New assembly loaded: " + args.LoadedAssembly.FullName);
            #endif
            
            if (!AssemblyAllowed(args.LoadedAssembly))
            {
                #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
				Debug.Log("Injected Assembly found:\n" + args.LoadedAssembly.FullName);
                #endif
                OnInjectionDetected();
            }
        }

        /// <summary>
        /// Find Injection In Current Assemblies
        /// </summary>
        /// <returns></returns>
        private bool FindInjectionInCurrentAssemblies()
        {
            bool result = false;
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Stopwatch stopwatch = Stopwatch.StartNew();
            #endif
            
            Assembly[] assembliesInCurrentDomain = AppDomain.CurrentDomain.GetAssemblies();
            if (assembliesInCurrentDomain.Length == 0)
            {
                #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
				stopwatch.Stop();
				Debug.Log("0 assemblies in current domain! Not genuine behavior.");
				stopwatch.Start();
                #endif
                result = true;
            }else
            {
                foreach (Assembly ass in assembliesInCurrentDomain)
                {
                    #if DEBUG_VERBOSE	
				    stopwatch.Stop();
				    Debug.Log("Currenly loaded assembly:\n" + ass.FullName);
				    stopwatch.Start();
                    #endif
                    
                    if (!AssemblyAllowed(ass))
                    {
                        #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
						stopwatch.Stop();
						Debug.Log("Injected Assembly found:\n" + ass.FullName + "\n" + GetAssemblyHash(ass));
						stopwatch.Start();
                        #endif
                        
                        result = true;
                        break;
                    }
                }
            }
            
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			stopwatch.Stop();
			Debug.Log("Loaded assemblies scan duration: " + stopwatch.ElapsedMilliseconds + " ms.");
            #endif
            
            return result;
        }

        /// <summary>
        /// Check Allowed Assembly
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        private bool AssemblyAllowed(Assembly ass)
        {
            #if !UNITY_WEBPLAYER
            string assemblyName = ass.GetName().Name;
            #else
			string fullname = ass.FullName;
			string assemblyName = fullname.Substring(0, fullname.IndexOf(", ", StringComparison.Ordinal));
            #endif
            
            int hash = GetAssemblyHash(ass);
            
            bool result = false;
            for (int i = 0; i < allowedAssemblies.Length; i++)
            {
                AllowedAssembly allowedAssembly = allowedAssemblies[i];

                if (allowedAssembly.name == assemblyName)
                {
                    if (Array.IndexOf(allowedAssembly.hashes, hash) != -1)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Load and Parse Allowed Assemblies
        /// </summary>
        private void LoadAndParseAllowedAssemblies()
        {
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Debug.Log("Starting LoadAndParseAllowedAssemblies()");
			Stopwatch sw = Stopwatch.StartNew();
            #endif
            
            TextAsset assembliesSignatures = (TextAsset)Resources.Load("assmdb", typeof(TextAsset));
            if (assembliesSignatures == null)
            {
                signaturesAreNotGenuine = true;
                return;
            }
            
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log("Creating separator array and opening MemoryStream");
			sw.Start();
            #endif
            
            string[] separator = {":"};

            MemoryStream ms = new MemoryStream(assembliesSignatures.bytes);
            BinaryReader br = new BinaryReader(ms);
			
            int count = br.ReadInt32();
            
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log("Allowed assemblies count from MS: " + count);
			sw.Start();
            #endif
            
            allowedAssemblies = new AllowedAssembly[count];
            
            for (int i = 0; i < count; i++)
            {
                string line = br.ReadString();
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log("ine: " + line);
				sw.Start();
                #endif
                line = SecuredString.EncryptDecrypt(line, "TinyPlay");
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log("Line decrypted : " + line);
				sw.Start();
                #endif
                
                string[] strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int stringsCount = strArr.Length;
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log("stringsCount : " + stringsCount);
				sw.Start();
                #endif
                
                if (stringsCount > 1)
                {
                    string assemblyName = strArr[0];

                    int[] hashes = new int[stringsCount - 1];
                    for (int j = 1; j < stringsCount; j++)
                    {
                        hashes[j - 1] = int.Parse(strArr[j]);
                    }

                    allowedAssemblies[i] = (new AllowedAssembly(assemblyName, hashes));
                }
                else
                {
                    signaturesAreNotGenuine = true;
                    br.Close();
                    ms.Close();
                    #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
					sw.Stop();
                    #endif
                    return;
                }
            }
            br.Close();
            ms.Close();
            Resources.UnloadAsset(assembliesSignatures);

            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log("Allowed Assemblies parsing duration: " + sw.ElapsedMilliseconds + " ms.");
            #endif

            hexTable = new string[256];
            for (int i = 0; i < 256; i++)
            {
                hexTable[i] = i.ToString("x2");
            }
        }

        /// <summary>
        /// Get Assembly Hash
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        private int GetAssemblyHash(Assembly ass)
        {
            string hashInfo;
            #if !UNITY_WEBPLAYER
            AssemblyName assName = ass.GetName();
            byte[] bytes = assName.GetPublicKeyToken();
            if (bytes.Length == 8)
            {
                hashInfo = assName.Name + PublicKeyTokenToString(bytes);
            }
            else
            {
                hashInfo = assName.Name;
            }
            #else
			string fullName = ass.FullName;

			string assemblyName = fullName.Substring(0, fullName.IndexOf(", ", StringComparison.Ordinal));
			int tokenIndex = fullName.IndexOf("PublicKeyToken=", StringComparison.Ordinal) + 15;
			string token = fullName.Substring(tokenIndex, fullName.Length - tokenIndex);
			if (token == "null") token = "";
			hashInfo = assemblyName + token;
            #endif
            
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
        
        #if !UNITY_WEBPLAYER
        private string PublicKeyTokenToString(byte[] bytes)
        {
            string result = "";
            for (int i = 0; i < 8; i++)
            {
                result += hexTable[bytes[i]];
            }

            return result;
        }
        #endif
        
        /// <summary>
        /// Allowed Assembly
        /// </summary>
        private class AllowedAssembly
        {
            public readonly string name;
            public readonly int[] hashes;

            public AllowedAssembly(string name, int[] hashes)
            {
                this.name = name;
                this.hashes = hashes;
            }
        }
        
        #else
        public override void StartDetector()
        {
            Debug.LogError("Injection Detector is not supported on selected platform!");
        }
        public override void StopDetector()
        {
            Debug.LogError("Injection Detector is not supported on selected platform!");
        }
        public override void PauseDetector()
        {
            Debug.LogError("Injection Detector is not supported on selected platform!");
        }
        public override void ResumeDetector()
        {
            Debug.LogError("Injection Detector is not supported on selected platform!");
        }
        public override bool IsRunning()
        {
            Debug.LogError("Injection Detector is not supported on selected platform!");
            return false;
        }
        #endif
    }
}