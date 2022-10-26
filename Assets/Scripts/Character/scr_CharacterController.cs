using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using static scr_Models;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;

public class scr_CharacterController : MonoBehaviour
{
    //Attach animator to set values
    public Animator anim;
    private bool isWalkingLeft;
    private bool isWalkingRight;

    private CharacterController characterController;
    private DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 input_View;

    [Header("References")]
    public Transform cameraHolder;
    public GameObject ThirdPersonCamera;

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

    private bool isMidAir;
    public float rotationLerp = 0.5f;
    public Vector3 nextPosition;
    public Quaternion nextRotation;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        //Get all input keys/axis
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();

        defaultInput.Enable();

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CalculateMovement();
        CalculateView();
        CalculateJump();

        //Set animator values
        anim.SetBool("isGrounded", characterController.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
        anim.SetBool("WalkingLeft", isWalkingLeft);
        anim.SetBool("WalkingRight", isWalkingRight);
    }

    private void CalculateView()
    {
        #region Horizontal Rotation

        //Rotate the Follow Target transform based on the input
        cameraHolder.transform.rotation *= Quaternion.AngleAxis(playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.x : input_View.x) * Time.deltaTime, Vector3.up);

        #endregion

        #region Vertical Rotation
        cameraHolder.transform.rotation *= Quaternion.AngleAxis(playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? -input_View.y : -input_View.y) * Time.deltaTime, Vector3.right);

        var angles = cameraHolder.transform.localEulerAngles;
        angles.z = 0;

        var angle = cameraHolder.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if (angle < 180 && angle > 40)
        {
            angles.x = 40;
        }


        cameraHolder.transform.localEulerAngles = angles;
        #endregion

        nextRotation = Quaternion.Lerp(cameraHolder.transform.rotation, nextRotation, Time.deltaTime * rotationLerp);

        if (input_Movement.x == 0 && input_Movement.y == 0)
        {
            if (Input.GetButton("Aim"))
            {
                //Set the player rotation based on the look transform
                transform.rotation = Quaternion.Euler(0, cameraHolder.transform.rotation.eulerAngles.y, 0);
                //reset the y rotation of the look transform
                cameraHolder.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
            }

            return;
        }

        //Set the player rotation based on the look transform
        transform.rotation = Quaternion.Euler(0, cameraHolder.transform.rotation.eulerAngles.y, 0);
        //reset the y rotation of the look transform
        cameraHolder.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
    }

    private void CalculateMovement()
    {
        isWalkingLeft = false;
        isWalkingRight = false;
        //Modify the speed by the player settings
        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;

        if (horizontalSpeed < 0)
        {
            isWalkingLeft = true;
        }
        else if (horizontalSpeed > 0)
        {
            isWalkingRight = true;
        }

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
}
