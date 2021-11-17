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
    /// Secured Bool Type
    /// </summary>
    [System.Serializable]
    public struct SecuredBool : IEquatable<SecuredBool>
    {
        // Crypto Key
        private static byte cryptoKey = 215;
        
        #if UNITY_EDITOR
        public static byte cryptoKeyEditor = cryptoKey;
        #endif
        
        // Serialized Fields
        [SerializeField] private byte currentCryptoKey;
        [SerializeField] private int hiddenValue;
        [SerializeField] private bool fakeValue;
        [SerializeField] private bool fakeValueChanged;
        [SerializeField] private bool inited;
        
        /// <summary>
        /// Secured Bool Constructor
        /// </summary>
        /// <param name="value"></param>
        private SecuredBool(int value)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = value;
            fakeValue = false;
            fakeValueChanged = false;
            inited = true;
        }
        
        /// <summary>
        /// Set New Crypto Key
        /// </summary>
        /// <param name="newKey"></param>
        public static void SetNewCryptoKey(byte newKey)
        {
            cryptoKey = newKey;
        }
        
        /// <summary>
        /// Apply New Crypto Key
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
        /// Use it to encrypt any bool Value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Encrypt(bool value)
        {
            return Encrypt(value, 0);
        }
        
        /// <summary>
        /// Encrypt Bool by Key
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Encrypt(bool value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            int encryptedValue = value ? 213 : 181;

            encryptedValue ^= key;

            return encryptedValue;
        }

        /// <summary>
        /// Decrypt int into bool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Decrypt(int value)
        {
            return Decrypt(value, 0);
        }
        
        /// <summary>
        /// Decrypt int into bool using key
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Decrypt(int value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            value ^= key;

            return value != 181;
        }

        /// <summary>
        /// Get Encrypted Data
        /// </summary>
        /// <returns></returns>
        public int GetEncrypted()
        {
            ApplyNewCryptoKey();
            return hiddenValue;
        }

        /// <summary>
        /// Set Encrypted Data
        /// </summary>
        /// <param name="encrypted"></param>
        public void SetEncrypted(int encrypted)
        {
            inited = true;
            hiddenValue = encrypted;
            if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
            {
                fakeValue = InternalDecrypt();
                fakeValueChanged = true;
            }
        }
        
        /// <summary>
        /// Internal Decrypt Method
        /// </summary>
        /// <returns></returns>
        private bool InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = cryptoKey;
                hiddenValue = Encrypt(false);
                fakeValue = false;
                fakeValueChanged = true;
                inited = true;
            }

            byte key = cryptoKey;

            if (currentCryptoKey != cryptoKey)
            {
                key = currentCryptoKey;
            }

            int value = hiddenValue;
            value ^= key;

            bool decrypted = value != 181;

            if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning() && fakeValueChanged && decrypted != fakeValue)
            {
                AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected?.Invoke(CheatingMessages.MemoryHackDetectedMessage);
            }

            return decrypted;
        }
        
        public static implicit operator SecuredBool(bool value)
        {
            SecuredBool obscured = new SecuredBool(Encrypt(value));

            if (AntiCheat.Instance().GetDetector<MemoryHackDetector>().IsRunning())
            {
                obscured.fakeValue = value;
                obscured.fakeValueChanged = true;
            }

            return obscured;
        }
        public static implicit operator bool(SecuredBool value)
        {
            return value.InternalDecrypt();
        }
        
        /// <summary>
        /// Equaks Method for any object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SecuredBool))
                return false;

            SecuredBool oi = (SecuredBool)obj;
            return (hiddenValue == oi.hiddenValue);
        }
        
        /// <summary>
        /// Equals Method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(SecuredBool obj)
        {
            return hiddenValue == obj.hiddenValue;
        }

        /// <summary>
        /// Get Hash Code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return InternalDecrypt().GetHashCode();
        }
        
        /// <summary>
        /// Convert to String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return InternalDecrypt().ToString();
        }
    }
}