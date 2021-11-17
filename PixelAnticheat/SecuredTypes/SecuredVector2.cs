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
    /// Secured Vector2 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredVector2
    {
        private static int cryptoKey = 120206;
		private static readonly Vector2 initialFakeValue = Vector2.zero;
		
#if UNITY_EDITOR
		public static int cryptoKeyEditor = cryptoKey;
#endif

		// Serialized Fields
		[SerializeField] private int currentCryptoKey;
		[SerializeField] private RawEncryptedVector2 hiddenValue;
		[SerializeField] private Vector2 fakeValue;
		[SerializeField] private bool inited;

		/// <summary>
		/// Secured Vector2 Constructor
		/// </summary>
		/// <param name="value"></param>
		private SecuredVector2(RawEncryptedVector2 value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = initialFakeValue;
			inited = true;
		}

		/// <summary>
		/// Vector2.x
		/// </summary>
		public float x
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.x);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.x) > detector.vector2Epsilon)
				{
					detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
				}
				return decrypted;
			}

			set
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				hiddenValue.x = InternalEncryptField(value);
				if (detector.IsRunning())
				{
					fakeValue.x = value;
				}
			}
		}

		/// <summary>
		/// Vector2.y
		/// </summary>
		public float y
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.y);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.y) > detector.vector2Epsilon)
				{
					detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
				}
				return decrypted;
			}

			set
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				hiddenValue.y = InternalEncryptField(value);
				if (detector.IsRunning())
				{
					fakeValue.y = value;
				}
			}
		}

		/// <summary>
		/// This
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;
					case 1:
						return y;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector2 index!");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector2 index!");
				}
			}
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
				hiddenValue = Encrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any Vector2 value, uses default crypto key.
		/// </summary>
		public static RawEncryptedVector2 Encrypt(Vector2 value)
		{
			return Encrypt(value, 0);
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any Vector2 value, uses passed crypto key.
		/// </summary>
		public static RawEncryptedVector2 Encrypt(Vector2 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedVector2 result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);

			return result;
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector2 you got from Encrypt(), uses default crypto key.
		/// </summary>
		public static Vector2 Decrypt(RawEncryptedVector2 value)
		{
			return Decrypt(value, 0);
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector2 you got from Encrypt(), uses passed crypto key.
		/// </summary>
		public static Vector2 Decrypt(RawEncryptedVector2 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Vector2 result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);

			return result;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public RawEncryptedVector2 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(RawEncryptedVector2 encrypted)
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
		private Vector2 InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue);
				fakeValue = initialFakeValue;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			Vector2 value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);

			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && !fakeValue.Equals(initialFakeValue) && !CompareVectorsWithTolerance(value, fakeValue))
			{
				AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return value;
		}

		/// <summary>
		/// Compare Vectors with Tolerance
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		private bool CompareVectorsWithTolerance(Vector2 vector1, Vector2 vector2)
		{
			MemoryHackDetector detector =
				(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			float epsilon = detector.vector2Epsilon;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
				   Math.Abs(vector1.y - vector2.y) < epsilon;
		}

		/// <summary>
		/// Internal Decrypt Field
		/// </summary>
		/// <param name="encrypted"></param>
		/// <returns></returns>
		private float InternalDecryptField(int encrypted)
		{
			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			float result = SecuredFloat.Decrypt(encrypted, key);
			return result;
		}

		/// <summary>
		/// Internal Encrypt Field
		/// </summary>
		/// <param name="encrypted"></param>
		/// <returns></returns>
		private int InternalEncryptField(float encrypted)
		{
			int result = SecuredFloat.Encrypt(encrypted, cryptoKey);
			return result;
		}
		
		public static implicit operator SecuredVector2(Vector2 value)
		{
			SecuredVector2 obscured = new SecuredVector2(Encrypt(value));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Vector2(SecuredVector2 value)
		{
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		/// <summary>
		/// Returns a nicely formatted string for this vector.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Returns a nicely formatted string for this vector.
		/// </summary>
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		/// <summary>
		/// Used to store encrypted Vector2.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector2
		{
			/// <summary>
			/// Encrypted value
			/// </summary>
			public int x;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int y;
		}
    }
}