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
namespace PixelAnticheat.Data
{
    using System;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Events;
    using PixelAnticheat.Detectors;
    using PixelAnticheat.SecuredTypes;
    using PixelAnticheat.Encryption;
    
    /// <summary>
    /// Secured Prefs Class
    /// This is an Analog for Player Prefs
    /// </summary>
    public static class SecuredPrefs
    {
        // Secured Prefs Constants
        private const byte VERSION = 2;
        private const string RAW_NOT_FOUND = "{not_found}";
        private const string DATA_SEPARATOR = "|";
        
        // Private Params
        private static string encryptionKey = "e806f6";
        private static bool foreignSavesReported;
        private static string deviceID;
        
        /// <summary>
		/// Allows to get current device ID or set custom device ID to lock saves to the device.
		/// </summary>
        public static string DeviceID
		{
			get
			{
				if (String.IsNullOrEmpty(deviceID))
				{
					deviceID = GetDeviceID();
				}
				return deviceID;
			}
			set
			{
				deviceID = value;
				deviceIDHash = CalculateChecksum(deviceID);
			}
		}
        
        // Device ID Hash
        private static uint deviceIDHash;
		private static uint DeviceIDHash
		{
			get
			{
				if (deviceIDHash == 0)
				{
					deviceIDHash = CalculateChecksum(DeviceID);
				}
				return deviceIDHash;
			}
		}

		/// <summary>
		/// Allows reacting on saves alteration. May be helpful for banning potential cheaters.
		/// </summary>
		public static UnityEvent OnCheatingDetecting = new UnityEvent();

		/// <summary>
		/// Allows saving original PlayerPrefs values while migrating to SecuredPrefs.
		/// </summary>
		public static bool preservePlayerPrefs = false;

		/// <summary>
		/// Allows reacting on detection of possible saves from some other device. 
		/// </summary>
		public static UnityEvent OnOtherDeviceDetected = new UnityEvent();

		/// <summary>
		/// Allows locking saved data to the current device.
		/// </summary>
		public static DeviceLockLevel lockToDevice = DeviceLockLevel.None;

		/// <summary>
		/// Allows reading saves locked to other device. #onPossibleForeignSavesDetected action still will be fired.
		/// </summary>
		public static bool readForeignSaves = false;

		/// <summary>
		/// Allows ignoring #lockToDevice to recover saved data in case of some unexpected issues, like unique device ID change for the same device.<br/>
		/// Similar to readForeignSaves, but doesn't fires #onPossibleForeignSavesDetected action on foreign saves detection.
		/// </summary>
		public static bool emergencyMode = false;

		/// <summary>
		/// Allows forcing device id obtaining on demand. Otherwise, it will be obtained automatically on first usage.
		/// </summary>
		public static void ForceLockToDeviceInit()
		{
			if (String.IsNullOrEmpty(deviceID))
			{
				deviceID = GetDeviceID();
				deviceIDHash = CalculateChecksum(deviceID);
			}
		}

		/// <summary>
		/// Allows changing default crypto key.
		/// </summary>
		public static void SetNewCryptoKey(string newKey)
		{
			encryptionKey = newKey;
			deviceIDHash = CalculateChecksum(deviceID);
		}

		#region int
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetInt(string key, int value)
		{
			#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptIntValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
			#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptIntValue(key, value));
			#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return 0.
		/// </summary>
		public static int GetInt(string key)
		{
			return GetInt(key,0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static int GetInt(string key, int defaultValue)
		{
			string encryptedKey = EncryptKey(key);
			
			if (!PlayerPrefs.HasKey(encryptedKey))
			{
				if (PlayerPrefs.HasKey(key))
				{
					int unencrypted = PlayerPrefs.GetInt(key, defaultValue);
					if (!preservePlayerPrefs)
					{
						SetInt(key, unencrypted);
						PlayerPrefs.DeleteKey(key);
					}
					return unencrypted;
				}
			}
			
			string encrypted = GetEncryptedPrefsString(key, encryptedKey);
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptIntValue(key, encrypted, defaultValue);
		}

		/// <summary>
		/// Encrypted Int Value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string EncryptIntValue(string key, int value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Int);
		}

		/// <summary>
		/// Decrypted Int Value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="encryptedInput"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private static int DecryptIntValue(string key, string encryptedInput, int defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			int cleanValue = BitConverter.ToInt32(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region uint
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetUInt(string key, uint value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptUIntValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptUIntValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return 0.
		/// </summary>
		public static uint GetUInt(string key)
		{
			return GetUInt(key, 0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static uint GetUInt(string key, uint defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptUIntValue(key, encrypted, defaultValue);
		}

		private static string EncryptUIntValue(string key, uint value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.UInt);
		}

		private static uint DecryptUIntValue(string key, string encryptedInput, uint defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			uint cleanValue = BitConverter.ToUInt32(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region string
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetString(string key, string value)
		{

#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptStringValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptStringValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return "".
		/// </summary>
		public static string GetString(string key)
		{
			return GetString(key, "");
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static string GetString(string key, string defaultValue)
		{
			string encryptedKey = EncryptKey(key);

			if (!PlayerPrefs.HasKey(encryptedKey))
			{
				if (PlayerPrefs.HasKey(key))
				{
					string unencrypted = PlayerPrefs.GetString(key, defaultValue);
					if (!preservePlayerPrefs)
					{
						SetString(key, unencrypted);
						PlayerPrefs.DeleteKey(key);
					}
					return unencrypted;
				}
			}
			
			string encrypted = GetEncryptedPrefsString(key, encryptedKey);
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptStringValue(key, encrypted, defaultValue);
		}

		private static string EncryptStringValue(string key, string value)
		{
			byte[] cleanBytes = Encoding.UTF8.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.String);
		}

		private static string DecryptStringValue(string key, string encryptedInput, string defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			string cleanValue = Encoding.UTF8.GetString(cleanBytes, 0, cleanBytes.Length);
			return cleanValue;
		}
		#endregion

		#region float
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetFloat(string key, float value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptFloatValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptFloatValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return 0.
		/// </summary>
		public static float GetFloat(string key)
		{
			return GetFloat(key, 0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static float GetFloat(string key, float defaultValue)
		{
			string encryptedKey = EncryptKey(key);

			if (!PlayerPrefs.HasKey(encryptedKey))
			{
				if (PlayerPrefs.HasKey(key))
				{
					float unencrypted = PlayerPrefs.GetFloat(key, defaultValue);
					if (!preservePlayerPrefs)
					{
						SetFloat(key, unencrypted);
						PlayerPrefs.DeleteKey(key);
					}
					return unencrypted;
				}
			}
			
			string encrypted = GetEncryptedPrefsString(key, encryptedKey);
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptFloatValue(key, encrypted, defaultValue);
		}

		private static string EncryptFloatValue(string key, float value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Float);
		}

		private static float DecryptFloatValue(string key, string encryptedInput, float defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			float cleanValue = BitConverter.ToSingle(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region double
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetDouble(string key, double value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptDoubleValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptDoubleValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return 0.
		/// </summary>
		public static double GetDouble(string key)
		{
			return GetDouble(key, 0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static double GetDouble(string key, double defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptDoubleValue(key, encrypted, defaultValue);
		}

		private static string EncryptDoubleValue(string key, double value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Double);
		}

		private static double DecryptDoubleValue(string key, string encryptedInput, double defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			double cleanValue = BitConverter.ToDouble(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region long
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetLong(string key, long value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptLongValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptLongValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return 0.
		/// </summary>
		public static long GetLong(string key)
		{
			return GetLong(key, 0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static long GetLong(string key, long defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptLongValue(key, encrypted, defaultValue);
		}

		private static string EncryptLongValue(string key, long value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Long);
		}

		private static long DecryptLongValue(string key, string encryptedInput, long defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			long cleanValue = BitConverter.ToInt64(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region bool
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetBool(string key, bool value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptBoolValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptBoolValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return false.
		/// </summary>
		public static bool GetBool(string key)
		{
			return GetBool(key, false);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static bool GetBool(string key, bool defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptBoolValue(key, encrypted, defaultValue);
		}

		private static string EncryptBoolValue(string key, bool value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Bool);
		}

		private static bool DecryptBoolValue(string key, string encryptedInput, bool defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			bool cleanValue = BitConverter.ToBoolean(cleanBytes, 0);
			return cleanValue;
		}
		#endregion

		#region byte[]
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetByteArray(string key, byte[] value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptByteArrayValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptByteArrayValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return new byte[0].
		/// </summary>
		public static byte[] GetByteArray(string key)
		{
			return GetByteArray(key, 0, 0);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>byte[defaultLength]</c> filled with <c>defaultValue</c>.
		/// </summary>
		public static byte[] GetByteArray(string key, byte defaultValue, int defaultLength)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));

			if (encrypted == RAW_NOT_FOUND)
			{
				return ConstructByteArray(defaultValue, defaultLength);
			}

			return DecryptByteArrayValue(key, encrypted, defaultValue, defaultLength);
		}

		private static string EncryptByteArrayValue(string key, byte[] value)
		{
			return EncryptData(key, value, DataType.ByteArray);
		}

		private static byte[] DecryptByteArrayValue(string key, string encryptedInput, byte defaultValue, int defaultLength)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return ConstructByteArray(defaultValue, defaultLength);
			}

			return cleanBytes;
		}

		private static byte[] ConstructByteArray(byte value, int length)
		{
			byte[] bytes = new byte[length];
			for (int i = 0; i < length; i++)
			{
				bytes[i] = value;
			}
			return bytes;
		}
		#endregion

		#region Vector2
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetVector2(string key, Vector2 value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptVector2Value(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptVector2Value(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return Vector2.zero.
		/// </summary>
		public static Vector2 GetVector2(string key)
		{
			return GetVector2(key, Vector2.zero);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Vector2 GetVector2(string key, Vector2 defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptVector2Value(key, encrypted, defaultValue);
		}

		private static string EncryptVector2Value(string key, Vector2 value)
		{
			byte[] cleanBytes = new byte[8];
			Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, cleanBytes, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, cleanBytes, 4, 4);
			return EncryptData(key, cleanBytes, DataType.Vector2);
		}

		private static Vector2 DecryptVector2Value(string key, string encryptedInput, Vector2 defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			Vector2 cleanValue;
			cleanValue.x = BitConverter.ToSingle(cleanBytes, 0);
			cleanValue.y = BitConverter.ToSingle(cleanBytes, 4);
			return cleanValue;
		}
		#endregion

		#region Vector3
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetVector3(string key, Vector3 value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptVector3Value(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptVector3Value(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return Vector3.zero.
		/// </summary>
		public static Vector3 GetVector3(string key)
		{
			return GetVector3(key, Vector3.zero);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Vector3 GetVector3(string key, Vector3 defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptVector3Value(key, encrypted, defaultValue);
		}

		private static string EncryptVector3Value(string key, Vector3 value)
		{
			byte[] cleanBytes = new byte[12];
			Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, cleanBytes, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, cleanBytes, 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, cleanBytes, 8, 4);
			return EncryptData(key, cleanBytes, DataType.Vector3);
		}

		private static Vector3 DecryptVector3Value(string key, string encryptedInput, Vector3 defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			Vector3 cleanValue;
			cleanValue.x = BitConverter.ToSingle(cleanBytes, 0);
			cleanValue.y = BitConverter.ToSingle(cleanBytes, 4);
			cleanValue.z = BitConverter.ToSingle(cleanBytes, 8);
			return cleanValue;
		}
		#endregion

		#region Quaternion
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetQuaternion(string key, Quaternion value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptQuaternionValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptQuaternionValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return Quaternion.identity.
		/// </summary>
		public static Quaternion GetQuaternion(string key)
		{
			return GetQuaternion(key, Quaternion.identity);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Quaternion GetQuaternion(string key, Quaternion defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptQuaternionValue(key, encrypted, defaultValue);
		}

		private static string EncryptQuaternionValue(string key, Quaternion value)
		{
			byte[] cleanBytes = new byte[16];
			Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, cleanBytes, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, cleanBytes, 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, cleanBytes, 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.w), 0, cleanBytes, 12, 4);
			return EncryptData(key, cleanBytes, DataType.Quaternion);
		}

		private static Quaternion DecryptQuaternionValue(string key, string encryptedInput, Quaternion defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			Quaternion cleanValue;
			cleanValue.x = BitConverter.ToSingle(cleanBytes, 0);
			cleanValue.y = BitConverter.ToSingle(cleanBytes, 4);
			cleanValue.z = BitConverter.ToSingle(cleanBytes, 8);
			cleanValue.w = BitConverter.ToSingle(cleanBytes, 12);
			return cleanValue;
		}
		#endregion

		#region Color
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetColor(string key, Color32 value)
		{
			uint encodedColor = (uint)((value.a << 24) | (value.r << 16) | (value.g << 8) | value.b);

#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptUIntValue(key, encodedColor));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptColorValue(key, encodedColor));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return Color.black.
		/// </summary>
		public static Color32 GetColor(string key)
		{
			return GetColor(key, new Color32(0,0,0,1));
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Color32 GetColor(string key, Color32 defaultValue)
		{
			// 16777216u == Color32(0,0,0,1);
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			if (encrypted == RAW_NOT_FOUND)
			{
				return defaultValue;
			}

			uint encodedColor = DecryptUIntValue(key, encrypted, 16777216u);
			byte a = (byte)(encodedColor >> 24);
			byte r = (byte)(encodedColor >> 16);
			byte g = (byte)(encodedColor >> 8);
			byte b = (byte)(encodedColor >> 0);
			return new Color32(r, g, b, a);
		}

		private static string EncryptColorValue(string key, uint value)
		{
			byte[] cleanBytes = BitConverter.GetBytes(value);
			return EncryptData(key, cleanBytes, DataType.Color);
		}
		#endregion

		#region Rect
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetRect(string key, Rect value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptRectValue(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptRectValue(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return (0,0,0,0) rect.
		/// </summary>
		public static Rect GetRect(string key)
		{
			return GetRect(key, new Rect(0,0,0,0));
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Rect GetRect(string key, Rect defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptRectValue(key, encrypted, defaultValue);
		}

		private static string EncryptRectValue(string key, Rect value)
		{
			byte[] cleanBytes = new byte[16];
			Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, cleanBytes, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, cleanBytes, 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.width), 0, cleanBytes, 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.height), 0, cleanBytes, 12, 4);
			return EncryptData(key, cleanBytes, DataType.Rect);
		}

		private static Rect DecryptRectValue(string key, string encryptedInput, Rect defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			Rect cleanValue = new Rect();
			cleanValue.x = BitConverter.ToSingle(cleanBytes, 0);
			cleanValue.y = BitConverter.ToSingle(cleanBytes, 4);
			cleanValue.width = BitConverter.ToSingle(cleanBytes, 8);
			cleanValue.height = BitConverter.ToSingle(cleanBytes, 12);
			return cleanValue;
		}
		#endregion
		
		#region Vector4
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		public static void SetVector4(string key, Vector4 value)
		{
#if UNITY_WEBPLAYER
			try
			{
				PlayerPrefs.SetString(EncryptKey(key), EncryptVector4Value(key, value));
			}
			catch (PlayerPrefsException exception)
			{
				Debug.LogException(exception);
			}
#else
			PlayerPrefs.SetString(EncryptKey(key), EncryptVector4Value(key, value));
#endif
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return Vector4.zero.
		/// </summary>
		public static Vector4 GetVector4(string key)
		{
			return GetVector4(key, Vector4.zero);
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		public static Vector4 GetVector4(string key, Vector4 defaultValue)
		{
			string encrypted = GetEncryptedPrefsString(key, EncryptKey(key));
			return encrypted == RAW_NOT_FOUND ? defaultValue : DecryptVector4Value(key, encrypted, defaultValue);
		}

		private static string EncryptVector4Value(string key, Vector4 value)
		{
			byte[] cleanBytes = new byte[16];
			Buffer.BlockCopy(BitConverter.GetBytes(value.x), 0, cleanBytes, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.y), 0, cleanBytes, 4, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.z), 0, cleanBytes, 8, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(value.w), 0, cleanBytes, 12, 4);
			return EncryptData(key, cleanBytes, DataType.Vector4);
		}

		private static Vector4 DecryptVector4Value(string key, string encryptedInput, Vector4 defaultValue)
		{
			byte[] cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
			{
				return defaultValue;
			}

			Vector4 cleanValue;
			cleanValue.x = BitConverter.ToSingle(cleanBytes, 0);
			cleanValue.y = BitConverter.ToSingle(cleanBytes, 4);
			cleanValue.z = BitConverter.ToSingle(cleanBytes, 8);
			cleanValue.w = BitConverter.ToSingle(cleanBytes, 12);
			return cleanValue;
		}
		#endregion

		/// <summary>
		/// Returns true if <c>key</c> exists in the SecuredPrefs or in regular PlayerPrefs.
		/// </summary>
		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key) || PlayerPrefs.HasKey(EncryptKey(key));
		}

		/// <summary>
		/// Removes <c>key</c> and its corresponding value from the SecuredPrefs and regular PlayerPrefs.
		/// </summary>
		public static void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(EncryptKey(key));
			PlayerPrefs.DeleteKey(key);
		}

		/// <summary>
		/// Removes all keys and values from the preferences, including anything saved with regular PlayerPrefs. Use with caution!
		/// </summary>
		public static void DeleteAll()
		{
			PlayerPrefs.DeleteAll();
		}

		/// <summary>
		/// Writes all modified preferences to disk.
		/// </summary>
		public static void Save()
		{
			PlayerPrefs.Save();
		}

		private static string GetEncryptedPrefsString(string key, string encryptedKey)
		{
			string result = PlayerPrefs.GetString(encryptedKey, RAW_NOT_FOUND);

			if (result == RAW_NOT_FOUND)
			{
				if (PlayerPrefs.HasKey(key))
				{
				}
			}
			return result;
		}

		private static string EncryptKey(string key)
		{
			key = SecuredString.EncryptDecrypt(key, encryptionKey);
			key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
			return key;
		}

		private static string EncryptData(string key, byte[] cleanBytes, DataType type)
		{
			int dataLength = cleanBytes.Length;
			byte[] encryptedBytes = EncryptDecryptBytes(cleanBytes, dataLength, key + encryptionKey);

			uint dataHash = xxHash.CalculateHash(cleanBytes, dataLength);
			byte[] dataHashBytes = new byte[4]; // replaces BitConverter.GetBytes(hash);
			dataHashBytes[0] = (byte)(dataHash & 0xFF);
			dataHashBytes[1] = (byte)((dataHash >> 8) & 0xFF);
			dataHashBytes[2] = (byte)((dataHash >> 16) & 0xFF);
			dataHashBytes[3] = (byte)((dataHash >> 24) & 0xFF);

			byte[] deviceHashBytes = null;
			int finalBytesLength;
			if (lockToDevice != DeviceLockLevel.None)
			{
				// 4 device id hash + 1 data type + 1 device lock mode + 1 version + 4 data hash
				finalBytesLength = dataLength + 11;
				uint deviceHash = DeviceIDHash;
				deviceHashBytes = new byte[4]; // replaces BitConverter.GetBytes(hash);
				deviceHashBytes[0] = (byte)(deviceHash & 0xFF);
				deviceHashBytes[1] = (byte)((deviceHash >> 8) & 0xFF);
				deviceHashBytes[2] = (byte)((deviceHash >> 16) & 0xFF);
				deviceHashBytes[3] = (byte)((deviceHash >> 24) & 0xFF);
			}
			else
			{
				// 1 data type + 1 device lock mode + 1 version + 4 data hash
				finalBytesLength = dataLength + 7;
			}

			byte[] finalBytes = new byte[finalBytesLength];

			Buffer.BlockCopy(encryptedBytes, 0, finalBytes, 0, dataLength);
			if (deviceHashBytes != null)
			{
				Buffer.BlockCopy(deviceHashBytes, 0, finalBytes, dataLength, 4);
			}

			finalBytes[finalBytesLength - 7] = (byte)type;
			finalBytes[finalBytesLength - 6] = VERSION;
			finalBytes[finalBytesLength - 5] = (byte)lockToDevice;
			Buffer.BlockCopy(dataHashBytes, 0, finalBytes, finalBytesLength - 4, 4);

			return Convert.ToBase64String(finalBytes);
		}

		private static byte[] DecryptData(string key, string encryptedInput)
		{
			byte[] inputBytes;

			try
			{
				inputBytes = Convert.FromBase64String(encryptedInput);
			}
			catch (Exception)
			{
				SavesTampered();
				return null;
			}

			if (inputBytes.Length <= 0)
			{
				SavesTampered();
				return null;
			}


			int inputLength = inputBytes.Length;

			// reserved for future use
			//DataType type = (DataType)inputBytes[inputLength - 7];

			byte inputVersion = inputBytes[inputLength - 6];
			if (inputVersion != VERSION)
			{
				// in future we possibly will have some old versions fallbacks here
				SavesTampered();
				return null;
			}

			DeviceLockLevel inputLockToDevice = (DeviceLockLevel)inputBytes[inputLength - 5];

			byte[] dataHashBytes = new byte[4];
			Buffer.BlockCopy(inputBytes, inputLength - 4, dataHashBytes, 0, 4);
			uint inputDataHash = (uint)(dataHashBytes[0] | dataHashBytes[1] << 8 | dataHashBytes[2] << 16 | dataHashBytes[3] << 24);

			int dataBytesLength = 0;
			uint inputDeviceHash = 0;

			if (inputLockToDevice != DeviceLockLevel.None)
			{
				dataBytesLength = inputLength - 11;
				if (lockToDevice != DeviceLockLevel.None)
				{
					byte[] deviceHashBytes = new byte[4];
					Buffer.BlockCopy(inputBytes, dataBytesLength, deviceHashBytes, 0, 4);
					inputDeviceHash = (uint)(deviceHashBytes[0] | deviceHashBytes[1] << 8 | deviceHashBytes[2] << 16 | deviceHashBytes[3] << 24);
				}
			}
			else
			{
				dataBytesLength = inputLength - 7;
			}

			byte[] encryptedBytes = new byte[dataBytesLength];
			Buffer.BlockCopy(inputBytes, 0, encryptedBytes, 0, dataBytesLength);
			byte[] cleanBytes = EncryptDecryptBytes(encryptedBytes, dataBytesLength, key + encryptionKey);

			uint realDataHash = xxHash.CalculateHash(cleanBytes, dataBytesLength);
			if (realDataHash != inputDataHash)
			{
				SavesTampered();
				return null;
			}

			if (lockToDevice == DeviceLockLevel.Strict && inputDeviceHash == 0 && !emergencyMode &&!readForeignSaves)
			{
				return null;
			}

			if (inputDeviceHash != 0 && !emergencyMode)
			{
				uint realDeviceHash = DeviceIDHash;
				if (inputDeviceHash != realDeviceHash)
				{
					PossibleForeignSavesDetected();
					if (!readForeignSaves) return null;
				}
			}

			return cleanBytes;
		}

		private static uint CalculateChecksum(string input)
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(input + encryptionKey);
			uint hash = xxHash.CalculateHash(inputBytes, inputBytes.Length);
			return hash;
		}

		private static void SavesTampered()
		{
			OnCheatingDetecting?.Invoke();
		}

		private static void PossibleForeignSavesDetected()
		{
			if (OnOtherDeviceDetected != null && !foreignSavesReported)
			{
				foreignSavesReported = true;
				OnOtherDeviceDetected?.Invoke();
			}
		}

		private static string GetDeviceID()
		{
			string id = "";
			#if UNITY_IPHONE
				#if UNITY_5_0
						id = UnityEngine.iOS.Device.vendorIdentifier;
				#else
						id = iPhone.vendorIdentifier;
				#endif
			#endif

			#if !PREVENT_READ_PHONE_STATE
				if (String.IsNullOrEmpty(id)) id = SystemInfo.deviceUniqueIdentifier;
			#else
				Debug.LogError("Looks like you forced PREVENT_READ_PHONE_STATE flag, but still use LockToDevice feature. It will work incorrect!");
			#endif
			return id;
		}

		private static byte[] EncryptDecryptBytes(byte[] bytes, int dataLength, string key)
		{
			int encryptionKeyLength = key.Length;

			byte[] result = new byte[dataLength];

			for (int i = 0; i < dataLength; i++)
			{
				result[i] = (byte)(bytes[i] ^ key[i % encryptionKeyLength]);
			}

			return result;
		}

		private enum DataType: byte
		{
			Int = 5,
			UInt = 10,
			String = 15,
			Float = 20,
			Double = 25,
			Long = 30,
			Bool = 35,
			ByteArray = 40,
			Vector2 = 45,
			Vector3 = 50,
			Quaternion = 55,
			Color = 60,
			Rect = 65,
			Vector4 = 70
		}

		/// <summary>
		/// Used to specify level of the device lock feature strictness.
		/// </summary>
		public enum DeviceLockLevel : byte
		{
			/// <summary>
			/// Both locked and not locked to any device data can be read (default one).
			/// </summary>
			None,

			/// <summary>
			/// Performs checks for locked data and still allows reading not locked data (useful when you decided to lock your saves in one of app updates and wish to keep user data).
			/// </summary>
			Soft,

			/// <summary>
			/// Only locked to the current device data can be read. This is a preferred mode, but it should be enabled right from the first app release. If you released app without data lock consider using Soft lock or all previously saved data will not be accessible.
			/// </summary>
			Strict
		}
    }
}