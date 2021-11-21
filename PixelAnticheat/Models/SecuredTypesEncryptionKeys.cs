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
    /// Secured Types Encryption Keys Configs
    /// </summary>
    [System.Serializable]
    public class SecuredTypesEncryptionKeys
    {
        // Base Types Encryption Keys
        public byte boolKey = 215;
        public byte byteKey = 244;
        public char charKey = '\x2014';
        public long decimalKey = 209208L;
        public long doubleKey = 210987L;
        public int floatKey = 230887;
        public int intKey = 444444;
        public long longKey = 444442L;
        public sbyte sbyteKey = 112;
        public short shortKey = 214;
        public string stringKey = "4441";
        public uint uintKey = 240513;
        public ulong ulongKey = 444443L;
        public ushort ushortKey = 224;
        
        // Additional Types Encryption Keys
        public int colorKey = 120222;
        public int color32Key = 120223;
        public int quaternionKey = 120205;
        public int vector2Key = 120206;
        public int vector3Key = 120207;
        public int vector4Key = 120207;
    }
}