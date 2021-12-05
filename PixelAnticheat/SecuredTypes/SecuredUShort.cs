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
    using PixelAnticheat.Detectors;
    
    /// <summary>
    /// Secured UShort Type
    /// </summary>
    [System.Serializable]
    public struct SecuredUShort : IEquatable<SecuredUShort>, IFormattable
    {
        private static ushort cryptoKey = 224;
        
#if UNITY_EDITOR
        public static ushort cryptoKeyEditor = cryptoKey;
#endif

	    [SerializeField] private ushort currentCryptoKey;
	    [SerializeField] private ushort hiddenValue;
	    [SerializeField] private ushort fakeValue;
	    [SerializeField] private bool inited;

		/// <summary>
		/// Secured UShort Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredUShort(ushort value)
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
		public static void SetNewCryptoKey(ushort newKey)
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
				hiddenValue = EncryptDecrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>ushort</c> (depending on what <c>ushort</c> was passed to the function)</returns>
		public static ushort EncryptDecrypt(ushort value)
		{
			return EncryptDecrypt(value, 0);
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>ushort</c> (depending on what <c>ushort</c> was passed to the function)</returns>
		public static ushort EncryptDecrypt(ushort value, ushort key)
		{
			if (key == 0)
			{
				return (ushort)(value ^ cryptoKey);
			}
			return (ushort)(value ^ key);
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public ushort GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(ushort encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				fakeValue = InternalDecrypt();
			}
		}

		/// <summary>
		/// Internal Decrypt
		/// </summary>
		/// <returns></returns>
		private ushort InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}

			ushort key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			ushort decrypted = EncryptDecrypt(hiddenValue, key);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && fakeValue != 0 && decrypted != fakeValue)
			{
				AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return decrypted;
		}
		
		public static implicit operator SecuredUShort(ushort value)
		{
			SecuredUShort obscured = new SecuredUShort(EncryptDecrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator ushort(SecuredUShort value)
		{
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Increment Operator
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static SecuredUShort operator ++(SecuredUShort input)
		{
			ushort decrypted = (ushort)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

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
		public static SecuredUShort operator --(SecuredUShort input)
		{
			ushort decrypted = (ushort)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

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
			if (!(obj is SecuredUShort))
				return false;
			
			SecuredUShort ob = (SecuredUShort)obj;
			return hiddenValue == ob.hiddenValue;
		}

		/// <summary>
		/// Returns a value indicating whether this instance and a specified SecuredUShort object represent the same value.
		/// </summary>
		public bool Equals(SecuredUShort obj)
		{
			return hiddenValue == obj.hiddenValue;
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation using the specified format.
		/// </summary>
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.
		/// </summary>
		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.
		/// </summary>
		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}
    }
}