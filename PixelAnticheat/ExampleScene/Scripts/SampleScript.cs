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
namespace PixelAnticheat.Examples
{
    using UnityEngine;
    using System.Collections.Generic;
    using PixelAnticheat.Detectors;
    using PixelAnticheat.Models;

    /// <summary>
    /// Example Script
    /// </summary>
    public class SampleScript : MonoBehaviour
    {
        [Header("Anti-Cheat References")] 
        [SerializeField] private Transform _playerTransform;
        
        [Header("UI Referneces")] 
        [SerializeField] private AntiCheatUI _antiCheatUI;

        /// <summary>
        /// On Start
        /// </summary>
        private void Start()
        {
            // Initialize All Detectors
            AntiCheat.Instance()
                .AddDetector<MemoryHackDetector>(new MemoryHackDetectorConfig())
                .AddDetector<InjectionDetector>()
                .AddDetector<SpeedHackDetector>(new SpeedhackDetectorConfig()
                {
                    coolDown = 30,
                    interval = 1f,
                    maxFalsePositives = 3
                })
                .AddDetector<WallHackDetector>(new WallhackDetectorConfig()
                {
                    spawnPosition = new Vector3(0,0,0)
                })
                .AddDetector<TeleportDetector>(new TeleportDetectorConfig()
                {
                    detectorTarget = _playerTransform,
                    availableSpeedPerSecond = 20f
                })
                .AddDetector<TimeHackDetector>(new TimeHackDetectorConfig()
                {
                    availableTolerance = 120,
                    networkCompare = true,
                    timeCheckInterval = 30f
                })
                .InitializeAllDetectors();

            // Add Detectors Handlers
            AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            AntiCheat.Instance().GetDetector<InjectionDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            AntiCheat.Instance().GetDetector<SpeedHackDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            AntiCheat.Instance().GetDetector<WallHackDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            AntiCheat.Instance().GetDetector<TeleportDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            AntiCheat.Instance().GetDetector<TimeHackDetector>().OnCheatingDetected.AddListener(DetectorCallback);
            
            AntiCheat.Instance().GetDetector<TimeHackDetector>().StartDetector();
        }

        /// <summary>
        /// Remove All Listeners
        /// </summary>
        private void OnDestroy()
        {
            AntiCheat.Instance().GetDetector<MemoryHackDetector>().OnCheatingDetected.RemoveAllListeners();
            AntiCheat.Instance().GetDetector<InjectionDetector>().OnCheatingDetected.RemoveAllListeners();
            AntiCheat.Instance().GetDetector<SpeedHackDetector>().OnCheatingDetected.RemoveAllListeners();
            AntiCheat.Instance().GetDetector<WallHackDetector>().OnCheatingDetected.RemoveAllListeners();
            AntiCheat.Instance().GetDetector<TeleportDetector>().OnCheatingDetected.RemoveAllListeners();
            AntiCheat.Instance().GetDetector<TimeHackDetector>().OnCheatingDetected.RemoveAllListeners();
        }
        
        /// <summary>
        /// Detector Callback
        /// </summary>
        /// <param name="message"></param>
        private void DetectorCallback(string message){
            Debug.Log("Cheating Detected: " + message);
            if (_antiCheatUI != null)
            {
                _antiCheatUI.SetContext(new AntiCheatUI.Context
                {
                    message = message,
                    OnCloseButtonClicked = QuitGame,
                    OnContactsButtonClicked = GoToSupport
                }).ShowUI();
            }
        }

        /// <summary>
        /// Quit Game
        /// </summary>
        private void QuitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Go To Support
        /// </summary>
        private void GoToSupport()
        {
            Application.OpenURL("https://example.com/");
        }
    }
}