﻿/*
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
#if UNITY_EDITOR
namespace PixelAnticheat.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using PixelAnticheat.SecuredTypes;
    
    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredBool))]
    public class SecuredBoolDrawer : SecuredPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
            SetBoldIfValueOverridePrefab(prop, hiddenValue);

            SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
            SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
            SerializedProperty fakeValueChanged = prop.FindPropertyRelative("fakeValueChanged");
            SerializedProperty inited = prop.FindPropertyRelative("inited");

            int currentCryptoKey = cryptoKey.intValue;
            bool val = false;

            if (!inited.boolValue)
            {
                if (currentCryptoKey == 0)
                {
                    currentCryptoKey = cryptoKey.intValue = SecuredBool.cryptoKeyEditor;
                }
                inited.boolValue = true;
                hiddenValue.intValue = SecuredBool.Encrypt(val, (byte)currentCryptoKey);
            }
            else
            {
                val = SecuredBool.Decrypt(hiddenValue.intValue, (byte)currentCryptoKey);
            }

            EditorGUI.BeginChangeCheck();
            val = EditorGUI.Toggle(position, label, val);
            if (EditorGUI.EndChangeCheck())
                hiddenValue.intValue = SecuredBool.Encrypt(val, (byte)currentCryptoKey);

            fakeValue.boolValue = val;
            fakeValueChanged.boolValue = true;
        }
    }
}
#endif