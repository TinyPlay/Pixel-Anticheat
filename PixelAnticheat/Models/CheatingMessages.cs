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
    /// Cheating Messages
    /// </summary>
    public static class CheatingMessages
    {
        public const string InjectionCheatingMessage = "An attempt to introduce dependencies while the application was running was detected. Restart the game and/or check your device for viruses.";
        public const string SpeedhackDetectedMessage = "Time acceleration was found within the game logic. Please restart the game and/or check your device for viruses.";
        public const string TeleportDetectedMessage = "A player's movement outside of game logic was detected. Please restart the game or contact the administration.";
        public const string WallhackDetectedMessage = "Wallhack detected. Please restart the game and try again.";
        public const string TimehackDetectedMessage =
            "During the session there was a desynchronization in time. Make sure that you did not reset the clock during the game.";

        public const string TimehackNetworkFailMessage = "Failed to get the network time. Please restart the game and make sure there is a network connection.";
        public const string MemoryHackDetectedMessage = "An attempt to change the application memory was detected. Please restart your game and/or check your device for viruses.";
    }
}