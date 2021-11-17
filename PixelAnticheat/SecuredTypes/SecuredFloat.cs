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
    using UnityEngine;
    using System.Runtime.InteropServices;
    using PixelAnticheat.Detectors;
    
    /// <summary>
    /// Secured Float Type
    /// </summary>
    [System.Serializable]
    public struct SecuredFloat : IEquatable<SecuredFloat>, IFormattable
    {
        private static int cryptoKey = 230887;
        
#if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
#endif

        [SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private float fakeValue;

		[SerializeField]
		private bool inited;

		/// <summary>
		/// Secured Float Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredFloat(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}

		/// <summary>
		/// Allows to change default crypto key of this type instances. All new instances will use specified key.<br/>
		/// All current instances will use previous key unless you call ApplyNewCryptoKey() on them explicitly.
		/// </summary>
		public static void SetNewCryptoKey(int newKey)
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
				hiddenValue = InternalEncrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any float value, uses default crypto key.
		/// </summary>
		public static int Encrypt(float value)
		{
			return Encrypt(value, cryptoKey);
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any float value, uses passed crypto key.
		/// </summary>
		public static int Encrypt(float value, int key)
		{
			var u = new FloatIntBytesUnion();
			u.f = value;
			u.i = u.i ^ key;

			return u.i;
		}

		/// <summary>
		/// Internal Encrypt
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static byte[] InternalEncrypt(float value)
		{
			return InternalEncrypt(value, 0);
		}

		/// <summary>
		/// Internal Encrypt by Key
		/// </summary>
		/// <param name="value"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private static byte[] InternalEncrypt(float value, int key)
		{
			int currKey = key;
			if (currKey == 0)
			{
				currKey = cryptoKey;
			}

			var u = new FloatIntBytesUnion();
			u.f = value;
			u.i = u.i ^ currKey;

			return new[] { u.b1, u.b2, u.b3, u.b4 };
		}

		/// <summary>
		/// Use it to decrypt int you got
		/// from Encrypt(float) back to float, uses default crypto key.
		/// </summary>
		public static float Decrypt(int value)
		{
			return Decrypt(value, cryptoKey);
		}

		/// <summary>
		/// Use it to decrypt int you got
		/// from Encrypt(float) back to float, uses passed crypto key.
		/// </summary>
		public static float Decrypt(int value, int key)
		{
			var u = new FloatIntBytesUnion();
			u.i = value ^ key;
			return u.f;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public int GetEncrypted()
		{
			ApplyNewCryptoKey();

			var union = new FloatIntBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];

			return union.i;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(int encrypted)
		{
			inited = true;
			FloatIntBytesUnion union = new FloatIntBytesUnion();
			union.i = encrypted;

			hiddenValue = new[] { union.b1, union.b2, union.b3, union.b4 };

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		/// <summary>
		/// Internal Decrypt
		/// </summary>
		/// <returns></returns>
		private float InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0);
				fakeValue = 0;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			var union = new FloatIntBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];

			union.i = union.i ^ key;

			float decrypted = union.f;

			MemoryHackDetector detector = (MemoryHackDetector)AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			if (detector.IsRunning() && fakeValue != 0 && Math.Abs(decrypted - fakeValue) > detector.floatEpsilon)
			{
				detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return decrypted;
		}

		/// <summary>
		/// Float Int Bytes
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			public float f;

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
		
		public static implicit operator SecuredFloat(float value)
		{
			SecuredFloat obscured = new SecuredFloat(InternalEncrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator float(SecuredFloat value)
		{
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Increment Operator
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static SecuredFloat operator ++(SecuredFloat input)
		{
			float decrypted = input.InternalDecrypt() + 1f;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				input.fakeValue = decrypted;
			}

			return input;
		}

		/// <summary>
		/// Decrement Operator
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static SecuredFloat operator --(SecuredFloat input)
		{
			float decrypted = input.InternalDecrypt() - 1f;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				input.fakeValue = decrypted;
			}

			return input;
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredFloat))
				return false;
			SecuredFloat d = (SecuredFloat)obj;
			float dParam = d.InternalDecrypt();
			float dThis = InternalDecrypt();
			if ((double)dParam == (double)dThis)
				return true;
			return float.IsNaN(dParam) && float.IsNaN(dThis);
		}

		/// <summary>
		/// Returns a value indicating whether this instance and a specified SecuredFloat object represent the same value.
		/// </summary>
		public bool Equals(SecuredFloat obj)
		{
			float dParam = obj.InternalDecrypt();
			float dThis = InternalDecrypt();


			if ((double)dParam == (double)dThis)
				return true;
			return float.IsNaN(dParam) && float.IsNaN(dThis);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		/// <summary>
		/// Converts the numeric value of this
		/// instance to its equivalent string representation.
		/// </summary>
		/// <returns>
		/// The string representation of the value of this instance.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent
		/// string representation, using the specified format.
		/// </summary>
		/// <returns>
		/// The string representation of the value of
		/// this instance as specified by <paramref name="format"/>.
		/// </returns>
		/// <param name="format">A numeric format string (see Remarks).</param><exception cref="T:System.FormatException"><paramref name="format"/> is invalid. </exception><filterpriority>1</filterpriority>
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent
		/// string representation using the specified culture-specific
		/// format information.
		/// </summary>
		/// <returns>
		/// The string representation of the value of
		/// this instance as specified by <paramref name="provider"/>.
		/// </returns>
		/// <param name="provider">An <see cref="T:System.IFormatProvider"/> that supplies culture-specific formatting information. </param><filterpriority>1</filterpriority>
		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent
		/// string representation using the specified format and
		/// culture-specific format information.
		/// </summary>
		/// <returns>
		/// The string representation of the value of this instance
		/// as specified by <paramref name="format"/> and <paramref name="provider"/>.
		/// </returns>
		/// <param name="format">A numeric format string (see Remarks).</param><param name="provider">An <see cref="T:System.IFormatProvider"/> that supplies culture-specific formatting information. </param><filterpriority>1</filterpriority>
		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}
    }
}