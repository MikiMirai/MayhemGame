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
        public float ViewXSensitivity = 6;
        [Tooltip("Looking left/right sensitivity")]
        public float ViewYSensitivity = 6;
        [Tooltip("Invert down/up?")]
        public bool ViewXInverted = false;
        [Tooltip("Invert left/right?")]
        public bool ViewYInverted = false;

        [Header("Movement Settings")]
        [Tooltip("Smooth movement amount")]
        public float MovementSmoothing = 0.15f;

        [Header("Movement - Running")]
        [Tooltip("Forward running speed")]
        public float RunningForwardSpeed = 8;
        [Tooltip("Left/right running speed")]
        public float RunningStrafeSpeed = 6;

        [Header("Movement - Walking")]
        [Tooltip("Forward walking speed")]
        public float WalkingForwardSpeed = 3;
        [Tooltip("Left/right walking speed")]
        public float WalkingStrafeSpeed = 2;

        [Header("Jumping")]
        [Tooltip("Amount of force added to jumping")]
        public float JumpingHeight = 5;
        [Tooltip("Amount of time to smooth the jumping")]
        public float JumpingFalloff = 0.15f;
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