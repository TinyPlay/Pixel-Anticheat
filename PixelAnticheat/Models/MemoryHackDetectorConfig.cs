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
    /// <summary>
    /// Memory Hack Detector Config
    /// </summary>
    [System.Serializable]
    public class MemoryHackDetectorConfig : IDetectorConfig
    {
        public float floatEpsilon = 0.0001f;
        public float vector2Epsilon = 0.1f;
        public float vector3Epsilon = 0.1f;
        public float vector4Epsilon = 0.1f;
        public float quaternionEpsilon = 0.1f;
        public float colorEpsilon = 0.1f;
        public byte color32Epsilon = 1;
    }
}