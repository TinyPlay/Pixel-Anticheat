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
namespace PixelAnticheat.Encryption
{
    using System;
    using System.Text;
    using System.Security.Cryptography;
    
    /// <summary>
    /// Base64 Encoding Class
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// Encode to Base64
        /// </summary>
        /// <param name="decodedText"></param>
        /// <returns></returns>
        public static string Encode(string decodedText)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes (decodedText);
            string encodedText = Convert.ToBase64String (bytesToEncode);
            return encodedText;
        }

        /// <summary>
        /// Encode Binary to Base64
        /// </summary>
        /// <param name="decodedBytes"></param>
        /// <returns></returns>
        public static string EncodeBinary(byte[] decodedBytes)
        {
            string encodedText = Convert.ToBase64String(decodedBytes);
            return encodedText;
        }

        /// <summary>
        /// Decode from Base64
        /// </summary>
        /// <param name="encodedText"></param>
        /// <returns></returns>
        public static string Decode(string encodedText)
        {
            byte[] decodedBytes = Convert.FromBase64String (encodedText);
            string decodedText = Encoding.UTF8.GetString (decodedBytes);
            return decodedText;
        }

        /// <summary>
        /// Decode Binary from Base64 String
        /// </summary>
        /// <param name="encodedText"></param>
        /// <returns></returns>
        public static byte[] DecodeBinary(string encodedText)
        {
            byte[] decodedBytes = Convert.FromBase64String (encodedText);
            return decodedBytes;
        }
    }
}