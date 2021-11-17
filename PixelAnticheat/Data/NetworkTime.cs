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
namespace PixelAnticheat.Data
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Networking;
    using PixelAnticheat.Utils;
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Network Time Reciever Class
    /// </summary>
    public static class NetworkTime
    {
        private const string networkTimeServer = "https://worldtimeapi.org/api/timezone/Etc/UTC";
        
        /// <summary>
        /// Get Current Network Time (in UTC)
        /// </summary>
        /// <param name="onTimeRecieved"></param>
        /// <param name="onRequestError"></param>
        public static void GetCurrentNetworkTime(Action<int> onTimeRecieved, Action onRequestError)
        {
            CoroutineProvider.Start(RequestNetworkTime(onTimeRecieved, onRequestError));
        }

        /// <summary>
        /// Request Network Time
        /// </summary>
        /// <param name="onTimeRecieved"></param>
        /// <param name="onRequestError"></param>
        /// <returns></returns>
        private static IEnumerator RequestNetworkTime(Action<int> onTimeRecieved, Action onRequestError)
        {
            UnityWebRequest webRequest = new UnityWebRequest(networkTimeServer, "GET");
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            webRequest.downloadHandler = dH;
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                NetworkTimeModel response = JsonUtility.FromJson<NetworkTimeModel>(webRequest.downloadHandler.text);
                onTimeRecieved?.Invoke(response.unixtime);
            }
            else
            {
                onRequestError?.Invoke();
            }
            
            webRequest.Dispose();
        }
        
        /// <summary>
        /// Get Current Local Unix Time (in UTC)
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentLocalTime()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
            return currentEpochTime;
        }
        
        /// <summary>
        /// Get Difference between two Unix Timestamps (in Seconds)
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static int SecondsElapsed(int t1, int t2)
        {
            int difference = t1 - t2;
            return Mathf.Abs(difference);
        }
    }
}