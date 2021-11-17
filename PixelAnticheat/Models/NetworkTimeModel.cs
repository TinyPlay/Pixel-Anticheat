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
    /// Network Time Model
    /// </summary>
    [System.Serializable]
    public class NetworkTimeModel
    {
        public string abbreviation = "";
        public string client_ip = "";
        
        public string datetime = "";
        public int day_of_week = 0;
        public int day_of_year = 0;
        
        public bool dst = false;
        public string dst_from = "";
        public int dst_offset = 0;
        public string dst_until = "";

        public int raw_offset = 0;

        public string timezone = "";

        public int unixtime = 0;

        public string utc_datetime = "";
        public string utc_offset = "";

        public int week_number = 0;
    }
}