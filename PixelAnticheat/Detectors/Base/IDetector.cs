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
namespace PixelAnticheat.Detectors
{
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Anti Cheat Detector Interface
    /// </summary>
    public interface IDetector
    {
        public void SetupDetector(IDetectorConfig config);
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public void StartDetector();

        /// <summary>
        /// Stop Detector
        /// </summary>
        public void StopDetector();

        /// <summary>
        /// Pause Detector
        /// </summary>
        public void PauseDetector();

        /// <summary>
        /// Resume Detector
        /// </summary>
        public void ResumeDetector();

        /// <summary>
        /// Dispose Detector
        /// </summary>
        public void DisposeDetector();

        /// <summary>
        /// Check if is Running
        /// </summary>
        /// <returns></returns>
        public bool IsRunning();
    }
}