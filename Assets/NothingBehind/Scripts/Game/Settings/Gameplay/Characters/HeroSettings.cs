using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "HeroSettings", menuName = "Game Settings/Characters/New Hero Settings")]
    public class HeroSettings : ScriptableObject
    {
        public float Health;
        
        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 1.5f;

        [Tooltip("Aim speed of the character in m/s")]
        public float AimSpeed = 1.0f;

        [Tooltip("Crouch speed of the character in m/s")]
        public float CrouchSpeed = 1.3f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 4f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.158f;

        [Tooltip("How fast the character turns with right stick gamepad")] [Range(1, 360f)]
        public float GamepadRotationSpeed = 50f;

        [Tooltip("How fast the character turns with mouse")] [Range(0, 1000)]
        public float MouseRotationSpeed = 1000f;

        [Tooltip("Speed change animation for aim")] [Range(0, 20)]
        public float SpeedBlendAim = 3.18f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("CharacterController properties")] [Tooltip("Height Character Controller in crouch")] 
        public float crouchHeight = 1.3f;

        [Tooltip("Center Character Controller in crouch")]
        public Vector3 crouchCenter = new Vector3(0, 0.7f, 0);

        [Tooltip("Height Character Controller default")]
        public float defaultHeight = 1.8f;

        [Tooltip("Center Character Controller default")]
        public Vector3 defaultCenter = new Vector3(0, 0.93f, 0);

        [Space(10)]
        [Header("Player Grounded")]
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -9.81f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        public float TerminalVelocity = 53.0f;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.35f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Space(10)] [Header("Aim")] [SerializeField]
        public LayerMask AimColliderLayerMask = new LayerMask();

        public Vector3 WeaponHeightOffset = new Vector3(0f, 1.2f, 0f);
        
        [Space(10)] [Header("Clip Prevention")] 

        public LayerMask obstacleMask;
    }
}