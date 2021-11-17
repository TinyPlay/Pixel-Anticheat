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
namespace PixelAnticheat.Editor.Windows
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Pixel Anti-Cheat Options
    /// </summary>
    internal class PixacOptions : EditorWindow
    {
        [UnityEditor.MenuItem(EditorGlobalStuff.WINDOWS_MENU_PATH + "Options")]
        private static void ShowWindow()
        {
            EditorWindow myself = GetWindow<PixacOptions>(false, "Pixel Anti-Cheat Options", true);
            myself.minSize = new Vector2(300, 100);
        }
        
        private void OnGUI()
        {
            // Welcome UI
            Rect welcome = (Rect) EditorGUILayout.BeginVertical();
            GUILayout.Label ("Welcome to the Pixel Anti-Cheat!", EditorStyles.largeLabel);
            GUILayout.Label("This library allows you to organize a simple anti-cheat for your game and take care of data security. You can use it in your projects for free. \n\nNote that it does not guarantee 100% protection for your game. If you are developing a multiplayer game - never trust the client and check everything on the server.", EditorStyles.helpBox);
            EditorGUILayout.Space();
            if (GUILayout.Button("Read Documentation"))
            {
                Application.OpenURL("https://github.com/TinyPlay/Pixel-Anticheat/wiki");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            GUILayout.Label("Injection Detector options", EditorStyles.largeLabel);

            bool enableInjectionDetector = false;

            enableInjectionDetector = EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL);
            enableInjectionDetector = GUILayout.Toggle(enableInjectionDetector, "Enable Injection Detector");
            EditorGUILayout.Space();
            if (GUILayout.Button("Edit Whitelist"))
            {
                AssembliesWhitelist.ShowWindow();
            }

            if (GUI.changed || EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION) != enableInjectionDetector)
            {
                EditorPrefs.SetBool(EditorGlobalStuff.PREFS_INJECTION, enableInjectionDetector);
                EditorPrefs.SetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL, enableInjectionDetector);
            }

            if (!enableInjectionDetector)
            {
                EditorGlobalStuff.CleanInjectionDetectorData();
            }
            else if (!File.Exists(EditorGlobalStuff.INJECTION_DATA_PATH))
            {
                Postprocessor.InjectionAssembliesScan();
            }
        }
    }
}