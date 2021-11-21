namespace PixelAnticheat.SecuredTypes
{
    using System;
    using UnityEngine;
    using PixelAnticheat.Detectors;
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Secured Color32 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredColor32
    {
        private static int cryptoKey = 120223;
        private static readonly Color32 initialFakeValue = new Color32(0,0,0,1);
        
        #if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
        #endif
        
        // Serialized Fields
        [SerializeField] private int currentCryptoKey;
        [SerializeField] private RawEncryptedColor32 hiddenValue;
        [SerializeField] private Color32 fakeValue;
        [SerializeField] private bool inited;
        
        /// <summary>
        /// Secured Color32 Constructor
        /// </summary>
        /// <param name="encrypted"></param>
        private SecuredColor32(RawEncryptedColor32 encrypted)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = encrypted;
            fakeValue = initialFakeValue;
            inited = true;
        }
        
        /// <summary>
        /// Color Red
        /// </summary>
        public byte r
        {
            get
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                byte decrypted = InternalDecryptField(hiddenValue.r);
                if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.r) > detector.vector3Epsilon)
                {
                    detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
                }
                return decrypted;
            }

            set
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                hiddenValue.r = InternalEncryptField(value);
                if (detector.IsRunning())
                {
                    fakeValue.r = value;
                }
            }
        }
        
        /// <summary>
        /// Color Green
        /// </summary>
        public byte g
        {
            get
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                byte decrypted = InternalDecryptField(hiddenValue.g);
                if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.g) > detector.vector3Epsilon)
                {
                    detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
                }
                return decrypted;
            }

            set
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                hiddenValue.g = InternalEncryptField(value);
                if (detector.IsRunning())
                {
                    fakeValue.g = value;
                }
            }
        }
        
        /// <summary>
        /// Color Blue
        /// </summary>
        public byte b
        {
            get
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                byte decrypted = InternalDecryptField(hiddenValue.b);
                if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.b) > detector.vector3Epsilon)
                {
                    detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
                }
                return decrypted;
            }

            set
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                hiddenValue.b = InternalEncryptField(value);
                if (detector.IsRunning())
                {
                    fakeValue.b = value;
                }
            }
        }
        
        /// <summary>
        /// Color Alpha
        /// </summary>
        public byte a
        {
            get
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                byte decrypted = InternalDecryptField(hiddenValue.a);
                if (detector.IsRunning() && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.a) > detector.vector3Epsilon)
                {
                    detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
                }
                return decrypted;
            }

            set
            {
                MemoryHackDetector detector =
                    (MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
                hiddenValue.a = InternalEncryptField(value);
                if (detector.IsRunning())
                {
                    fakeValue.a = value;
                }
            }
        }
        
        /// <summary>
        /// This
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return r;
                    case 1:
                        return g;
                    case 2:
                        return b;
                    case 3:
                        return a;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor32 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        r = value;
                        break;
                    case 1:
                        g = value;
                        break;
                    case 2:
                        b = value;
                        break;
                    case 3:
                        a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor32 index!");
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
		/// Use this simple encryption method to encrypt any Vector3 value, uses default crypto key.
		/// </summary>
		public static RawEncryptedColor32 Encrypt(Color32 value)
		{
			return Encrypt(value, 0);
		}

		/// <summary>
		/// Use this simple encryption method to encrypt any Vector3 value, uses passed crypto key.
		/// </summary>
		public static RawEncryptedColor32 Encrypt(Color32 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedColor32 result;
			result.r = SecuredByte.EncryptDecrypt((byte)value.r, (byte)key);
			result.g = SecuredByte.EncryptDecrypt((byte)value.g, (byte)key);
			result.b = SecuredByte.EncryptDecrypt((byte)value.b, (byte)key);
			result.a = SecuredByte.EncryptDecrypt((byte)value.a, (byte)key);

			return result;
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector3 you got from Encrypt(), uses default crypto key.
		/// </summary>
		public static Color32 Decrypt(RawEncryptedColor32 value)
		{
			return Decrypt(value, 0);
		}

		/// <summary>
		/// Use it to decrypt RawEncryptedVector3 you got from Encrypt(), uses passed crypto key.
		/// </summary>
		public static Color32 Decrypt(RawEncryptedColor32 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Color32 result = new Color32();
			result.r = SecuredByte.EncryptDecrypt((byte)value.r, (byte)key);
			result.g = SecuredByte.EncryptDecrypt((byte)value.g, (byte)key);
			result.b = SecuredByte.EncryptDecrypt((byte)value.b, (byte)key);
			result.a = SecuredByte.EncryptDecrypt((byte)value.a, (byte)key);

			return result;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		public RawEncryptedColor32 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		public void SetEncrypted(RawEncryptedColor32 encrypted)
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
		private Color32 InternalDecrypt()
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

			Color32 value = new Color32();

			value.r = SecuredByte.EncryptDecrypt((byte)hiddenValue.r, (byte)key);
			value.g = SecuredByte.EncryptDecrypt((byte)hiddenValue.g, (byte)key);
			value.b = SecuredByte.EncryptDecrypt((byte)hiddenValue.b, (byte)key);
			value.a = SecuredByte.EncryptDecrypt((byte)hiddenValue.a, (byte)key);

			MemoryHackDetector detector =
				(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			if (detector.IsRunning() && !fakeValue.Equals(Color.black) && !CompareVectorsWithTolerance(value, fakeValue))
			{
				detector.OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
			}

			return value;
		}

		/// <summary>
		/// Compare Vectors with Tolerance
		/// </summary>
		/// <param name="color1"></param>
		/// <param name="color2"></param>
		/// <returns></returns>
		private bool CompareVectorsWithTolerance(Color32 color1, Color32 color2)
		{
			MemoryHackDetector detector =
				(MemoryHackDetector) AntiCheat.Instance().GetDetector<MemoryHackDetector>();
			float epsilon = detector.color32Epsilon;
			return Math.Abs(color1.r - color2.r) < epsilon &&
				   Math.Abs(color1.g - color2.g) < epsilon &&
				   Math.Abs(color1.b - color2.b) < epsilon &&
				   Math.Abs(color1.a - color2.a) < epsilon;
		}

		/// <summary>
		/// Internal Decrypt Field
		/// </summary>
		/// <param name="encrypted"></param>
		/// <returns></returns>
		private byte InternalDecryptField(int encrypted)
		{
			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			byte result = SecuredByte.EncryptDecrypt((byte)encrypted, (byte)key);
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
		/// Returns a nicely formatted string for this color.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		/// <summary>
		/// Returns a nicely formatted string for this color.
		/// </summary>
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		/// <summary>
		/// Used to store encrypted Color.
		/// </summary>
		[Serializable]
		public struct RawEncryptedColor32
		{
			/// <summary>
			/// Encrypted value
			/// </summary>
			public int r;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int g;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int b;

			/// <summary>
			/// Encrypted Value
			/// </summary>
			public int a;
		}
    }
}