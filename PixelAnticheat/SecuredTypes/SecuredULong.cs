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
    /// Secured ULong Type
    /// </summary>
    [System.Serializable]
    public struct SecuredULong : IEquatable<SecuredULong>, IFormattable
    {
        private static ulong cryptoKey = 444443L;
        
#if UNITY_EDITOR
        public static ulong cryptoKeyEditor = cryptoKey;
#endif

        [SerializeField] private ulong currentCryptoKey;
        [SerializeField] private ulong hiddenValue;
        [SerializeField] private ulong fakeValue;
		[SerializeField] private bool inited;

		/// <summary>
		/// Secured UShort Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredULong(ulong value)
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
		public static void SetNewCryptoKey(ulong newKey)
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
		/// <returns>Encrypted <c>ulong</c>.</returns>
		public static ulong Encrypt(ulong value)
		{
			return Encrypt(value, 0);
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Decrypted <c>ulong</c>.</returns>
		public static ulong Decrypt(ulong value)
		{
			return Decrypt(value, 0);
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Encrypted <c>ulong</c>.</returns>
		public static ulong Encrypt(ulong value, ulong key)
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
		/// <returns>Decrypted <c>ulong</c>.</returns>
		public static ulong Decrypt(ulong value, ulong key)
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
		public ulong GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(ulong encrypted)
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
		private ulong InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			ulong key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			ulong decrypted = Decrypt(hiddenValue, key);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && fakeValue != 0 && decrypted != fakeValue)
			{
				AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return decrypted;
		}
		
		public static implicit operator SecuredULong(ulong value)
		{
			SecuredULong obscured = new SecuredULong(Encrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator ulong(SecuredULong value)
		{
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Increment Operator
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static SecuredULong operator ++(SecuredULong input)
		{
			ulong decrypted = input.InternalDecrypt() + 1L;
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
		public static SecuredULong operator --(SecuredULong input)
		{
			ulong decrypted = input.InternalDecrypt() - 1L;
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
			if (!(obj is SecuredULong))
				return false;
			
			SecuredULong o = (SecuredULong)obj;
			return (hiddenValue == o.hiddenValue);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified SecuredULong value.
		/// </summary>
		public bool Equals(SecuredULong obj)
		{
			return hiddenValue == obj.hiddenValue;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns></returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string representation, using the specified format.
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