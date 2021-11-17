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
    /// Secured Vector4 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredVector4
    {
        private static int cryptoKey = 120207;
		private static readonly Vector4 initialFakeValue = Vector4.zero;
		
#if UNITY_EDITOR
		public static int cryptoKeyEditor = cryptoKey;
#endif
		
		// Serialized Fields
		[SerializeField] private int currentCryptoKey;
		[SerializeField] private RawEncryptedVector4 hiddenValue;
		[SerializeField] private Vector4 fakeValue;
		[SerializeField] private bool inited;

		/// <summary>
		/// Secured Vector4 Constructor
		/// </summary>
		/// <param name="encrypted"></param>
		private SecuredVector4(RawEncryptedVector4 encrypted)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = encrypted;
			fakeValue = initialFakeValue;
			inited = true;
		}

		/// <summary>
		/// Vector4.x
		/// </summary>
		public float x
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.x);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.x) > detector.vector3Epsilon)
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
		/// Vector4.y
		/// </summary>
		public float y
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.y);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.y) > detector.vector3Epsilon)
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
		/// Vector4.z
		/// </summary>
		public float z
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.z);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.z) > detector.vector3Epsilon)
				{
					detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
				}
				return decrypted;
			}

			set
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				hiddenValue.z = InternalEncryptField(value);
				if (detector.IsRunning())
				{
					fakeValue.z = value;
				}
			}
		}
		
		/// <summary>
		/// Vector4.w
		/// </summary>
		public float w
		{
			get
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				float decrypted = InternalDecryptField(hiddenValue.w);
				if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.w) > detector.vector4Epsilon)
				{
					detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
				}
				return decrypted;
			}

			set
			{
				MemoryHackDetector detector =
					(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
				hiddenValue.w = InternalEncryptField(value);
				if (detector.IsRunning())
				{
					fakeValue.w = value;
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
					case 2:
						return z;
					case 3:
						return w;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector3 index!");
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
					case 2:
						z = value;
						break;
					case 3:
						w = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector3 index!");
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
		/// Use this simple encryption method to encrypt any Vector4 value, uses default crypto key.
		/// </summary>
		public static RawEncryptedVector4 Encrypt(Vector4 value)
		{
			return Encrypt(value, 0);
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any Vector4 value, uses passed crypto key.
		/// </summary>
		public static RawEncryptedVector4 Encrypt(Vector4 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedVector4 result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);
			result.z = SecuredFloat.Encrypt(value.z, key);
			result.w = SecuredFloat.Encrypt(value.w, key);

			return result;
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector4 you got from Encrypt(), uses default crypto key.
		/// </summary>
		public static Vector4 Decrypt(RawEncryptedVector4 value)
		{
			return Decrypt(value, 0);
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector4 you got from Encrypt(), uses passed crypto key.
		/// </summary>
		public static Vector4 Decrypt(RawEncryptedVector4 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Vector4 result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);
			result.z = SecuredFloat.Decrypt(value.z, key);
			result.w = SecuredFloat.Decrypt(value.w, key);

			return result;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public RawEncryptedVector4 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(RawEncryptedVector4 encrypted)
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
		private Vector4 InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue, cryptoKey);
				fakeValue = initialFakeValue;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			Vector4 value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);
			value.z = SecuredFloat.Decrypt(hiddenValue.z, key);
			value.w = SecuredFloat.Decrypt(hiddenValue.w, key);

			MemoryHackDetector detector =
				(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			if (detector.IsRunning() && !fakeValue.Equals(Vector4.zero) && !CompareVectorsWithTolerance(value, fakeValue))
			{
				detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return value;
		}

		/// <summary>
		/// Compare Vectors with Tolerance
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		private bool CompareVectorsWithTolerance(Vector4 vector1, Vector4 vector2)
		{
			MemoryHackDetector detector =
				(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			float epsilon = detector.vector4Epsilon;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
				   Math.Abs(vector1.y - vector2.y) < epsilon &&
				   Math.Abs(vector1.z - vector2.z) < epsilon &&
				   Math.Abs(vector1.w - vector2.w) < epsilon;
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

		#region Operators
		public static implicit operator SecuredVector4(Vector4 value)
		{
			SecuredVector4 obscured = new SecuredVector4(Encrypt(value, cryptoKey));
			if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Vector4(SecuredVector4 value)
		{
			return value.InternalDecrypt();
		}
		public static SecuredVector4 operator +(SecuredVector4 a, SecuredVector4 b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}
		public static SecuredVector4 operator +(Vector4 a, SecuredVector4 b)
		{
			return a + b.InternalDecrypt();
		}
		public static SecuredVector4 operator +(SecuredVector4 a, Vector4 b)
		{
			return a.InternalDecrypt() + b;
		}
		public static SecuredVector4 operator -(SecuredVector4 a, SecuredVector4 b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}
		public static SecuredVector4 operator -(Vector4 a, SecuredVector4 b)
		{
			return a - b.InternalDecrypt();
		}
		public static SecuredVector4 operator -(SecuredVector4 a, Vector4 b)
		{
			return a.InternalDecrypt() - b;
		}
		public static SecuredVector4 operator -(SecuredVector4 a)
		{
			return -a.InternalDecrypt();
		}
		public static SecuredVector4 operator *(SecuredVector4 a, float d)
		{
			return a.InternalDecrypt() * d;
		}
		public static SecuredVector4 operator *(float d, SecuredVector4 a)
		{
			return d * a.InternalDecrypt();
		}
		public static SecuredVector4 operator /(SecuredVector4 a, float d)
		{
			return a.InternalDecrypt() / d;
		}

		public static bool operator ==(SecuredVector4 lhs, SecuredVector4 rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}
		public static bool operator ==(Vector4 lhs, SecuredVector4 rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}
		public static bool operator ==(SecuredVector4 lhs, Vector4 rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(SecuredVector4 lhs, SecuredVector4 rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}
		public static bool operator !=(Vector4 lhs, SecuredVector4 rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}
		public static bool operator !=(SecuredVector4 lhs, Vector4 rhs)
		{
			return lhs.InternalDecrypt() != rhs;
		}
		#endregion
		
		/// <summary>
		/// Check Equals
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return InternalDecrypt().Equals(other);
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
		/// Used to store encrypted Vector3.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector4
		{
			/// <summary>
			/// Encrypted value
			/// </summary>
			public int x;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int y;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int z;

			/// <summary>
			/// Encrypted Value
			/// </summary>
			public int w;
		}
    }
}