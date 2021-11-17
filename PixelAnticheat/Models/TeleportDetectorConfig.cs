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
namespace PixelAnticheat.Models
{
    using UnityEngine;
    
    /// <summary>
    /// Teleport Detector Config
    /// </summary>
    [System.Serializable]
    public class TeleportDetectorConfig : IDetectorConfig
    {
        public float availableSpeedPerSecond = 5f;
        public Transform detectorTarget = null;
    }
}