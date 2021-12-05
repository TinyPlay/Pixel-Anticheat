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
    /// Secured Long Type
    /// </summary>
    [System.Serializable]
    public struct SecuredLong : IEquatable<SecuredLong>, IFormattable
    {
        private static long cryptoKey = 444442L;
        
#if UNITY_EDITOR
        public static long cryptoKeyEditor = cryptoKey;
#endif
        
	    [SerializeField] private long currentCryptoKey;
		[SerializeField] private long hiddenValue;
		[SerializeField] private long fakeValue;
		[SerializeField] private bool inited;

		/// <summary>
		/// Secured Long Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredLong(long value)
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
		public static void SetNewCryptoKey(long newKey)
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
				hiddenValue = Encrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Encrypted <c>long</c>.</returns>
		public static long Encrypt(long value)
		{
			return Encrypt(value, 0);
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Decrypted <c>long</c>.</returns>
		public static long Decrypt(long value)
		{
			return Decrypt(value, 0);
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Encrypted <c>long</c>.</returns>
		public static long Encrypt(long value, long key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Decrypted <c>long</c>.</returns>
		public static long Decrypt(long value, long key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public long GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(long encrypted)
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
		private long InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			long key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			long decrypted = Decrypt(hiddenValue, key);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && fakeValue != 0 && decrypted != fakeValue)
			{
				AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return decrypted;
		}
		
		
		public static implicit operator SecuredLong(long value)
		{
			SecuredLong obscured = new SecuredLong(Encrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator long(SecuredLong value)
		{
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Increment Operator
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static SecuredLong operator ++(SecuredLong input)
		{
			long decrypted = input.InternalDecrypt() + 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

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
		public static SecuredLong operator --(SecuredLong input)
		{
			long decrypted = input.InternalDecrypt() - 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

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
			if (!(obj is SecuredLong))
				return false;
			
			SecuredLong o = (SecuredLong)obj;
			return (hiddenValue == o.hiddenValue);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified SecuredLong value.
		/// </summary>
		public bool Equals(SecuredLong obj)
		{
			return hiddenValue == obj.hiddenValue;
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
		/// Converts the numeric value of this instance to
		/// its equivalent string representation.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Converts the numeric value of this instance to
		/// its equivalent string representation, using the specified format.
		/// </summary>
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
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