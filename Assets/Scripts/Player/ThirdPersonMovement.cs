using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PlayerModels;

public class ThirdPersonMovement : MonoBehaviour
{
    //Attach animator to set values
    [Header("Animation")]
    [Tooltip("Player's animtor goes here")]
    public Animator animator;
    private bool isWalkingLeft;
    private bool isWalkingRight;

    private CharacterController characterController;
    private DefaultInput defaultInput;
    private Vector2 input_Movement;
    private Vector2 input_View;

    [Header("References")]
    public Transform orientation;
    [Tooltip("Neck-height object for first person camera and weapons")]
    public Transform cameraHolder;
    [Tooltip("Third person camera holder")]
    public GameObject ThirdPersonCameraHolder;
    [Tooltip("Third person camera")]
    public CinemachineVirtualCamera ThirdPersonCamera;

    [Header("Settings")]
    [Tooltip("Most of player's settings")]
    public PlayerSettingsModel playerSettings;

    [Header("Gravity")]
    [Tooltip("Amount of gravity applied to player when falling")]
    public float gravityAmount;
    [Tooltip("Max amount of gravity force when falling")]
    public float gravityMin;
    private float playerGravity;

    [Tooltip("")]
    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    private bool isMidAir;
    public bool IsGrounded;
    public float rotationLerp = 0.5f;
    public Vector3 nextPosition;
    public Quaternion nextRotation;

    [Header("Debug")]
    [Tooltip("If the player is sprinting, for debug")]
    public bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    private void Awake()
    {
        defaultInput = new DefaultInput();
        Cursor.visible = false;

        //Get all input keys/axis
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        defaultInput.Character.Sprinting.performed += e => ToggleSprint();

        defaultInput.Enable();

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //Set animator values
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
        animator.SetBool("WalkingLeft", isWalkingLeft);
        animator.SetBool("WalkingRight", isWalkingRight);
    }

    private void FixedUpdate()
    {
        CalculateMovement();
        CalculateJump();

        CheckGrounded();
    }

    private void CheckGrounded()
    {
        if (characterController.isGrounded)
        {
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }

    private void CalculateMovement()
    {
        if (PauseMenu.isPaused)
        {
            return;
        }

        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        isWalkingLeft = false;
        isWalkingRight = false;

        //Make new movement vector and transform it to world space
        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVelocity, playerSettings.MovementSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        //Restrict max falling speed
        if (playerGravity > gravityMin && jumpingForce.y < 0.1f)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        //Turn off player gravity when player on ground
        if (playerGravity < -0.1 && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        //When player is jumping gravity has no effect
        if (jumpingForce.y > 0.1f)
        {
            playerGravity = 0;
        }

        //Add gravity or jumping force if there is any and move player
        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (characterController.isGrounded)
        {
            jumpingForce = Vector3.up * playerSettings.JumpingHeight;
            isMidAir = true;
        }

        //Double jump here
        else if (isMidAir)
        {
            jumpingForce = Vector3.up * playerSettings.JumpingHeight;
            isMidAir = false;
        }

        else if (!characterController.isGrounded)
        {
            return;
        }
    }

    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
            return;
        }

        isSprinting = !isSprinting;
    }
}
