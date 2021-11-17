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
namespace PixelAnticheat.Examples
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    
    /// <summary>
    /// Anti-Cheat UI View
    /// </summary>
    internal class AntiCheatUI : MonoBehaviour
    {
        /// <summary>
        /// UI Context
        /// </summary>
        public struct Context
        {
            public string message;
            public Action OnCloseButtonClicked;
            public Action OnContactsButtonClicked;
        }
        private Context _ctx;
        
        // UI References
        [Header("UI References")] 
        [SerializeField] private Canvas _viewCanvas;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _contactsButton;
        [SerializeField] private Text _messageText;

        /// <summary>
        /// Set UI Context
        /// </summary>
        /// <param name="_ctx"></param>
        public AntiCheatUI SetContext(Context ctx)
        {
            _ctx = ctx;
            
            // Set Message
            _messageText.text = _ctx.message;
            
            // Add Handlers
            _closeButton.onClick.AddListener(() =>
            {
                _ctx.OnCloseButtonClicked?.Invoke();
            });
            _contactsButton.onClick.AddListener(() =>
            {
                _ctx.OnContactsButtonClicked?.Invoke();
            });
            return this;
        }

        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake()
        {
            if (_viewCanvas == null)
                _viewCanvas = GetComponent<Canvas>();
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _contactsButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Show UI
        /// </summary>
        public void ShowUI()
        {
            _viewCanvas.enabled = true;
        }

        /// <summary>
        /// Hide UI
        /// </summary>
        public void HideUI()
        {
            _viewCanvas.enabled = false;
        }
    }
}