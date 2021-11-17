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
namespace PixelAnticheat.Editor.Editors
{
    using PixelAnticheat.Detectors;
    using UnityEditor;
    using UnityEngine;
    
    [CustomEditor(typeof(InjectionDetector))]
    public class InjectionDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUIStyle textStyle = new GUIStyle();
            textStyle.normal.textColor = GUI.skin.label.normal.textColor;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(new GUIContent("Don't forget to start detection (check readme)!", "You should start detector from code using AntiCheat.Instance() methods. See example scripts to learn more."), textStyle);

            if (!EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION_GLOBAL))
            {
                textStyle.normal.textColor = new Color32(220, 64, 64, 255);
                textStyle.fontStyle = FontStyle.Bold;

                EditorGUILayout.LabelField("Injection Detector is not enabled in Pixel Anti-Cheat options (check readme)!", textStyle);
            }
            else if (!EditorPrefs.GetBool(EditorGlobalStuff.PREFS_INJECTION))
            {
                textStyle.normal.textColor = new Color32(220, 64, 64, 255);
                textStyle.fontStyle = FontStyle.Bold;

                EditorGUILayout.LabelField("Injection Detector disabled on current platform!", textStyle);
            }
        }
    }
}