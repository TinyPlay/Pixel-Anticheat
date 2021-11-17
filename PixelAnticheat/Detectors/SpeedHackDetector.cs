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
    
    /// <summary>
    /// Speedhack Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class SpeedHackDetector : BaseDetector
    {
        // Is Running
        private bool isRunning;
        
        // Detector Constants
        private const long TICKS_PER_SECOND = TimeSpan.TicksPerMillisecond * 1000;
        private const int THRESHOLD = 5000000;
        
        // Private Params
        private float interval = 1f;
        private byte maxFalsePositives = 3;
        private int coolDown = 30;
        
        private byte currentFalsePositives;
        private int currentCooldownShots;
        private long ticksOnStart;
        private long vulnerableTicksOnStart;
        private long prevTicks;
        private long prevIntervalTicks;
        
        // Prevent Direct Access
        private SpeedHackDetector() { }

        #region Setup Methods
        /// <summary>
        /// Setup Detector
        /// </summary>
        /// <param name="config"></param>
        public override void SetupDetector(IDetectorConfig config)
        {
            SpeedhackDetectorConfig cfg = (SpeedhackDetectorConfig) config;
            interval = cfg.interval;
            maxFalsePositives = cfg.maxFalsePositives;
            coolDown = cfg.coolDown;
        }
        #endregion
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if (isRunning || !enabled)
                return;
            
            ResetStartTicks();
            currentFalsePositives = 0;
            currentCooldownShots = 0;

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
        /// Reset Start Ticks
        /// </summary>
        private void ResetStartTicks()
        {
            ticksOnStart = DateTime.UtcNow.Ticks;
            vulnerableTicksOnStart = System.Environment.TickCount * TimeSpan.TicksPerMillisecond;
            prevTicks = ticksOnStart;
            prevIntervalTicks = ticksOnStart;
        }

        /// <summary>
        /// On Application Paused
        /// </summary>
        /// <param name="pause"></param>
        private void OnApplicationPause(bool pause)
        {
            if (!pause)
                ResetStartTicks();
        }
        
        /// <summary>
        /// On Update
        /// </summary>
        private void Update()
        {
            if (!isRunning) 
                return;

            long ticks = DateTime.UtcNow.Ticks;
            long ticksSpentSinceLastUpdate = ticks - prevTicks;
		
            if (ticksSpentSinceLastUpdate < 0 || ticksSpentSinceLastUpdate > TICKS_PER_SECOND)
            {
                if (Debug.isDebugBuild) Debug.LogWarning("SpeedHackDetector: System DateTime change or > 1 second game freeze detected!");
                ResetStartTicks();
                return;
            }
            prevTicks = ticks;

            long intervalTicks = (long)(interval * TICKS_PER_SECOND);
            
            if (ticks - prevIntervalTicks >= intervalTicks)
            {
                long vulnerableTicks = System.Environment.TickCount * TimeSpan.TicksPerMillisecond;

                if (Mathf.Abs((vulnerableTicks - vulnerableTicksOnStart) - (ticks - ticksOnStart)) > THRESHOLD)
                {
                    currentFalsePositives++;
                    if (currentFalsePositives > maxFalsePositives)
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning("SpeedHackDetector: final detection!");
                        OnCheatingDetected?.Invoke(CheatingMessages.SpeedhackDetectedMessage);
                        StopDetector();
                    }
                    else
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning("SpeedHackDetector: detection! Allowed false positives left: " + (maxFalsePositives - currentFalsePositives));
                        currentCooldownShots = 0;
                        ResetStartTicks();
                    }
                }
                else if (currentFalsePositives > 0 && coolDown > 0)
                {
                    if (Debug.isDebugBuild) Debug.LogWarning("SpeedHackDetector: success shot! Shots till Cooldown: " + (coolDown - currentCooldownShots));
                    currentCooldownShots++;
                    if (currentCooldownShots >= coolDown)
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning("SpeedHackDetector: Cooldown!");
                        currentFalsePositives = 0;
                    }
                }

                prevIntervalTicks = ticks;
            }
        }
    }
}