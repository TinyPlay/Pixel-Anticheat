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
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Base Detector Class
    /// </summary>
    [AddComponentMenu("")]
    public class BaseDetector : MonoBehaviour, IDetector
    {
        // Add Events
        public UnityEvent<string> OnCheatingDetected = new UnityEvent<string>();
        
        // Is Initialized
        private bool inited;
        
        /// <summary>
        /// On Start
        /// </summary>
        private void Start()
        {
            inited = true;
        }
        
        /// <summary>
        /// On Disable
        /// </summary>
        private void OnDisable()
        {
            if (!inited) return;
            PauseDetector();
        }
        
        /// <summary>
        /// On Enable
        /// </summary>
        private void OnEnable()
        {
            if (!inited || OnCheatingDetected == null) return;
            ResumeDetector();
        }
        
        /// <summary>
        /// On Application Quit
        /// </summary>
        private void OnApplicationQuit()
        {
            DisposeDetector();
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy()
        {
            DisposeDetector();
        }

        /// <summary>
        /// Start Detector
        /// </summary>
        public virtual void StartDetector() {
        }

        /// <summary>
        /// Stop Detector
        /// </summary>
        public virtual void StopDetector() { }

        /// <summary>
        /// Pause Detector
        /// </summary>
        public virtual void PauseDetector() { }

        /// <summary>
        /// Resume Detector
        /// </summary>
        public virtual void ResumeDetector() { }

        /// <summary>
        /// Setup detector Config
        /// </summary>
        /// <param name="config"></param>
        public virtual void SetupDetector(IDetectorConfig config) { }

        /// <summary>
        /// Dispose Detector
        /// </summary>
        public virtual void DisposeDetector()
        {
            StopDetector();
            Destroy(this);
        }

        /// <summary>
        /// Check if Detector is Running
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRunning()
        {
            return false;
        }
    }
}