using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    //Attach animator to set values
    public Animator anim;

    private CharacterController characterController;
    private DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform camerHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        //Get all input keys/axis
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();

        defaultInput.Enable();

        //Get starting location of char and camera
        newCameraRotation = camerHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();

        //Set animator values
        anim.SetBool("isGrounded", characterController.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
    }

    private void CalculateView()
    {
        //Camera rotation left-right
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        //Turning rotation vector, back to quaternion
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        //Camera rotation up-down
        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
        //Turning rotation vector, back to quaternion
        camerHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        //Modify the speed by the player settings
        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;

        //Make new movement vector and transform it to world space
        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        //Restrict max falling speed
        if (playerGravity > gravityMin && jumpingForce.y < 0.1f)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        //Turn off player gravity when player on ground
        if (playerGravity < -1 && characterController.isGrounded)
        {
            playerGravity = -1;
        }

        //When player is jumping gravity has no effect
        if (jumpingForce.y > 0.1f)
        {
            playerGravity = 0;
        }

        //Add gravity or jumping force if there is any and move player
        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
    }
}
