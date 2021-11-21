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
namespace PixelAnticheat.Detectors
{
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Memory Hack Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class MemoryHackDetector : BaseDetector
    {
        // Is Running
        private bool isRunning;

        // Max Allowed Values for Hack Detection
        [HideInInspector] public float floatEpsilon = 0.0001f;
        [HideInInspector] public float vector2Epsilon = 0.1f;
        [HideInInspector] public float vector3Epsilon = 0.1f;
        [HideInInspector] public float vector4Epsilon = 0.1f;
        [HideInInspector] public float quaternionEpsilon = 0.1f;
        [HideInInspector] public float colorEpsilon = 0.1f;
        [HideInInspector] public byte color32Epsilon = 1;

        // Prevent Direct Access
        private MemoryHackDetector() { }
        
        /// <summary>
        /// Setup Detector
        /// </summary>
        /// <param name="config"></param>
        public override void SetupDetector(IDetectorConfig config)
        {
            MemoryHackDetectorConfig cfg = (MemoryHackDetectorConfig) config;
            floatEpsilon = cfg.floatEpsilon;
            vector2Epsilon = cfg.vector2Epsilon;
            vector3Epsilon = cfg.vector3Epsilon;
            vector4Epsilon = cfg.vector4Epsilon;
            quaternionEpsilon = cfg.quaternionEpsilon;
            colorEpsilon = cfg.colorEpsilon;
            color32Epsilon = cfg.color32Epsilon;
        }
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if (isRunning || !enabled)
                return;
            
            isRunning = true;
        }

        /// <summary>
        /// Stop Detector
        /// </summary>
        public override void StopDetector()
        {
            if (isRunning)
            {
                OnCheatingDetected.RemoveAllListeners();
                isRunning = false;
            }
        }

        /// <summary>
        /// Pause Detector
        /// </summary>
        public override void PauseDetector()
        {
            isRunning = false;
        }

        /// <summary>
        /// Resume Detector
        /// </summary>
        public override void ResumeDetector()
        {
            isRunning = true;
        }

        /// <summary>
        /// Check if is Running
        /// </summary>
        /// <returns></returns>
        public override bool IsRunning()
        {
            return isRunning;
        }
    }
}