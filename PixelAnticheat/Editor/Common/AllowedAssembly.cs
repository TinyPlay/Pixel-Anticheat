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
namespace PixelAnticheat.Editor.Common
{
    using System;
    
    /// <summary>
    /// Allowed Assembly
    /// </summary>
    internal class AllowedAssembly
    {
        public string name;
        public int[] hashes;

        public AllowedAssembly(string name, int[] hashes)
        {
            this.name = name;
            this.hashes = hashes;
        }

        public bool AddHash(int hash)
        {
            if (Array.IndexOf(hashes, hash) != -1) return false;

            int oldLen = hashes.Length;
            int newLen = oldLen + 1;

            int[] newHashesArray = new int[newLen];
            Array.Copy(hashes, newHashesArray, oldLen);

            hashes = newHashesArray;
            hashes[oldLen] = hash;

            return true;
        }

        public override string ToString()
        {
            return name + " (hashes: " + hashes.Length + ")";
        }
    }
}