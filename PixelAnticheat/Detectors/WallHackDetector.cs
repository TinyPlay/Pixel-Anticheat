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
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using PixelAnticheat.Models;
    
    /// <summary>
    /// Wallhack Detector Class
    /// </summary>
    [DisallowMultipleComponent]
    public class WallHackDetector : BaseDetector
    {
        // Is Running
        private bool isRunning;
        
        // Wall Hack Constants
        private const string SERVICE_CONTAINER_NAME = "__WALLHACK_SERVICE__";
        private readonly Vector3 rigidPlayerVelocity = new Vector3(0, 0, 1f);
        
        // Spawn Position
        private Vector3 spawnPosition;
        
        // Private Params
        private int whLayer = -1;
        private GameObject serviceContainer;
        private Rigidbody rigidPlayer;
        private CharacterController charControllerPlayer;
        private float charControllerVelocity = 0;
        
        // Debug
        #if DEBUG
        private bool rigidDetected = false;
        private bool controllerDetected = false;
        #endif
        
        // Prevent Direct Access
        private WallHackDetector() { }

        #region Setup
        /// <summary>
        /// Setup Detector
        /// </summary>
        /// <param name="config"></param>
        public override void SetupDetector(IDetectorConfig config)
        {
            WallhackDetectorConfig cfg = (WallhackDetectorConfig) config;
            spawnPosition = cfg.spawnPosition;
        }
        #endregion
        
        /// <summary>
        /// Start Detector
        /// </summary>
        public override void StartDetector()
        {
            if(isRunning || !enabled)
                return;
            
            InitDetector();
            isRunning = true;
        }
        
        /// <summary>
        /// Stop Detector
        /// </summary>
        public override void StopDetector()
        {
            if (isRunning)
            {
                UninitDetector();
                OnCheatingDetected?.RemoveAllListeners();
                isRunning = false;
            }
        }

        /// <summary>
        /// Pause Detector
        /// </summary>
        public override void PauseDetector()
        {
            if (!isRunning) return;
            
            isRunning = false;
            StopRigidModule();
            StopControllerModule();
        }

        /// <summary>
        /// Resume Detector
        /// </summary>
        public override void ResumeDetector()
        {
            isRunning = true;
            StartRigidModule();
            StartControllerModule();
        }

        /// <summary>
        /// Check if is Running
        /// </summary>
        /// <returns></returns>
        public override bool IsRunning()
        {
            return isRunning;
        }
        
        /// <summary>
        /// Init Detector
        /// </summary>
        private void InitDetector()
        {
            InitCommon();
            InitRigidModule();
            InitControllerModule();

            StartRigidModule();
            StartControllerModule();
        }
        
        /// <summary>
        /// Uninit Detector
        /// </summary>
        private void UninitDetector()
        {
            isRunning = false;
            StopRigidModule();
            StopControllerModule();
            Destroy(serviceContainer);
        }
        
        /// <summary>
        /// Initialize Common
        /// </summary>
        private void InitCommon()
        {
            if (whLayer == -1) whLayer = LayerMask.NameToLayer("Ignore Raycast");

            serviceContainer = new GameObject(SERVICE_CONTAINER_NAME);
            serviceContainer.layer = whLayer;
            serviceContainer.transform.position = spawnPosition;
            DontDestroyOnLoad(serviceContainer);

            GameObject wall = new GameObject("Wall");
            wall.AddComponent<BoxCollider>();
            wall.layer = whLayer;
            wall.transform.parent = serviceContainer.transform;
            wall.transform.localPosition = Vector3.zero;

            wall.transform.localScale = new Vector3(3, 3, 0.5f);
        }
        
        /// <summary>
        /// Init Rigid Module
        /// </summary>
        private void InitRigidModule()
        {
            GameObject player = new GameObject("RigidPlayer");
            player.AddComponent<CapsuleCollider>().height = 2;
            player.layer = whLayer;
            player.transform.parent = serviceContainer.transform;
            player.transform.localPosition = new Vector3(0.75f, 0, -1f);
            rigidPlayer = player.AddComponent<Rigidbody>();
            rigidPlayer.useGravity = false;
        }
        
        /// <summary>
        /// Init Controller Module
        /// </summary>
        private void InitControllerModule()
        {
            GameObject player = new GameObject("ControlledPlayer");
            player.AddComponent<CapsuleCollider>().height = 2;
            player.layer = whLayer;
            player.transform.parent = serviceContainer.transform;
            player.transform.localPosition = new Vector3(-0.75f, 0, -1f);
            charControllerPlayer = player.AddComponent<CharacterController>();
        }
        
        /// <summary>
        /// Start RigidBody Module
        /// </summary>
        private void StartRigidModule()
        {
            rigidPlayer.rotation = Quaternion.identity;
            rigidPlayer.angularVelocity = Vector3.zero;
            rigidPlayer.transform.localPosition = new Vector3(0.75f, 0, -1f);
            rigidPlayer.velocity = rigidPlayerVelocity;
            Invoke("StartRigidModule", 4);
        }

        /// <summary>
        /// Stop RigidBody Module
        /// </summary>
        private void StopRigidModule()
        {
            rigidPlayer.velocity = Vector3.zero;
            CancelInvoke("StartRigidModule");
        }

        /// <summary>
        /// Start Controller Module
        /// </summary>
        private void StartControllerModule()
        {
            charControllerPlayer.transform.localPosition = new Vector3(-0.75f, 0, -1f);
            charControllerVelocity = 0.01f;
            Invoke("StartControllerModule", 4);
        }

        /// <summary>
        /// Stop Controller Module
        /// </summary>
        private void StopControllerModule()
        {
            charControllerVelocity = 0;
            CancelInvoke("StartControllerModule");
        }
        
        /// <summary>
        /// Fixed Update
        /// </summary>
        private void FixedUpdate()
        {
            if (!isRunning) return;

            if (rigidPlayer.transform.localPosition.z > 1f)
            {
                #if DEBUG
                rigidDetected = true;
                #endif
                StopRigidModule();

                Detect();
            }
        }
        
        /// <summary>
        /// On Update
        /// </summary>
        private void Update()
        {
            if (!isRunning) return;

            if (charControllerVelocity > 0)
            {
                charControllerPlayer.Move(new Vector3(Random.Range(-0.002f, 0.002f), 0, charControllerVelocity));

                if (charControllerPlayer.transform.localPosition.z > 1f)
                {
                    #if DEBUG
                    controllerDetected = true;
                    #endif
                    StopControllerModule();

                    Detect();
                }
            }
        }

        /// <summary>
        /// Detect Cheating
        /// </summary>
        private void Detect()
        {
            OnCheatingDetected?.Invoke(CheatingMessages.WallhackDetectedMessage);
            StopDetector();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(spawnPosition, new Vector3(3, 3, 3));
        }

        #if DEBUG
        private void OnGUI()
        {
            GUILayout.Label("Rigid detected: " + rigidDetected);
            GUILayout.Label("Controller detected: " + controllerDetected);
        }
        #endif
    }
}