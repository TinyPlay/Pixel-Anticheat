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
namespace PixelAnticheat
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using PixelAnticheat.Detectors;
    using PixelAnticheat.Models;
    using PixelAnticheat.SecuredTypes;
    
    /// <summary>
    /// Base Anti Cheat Class
    /// </summary>
    [DisallowMultipleComponent]
    public class AntiCheat : MonoBehaviour
    {
        // Anti-Cheat Events
        
        // Anti-Cheat Container
        private static AntiCheat _instance;
        private static GameObject _antiCheatContainer;
        private List<BaseDetector> _detectors = new List<BaseDetector>();

        /// <summary>
        /// Get Anti-Cheat Instance
        /// </summary>
        /// <returns></returns>
        public static AntiCheat Instance()
        {
            if (_antiCheatContainer == null)
                _antiCheatContainer = new GameObject("__ANTICHEAT__");
            if (_instance == null)
                _instance = _antiCheatContainer.AddComponent<AntiCheat>();
            return _instance;
        }

        /// <summary>
        /// On Start
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Add Detector for Initialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AntiCheat AddDetector<T>(IDetectorConfig config = null) where T : BaseDetector
        {
            GetDetector<T>(config);
            return _instance;
        }

        /// <summary>
        /// Get Detector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BaseDetector GetDetector<T>(IDetectorConfig config = null) where T : BaseDetector
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                if(config!=null) component.SetupDetector(config);
                _detectors.Add(component);
            }
            
            return component;
        }
        
        /// <summary>
        /// Remove Detector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AntiCheat RemoveDetector<T>() where T : BaseDetector
        {
            T component = GetComponent<T>();
            if (component != null)
            {
                _detectors.Remove(component);
                Destroy(component);
            }
            
            return _instance;
        }

        /// <summary>
        /// Initialize All Detectors
        /// </summary>
        public void InitializeAllDetectors()
        {
            foreach (BaseDetector detector in _detectors)
            {
                detector.StartDetector();
            }
        }

        /// <summary>
        /// Setup Encryption Keys for Secured Types
        /// </summary>
        /// <param name="keys"></param>
        public void SetupSecuredTypesKeys(SecuredTypesEncryptionKeys keys)
        {
            // Setup Base Types Keys
            SecuredBool.SetNewCryptoKey(keys.boolKey);
            SecuredByte.SetNewCryptoKey(keys.byteKey);
            SecuredChar.SetNewCryptoKey(keys.charKey);
            SecuredDecimal.SetNewCryptoKey(keys.decimalKey);
            SecuredDouble.SetNewCryptoKey(keys.doubleKey);
            SecuredFloat.SetNewCryptoKey(keys.floatKey);
            SecuredInt.SetNewCryptoKey(keys.intKey);
            SecuredLong.SetNewCryptoKey(keys.decimalKey);
            SecuredSByte.SetNewCryptoKey(keys.sbyteKey);
            SecuredShort.SetNewCryptoKey(keys.shortKey);
            SecuredString.SetNewCryptoKey(keys.stringKey);
            SecuredUInt.SetNewCryptoKey(keys.uintKey);
            SecuredULong.SetNewCryptoKey(keys.ulongKey);
            SecuredUShort.SetNewCryptoKey(keys.ushortKey);
            
            // Setup Additional Types Keys
            SecuredColor.SetNewCryptoKey(keys.colorKey);
            SecuredColor32.SetNewCryptoKey(keys.color32Key);
            SecuredQuaternion.SetNewCryptoKey(keys.quaternionKey);
            SecuredVector2.SetNewCryptoKey(keys.vector2Key);
            SecuredVector3.SetNewCryptoKey(keys.vector3Key);
            SecuredVector4.SetNewCryptoKey(keys.vector4Key);
        }
    }
}