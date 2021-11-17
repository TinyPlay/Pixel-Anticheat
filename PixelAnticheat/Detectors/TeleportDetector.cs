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
    /// Teleport Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class TeleportDetector : BaseDetector
    {
        // Is Running
        private bool isRunning;

        // Teleport Hack Target
        private Transform _target;
        private float availableSpeed = 3f;
        
        // Private Params
        private GameObject serviceContainer;
        private float timeToCheck = 1f;
        private Vector3 lastPosition;

        // Prevent Direct Access
        private TeleportDetector() { }
        
        #region Setup
        /// <summary>
        /// Setup Detector
        /// </summary>
        /// <param name="config"></param>
        public override void SetupDetector(IDetectorConfig config)
        {
            TeleportDetectorConfig cfg = (TeleportDetectorConfig) config;
            _target = cfg.detectorTarget;
            availableSpeed = cfg.availableSpeedPerSecond;
        }
        #endregion
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if(isRunning || !enabled)
                return;
            
            if(_target == null)
                Debug.LogError("Failed to Start Teleport Detector. Please, setup detector target using SetupDetector(IDetectorConfig config) method!");

            lastPosition = _target.position;
            timeToCheck = 1f;
            isRunning = true;
        }
        
        /// <summary>
        /// Stop Detector
        /// </summary>
        public override void StopDetector()
        {
            if (isRunning)
            {
                OnCheatingDetected?.RemoveAllListeners();
                isRunning = false;
            }
        }
        
        /// <summary>
        /// Pause Detector
        /// </summary>
        public override void PauseDetector()
        {
            if (!isRunning) return;
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

        /// <summary>
        /// On Update
        /// </summary>
        private void Update()
        {
            if (!isRunning) return;

            if (timeToCheck <= 0f)
            {
                float distance = Vector3.Distance(lastPosition, _target.position);
                if (distance > availableSpeed)
                {
                    OnCheatingDetected?.Invoke(CheatingMessages.TeleportDetectedMessage);
                    StopDetector();
                }

                lastPosition = _target.position;
            }
            else
            {
                timeToCheck -= Time.deltaTime;
            }
        }
    }
}