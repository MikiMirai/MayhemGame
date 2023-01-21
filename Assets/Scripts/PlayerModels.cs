using System;
using UnityEngine;

public static class PlayerModels
{
    #region - Player -

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        [Tooltip("Looking down/up sensitivity")]
        public float ViewXSensitivity;
        [Tooltip("Looking left/right sensitivity")]
        public float ViewYSensitivity;
        [Tooltip("Invert down/up?")]
        public bool ViewXInverted;
        [Tooltip("Invert left/right?")]
        public bool ViewYInverted;

        [Header("Movement Settings")]
        [Tooltip("Smooth movement amount")]
        public float MovementSmoothing;

        [Header("Movement - Running")]
        [Tooltip("Forward running speed")]
        public float RunningForwardSpeed;
        [Tooltip("Left/right running speed")]
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        [Tooltip("Forward walking speed")]
        public float WalkingForwardSpeed;
        [Tooltip("Left/right walking speed")]
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        [Tooltip("Amount of force added to jumping")]
        public float JumpingHeight;
        [Tooltip("Amount of time to smooth the jumping")]
        public float JumpingFalloff;
    }
    [Serializable]
    public class CharacterStance
    {
        [Tooltip("Camera height when in this stance")]
        public float CameraHeight;
        [Tooltip("Reference to the collider corresponding to this stance")]
        public CapsuleCollider StanceCollider;
    }

    #endregion

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }
}