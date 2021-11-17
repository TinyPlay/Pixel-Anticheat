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
#if UNITY_EDITOR
namespace PixelAnticheat.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using PixelAnticheat.SecuredTypes;
    using System.Runtime.InteropServices;
    
    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredFloat))]
    public class SecuredFloatDrawer : SecuredPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			int currentCryptoKey = cryptoKey.intValue;

			IntBytesUnion union = new IntBytesUnion();
			float val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = SecuredFloat.cryptoKeyEditor;
				}
				hiddenValue.arraySize = 4;
				inited.boolValue = true;

				union.i = SecuredFloat.Encrypt(0, currentCryptoKey);

				hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
				hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
				hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
				hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
			}
			else
			{
				int arraySize = hiddenValue.arraySize;
				byte[] hiddenValueArray = new byte[arraySize];
				for (int i = 0; i < arraySize; i++)
				{
					hiddenValueArray[i] = (byte)hiddenValue.GetArrayElementAtIndex(i).intValue;
				}

				union.b1 = hiddenValueArray[0];
				union.b2 = hiddenValueArray[1];
				union.b3 = hiddenValueArray[2];
				union.b4 = hiddenValueArray[3];

				val = SecuredFloat.Decrypt(union.i, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.FloatField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				union.i = SecuredFloat.Encrypt(val, currentCryptoKey);

				hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
				hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
				hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
				hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
			}

			fakeValue.floatValue = val;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct IntBytesUnion
		{
			[FieldOffset(0)]
			public int i;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;
		}
    }
}
#endif