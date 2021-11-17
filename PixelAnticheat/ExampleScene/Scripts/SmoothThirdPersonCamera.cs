namespace PixelAnticheat.Examples
{
    using UnityEngine;
    
    /// <summary>
    /// Third Person Camera Controller Component
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SmoothThirdPersonCamera : MonoBehaviour
    {
        [Header("Camera Params")]
        [SerializeField] private Transform Target;
        [SerializeField] private float CameraSpeed = 0.125f;
        [SerializeField] private Vector3 CameraOffset = new Vector3();
        
        /// <summary>
        /// Fixed Update
        /// </summary>
        private void FixedUpdate ()
        {
            Vector3 desiredPosition = Target.position + CameraOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, CameraSpeed);
            transform.position = smoothedPosition;
            transform.LookAt(Target);
        }
    }
}