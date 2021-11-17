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

using PixelAnticheat.Models;

namespace PixelAnticheat.SecuredTypes
{
    using System;
    using System.Text;
    using UnityEngine;
    using PixelAnticheat.Detectors;
    
    /// <summary>
    /// Secured String Type
    /// </summary>
    [System.Serializable]
    public sealed class SecuredString
    {
        private static string cryptoKey = "4441";
        
#if UNITY_EDITOR
        public static string cryptoKeyEditor = cryptoKey;
#endif
        
        // Serialized Params
        [SerializeField] private string currentCryptoKey;
        [SerializeField] private byte[] hiddenValue;
        [SerializeField] private string fakeValue;
        [SerializeField] private bool inited;

        
        // for serialization purposes
		private SecuredString(){}

		/// <summary>
		/// Secured String Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredString(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = null;
			inited = true;
		}

		/// <summary>
		/// Allows to change default crypto key of this type instances. All new instances will use specified key.<br/>
		/// All current instances will use previous key unless you call ApplyNewCryptoKey() on them explicitly.
		/// </summary>
		public static void SetNewCryptoKey(string newKey)
		{
			cryptoKey = newKey;
		}

		/// <summary>
		/// Use it after SetNewCryptoKey() to re-encrypt current instance using new crypto key.
		/// </summary>
		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = InternalEncrypt(InternalDecrypt());
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>string</c> (depending on what <c>string</c> was passed to the function)</returns>
		public static string EncryptDecrypt(string value)
		{
			return EncryptDecrypt(value, "");
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>string</c> (depending on what <c>string</c> was passed to the function)</returns>
		public static string EncryptDecrypt(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "";
			}

			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			int keyLength = key.Length;
			int valueLength = value.Length;

			char[] result = new char[valueLength];

			for (int i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public string GetEncrypted()
		{
			ApplyNewCryptoKey();
			return GetString(hiddenValue);
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(string encrypted)
		{
			inited = true;
			hiddenValue = GetBytes(encrypted);
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		/// <summary>
		/// Internal Encrypt
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static byte[] InternalEncrypt(string value)
		{
			return GetBytes(EncryptDecrypt(value, cryptoKey));
		}

		/// <summary>
		/// Internal Decrypt
		/// </summary>
		/// <returns></returns>
		private string InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt("");
				fakeValue = "";
				inited = true;
			}

			string key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			string result = EncryptDecrypt(GetString(hiddenValue), key);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && !string.IsNullOrEmpty(fakeValue) && result != fakeValue)
			{
				AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return result;
		}

		
		public static implicit operator SecuredString(string value)
		{
			if (value == null)
			{
				return null;
			}

			SecuredString obscured = new SecuredString(InternalEncrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator string(SecuredString value)
		{
			if (value == null)
			{
				return null;
			}
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Overrides default ToString to provide easy implicit conversion to the <c>string</c>.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt();
		}

		/// <summary>
		/// Equals Operator
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(SecuredString a, SecuredString b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			return ArraysEquals(a.hiddenValue, b.hiddenValue);
		}

		/// <summary>
		/// Not Equals Operator
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(SecuredString a, SecuredString b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Determines whether this instance of ObscuredString and a specified object, which must also be a SecuredString object, have the same value.
		/// </summary>
		public override bool Equals(object obj)
		{
			SecuredString strA = obj as SecuredString;
			string strB = null;
			if (strA != null) strB = GetString(strA.hiddenValue);

			return string.Equals(hiddenValue, strB);
		}

		/// <summary>
		/// Determines whether this instance and another specified SecuredString object have the same value.
		/// </summary>
		public bool Equals(SecuredString value)
		{
			byte[] a = null;
			if (value != null) a = value.hiddenValue;

			return ArraysEquals(hiddenValue, a);
		}

		/// <summary>
		/// Determines whether this string and a specified SecuredString object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.
		/// </summary>
		public bool Equals(SecuredString value, StringComparison comparisonType)
		{
			string strA = null;
			if (value != null) strA = value.InternalDecrypt();

			return string.Equals(InternalDecrypt(), strA, comparisonType);
		}

		/// <summary>
		/// Returns the hash code for this ObscuredString.
		/// </summary>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		static bool ArraysEquals(byte[] a1, byte[] a2)
		{
			if (a1 == a2)
			{
				return true;
			}

			if ((a1 != null) && (a2 != null))
			{
				if (a1.Length != a2.Length)
				{
					return false;
				}
				for (int i = 0; i < a1.Length; i++)
				{
					if (a1[i] != a2[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
    }
}