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
    using Debug = UnityEngine.Debug;
    using PixelAnticheat.Models;
    using PixelAnticheat.Data;
    using PixelAnticheat.Utils;

    /// <summary>
    /// Time Hack Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class TimeHackDetector : BaseDetector
    {
        // Is Running
        private bool isRunning;
        
        // Time Hack Params
        private float timeCheckInterval = 10f;
        private int availableTolerance = 60;
        private bool networkCompare = true;
        
        // Time Checking Timer
        private float timeToCheck = 10f;
        private int lastTime = 0;
        private int lastLocalTime = 0;
        
        // Prevent Direct Access
        private TimeHackDetector() { }

        #region Setup
        /// <summary>
        /// Setup Detector
        /// </summary>
        /// <param name="config"></param>
        public override void SetupDetector(IDetectorConfig config)
        {
            TimeHackDetectorConfig cfg = (TimeHackDetectorConfig) config;
            timeCheckInterval = cfg.timeCheckInterval;
            availableTolerance = cfg.availableTolerance;
            networkCompare = cfg.networkCompare;
        }
        #endregion
        
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if(isRunning || !enabled)
                return;
            
            timeToCheck = timeCheckInterval;
            CompareTime();
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
            if(!isRunning)
                return;

            if (timeToCheck <= 0f)
            {
                CompareTime();
            }
            else
            {
                timeToCheck -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Compare Time
        /// </summary>
        private void CompareTime()
        {
            int currentNetworkTime = 0;
            int currentLocalTime = NetworkTime.GetCurrentLocalTime();

            if (networkCompare)
            {
                NetworkTime.GetCurrentNetworkTime(time =>
                {
                    currentNetworkTime = time;
                    if (lastTime == 0)
                    {
                        lastTime = currentNetworkTime;
                        lastLocalTime = currentLocalTime;
                    }
                    else
                    {
                        CompareTwoTimestamps(currentNetworkTime, currentLocalTime);
                    }
                }, () =>
                {
                    OnCheatingDetected?.Invoke(CheatingMessages.TimehackNetworkFailMessage);
                    StopDetector();
                });
            }
            else
            {
                currentNetworkTime = NetworkTime.GetCurrentLocalTime();
                if (lastTime == 0)
                {
                    lastTime = currentNetworkTime;
                    lastLocalTime = currentLocalTime;
                }
                else
                {
                    CompareTwoTimestamps(currentNetworkTime, currentLocalTime);
                }
            }
        }

        /// <summary>
        /// Compare Two Timestamps
        /// </summary>
        private void CompareTwoTimestamps(int currentNetworkTime, int currentLocalTime)
        {
            int networkTimeDiff = 0;
            int localTimeDiff = 0;
            int avgTimeDiff = 0;
            
            networkTimeDiff = currentNetworkTime - lastTime;
            localTimeDiff = currentLocalTime - lastLocalTime;
            avgTimeDiff = (localTimeDiff > networkTimeDiff)
                ? localTimeDiff - networkTimeDiff
                : networkTimeDiff - localTimeDiff;
            if (avgTimeDiff > availableTolerance)
            {
                OnCheatingDetected?.Invoke(CheatingMessages.TimehackDetectedMessage);
                StopDetector();
                return;
            }
                    
            lastTime = currentNetworkTime;
            lastLocalTime = currentLocalTime;
        }
    }
}