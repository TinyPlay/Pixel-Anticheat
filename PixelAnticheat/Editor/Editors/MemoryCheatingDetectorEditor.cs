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
    
    [CustomEditor(typeof(MemoryHackDetector))]
    public class MemoryCheatingDetectorEditor : Editor
    {
        private SerializedProperty floatEpsilon;
        private SerializedProperty vector2Epsilon;
        private SerializedProperty vector3Epsilon;
        private SerializedProperty vector4Epsilon;
        private SerializedProperty quaternionEpsilon;
        private SerializedProperty colorEpsilon;
        private SerializedProperty color32Epsilon;
        
        public void OnEnable()
        {
            floatEpsilon = serializedObject.FindProperty("floatEpsilon");
            vector2Epsilon = serializedObject.FindProperty("vector2Epsilon");
            vector3Epsilon = serializedObject.FindProperty("vector3Epsilon");
            vector4Epsilon = serializedObject.FindProperty("vector4Epsilon");
            quaternionEpsilon = serializedObject.FindProperty("quaternionEpsilon");
            colorEpsilon = serializedObject.FindProperty("colorEpsilon");
            color32Epsilon = serializedObject.FindProperty("color32Epsilon");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.PropertyField(floatEpsilon, new GUIContent("Float Epsilon", "Max allowed difference between encrypted and fake values in SecuredFloat. Increase in case of false positives."));
            EditorGUILayout.PropertyField(vector2Epsilon, new GUIContent("Vector2 Epsilon", "Max allowed difference between encrypted and fake values in SecuredVector2. Increase in case of false positives."));
            EditorGUILayout.PropertyField(vector3Epsilon, new GUIContent("Vector3 Epsilon", "Max allowed difference between encrypted and fake values in SecuredVector3. Increase in case of false positives."));
            EditorGUILayout.PropertyField(vector4Epsilon, new GUIContent("Vector4 Epsilon", "Max allowed difference between encrypted and fake values in SecuredVector4. Increase in case of false positives."));
            EditorGUILayout.PropertyField(quaternionEpsilon, new GUIContent("Quaternion Epsilon", "Max allowed difference between encrypted and fake values in ObscuredQuaternion. Increase in case of false positives."));
            EditorGUILayout.PropertyField(colorEpsilon, new GUIContent("Color Epsilon", "Max allowed difference between encrypted and fake values in SecuredColor. Increase in case of false positives."));
            EditorGUILayout.PropertyField(colorEpsilon, new GUIContent("Color32 Epsilon", "Max allowed difference between encrypted and fake values in SecuredColor32. Increase in case of false positives."));
            
            GUIStyle textStyle = new GUIStyle();
            textStyle.normal.textColor = GUI.skin.label.normal.textColor;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            EditorGUILayout.LabelField(new GUIContent("Don't forget to start detection (check readme)!", "You should start detector from code using AntiCheat.Instance() methods. See example scripts to learn more."), textStyle);

            serializedObject.ApplyModifiedProperties();
        }
    }
}