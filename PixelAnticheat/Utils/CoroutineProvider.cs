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
namespace PixelAnticheat.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// Coroutine Provider
    /// </summary>
    public class CoroutineProvider : MonoBehaviour
    {
        static CoroutineProvider _singleton;
        static Dictionary<string,IEnumerator> _routines = new Dictionary<string,IEnumerator>(100);

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        static void InitializeType ()
        {
            _singleton = new GameObject($"#{nameof(CoroutineProvider)}").AddComponent<CoroutineProvider>();
            DontDestroyOnLoad( _singleton );
        }

        public static Coroutine Start ( IEnumerator routine ) => _singleton.StartCoroutine( routine );
        public static Coroutine Start ( IEnumerator routine , string id )
        {
            var coroutine = _singleton.StartCoroutine( routine );
            if( !_routines.ContainsKey(id) ) _routines.Add( id , routine );
            else
            {
                _singleton.StopCoroutine( _routines[id] );
                _routines[id] = routine;
            }
            return coroutine;
        }
        public static void Stop ( IEnumerator routine ) => _singleton.StopCoroutine( routine );
        public static void Stop ( string id )
        {
            if( _routines.TryGetValue(id,out var routine) )
            {
                _singleton.StopCoroutine( routine );
                _routines.Remove( id );
            }
            else Debug.LogWarning($"coroutine '{id}' not found");
        }
        public static void StopAll () => _singleton.StopAllCoroutines();
    }
}