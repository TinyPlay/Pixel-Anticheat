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
    /// Speedhack Detector Configs
    /// </summary>
    [System.Serializable]
    public class SpeedhackDetectorConfig : IDetectorConfig
    {
        public float interval = 1f;
        public byte maxFalsePositives = 3;
        public int coolDown = 30;
    }
}