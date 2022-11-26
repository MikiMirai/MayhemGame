using System;
using UnityEngine;

public static class scr_Models
{
    #region - Player -

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed;
        public float WalkingStrafeSpeed;
        public float WalkingBackwardsSpeed;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;
    }

    #endregion

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }
}