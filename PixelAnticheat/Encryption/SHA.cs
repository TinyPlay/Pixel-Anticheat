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
    /// SHA Hashes Class
    /// </summary>
    public class SHA
    {
        /// <summary>
        /// Get SHA1 Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSHA1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get SHA1 Binary Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] GetSHA1ByteHash(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(input);
                return hash;
            }
        }

        /// <summary>
        /// Get SHA256 Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSHA256Hash(string input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
        
        /// <summary>
        /// Get SHA256 Binary Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] GetSHA256ByteHash(byte[] input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                var hash = sha256.ComputeHash(input);
                return hash;
            }
        }

        /// <summary>
        /// Get SHA512 Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSHA512Hash(string input)
        {
            using (SHA512Managed sha256 = new SHA512Managed())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
        
        /// <summary>
        /// Get SHA512 Binary Hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] GetSHA512ByteHash(byte[] input)
        {
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                var hash = sha512.ComputeHash(input);
                return hash;
            }
        }
    }
}