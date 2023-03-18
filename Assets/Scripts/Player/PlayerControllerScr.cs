using Cinemachine;
using UnityEngine;
using static PlayerModels;

public class PlayerControllerScr : MonoBehaviour
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
    private Vector3 newCharacterRotation;

    [Header("References")]
    [Tooltip("Neck-height object for first person camera and weapons")]
    public Transform cameraHolder;
    [Tooltip("Empty game object placed at feet")]
    public Transform feetTransform;
    [Tooltip("Third person camera holder")]
    public GameObject ThirdPersonCameraHolder;
    [Tooltip("Third person camera")]
    public CinemachineVirtualCamera ThirdPersonCamera;
    [Tooltip("First person camera")]
    public Camera FirstPersonCamera;
    [Tooltip("Choose the player layer to be checked for the stances")]
    public LayerMask playerMask;

    [Header("Settings")]
    [Tooltip("Most of player's settings")]
    public PlayerSettingsModel playerSettings;
    [Tooltip("Min rotation while looking down/up")]
    public float viewClampYMin = 300;
    [Tooltip("Max rotation while looking down/up")]
    public float viewClampYMax = 80;

    [Header("Gravity")]
    [Tooltip("Amount of gravity applied to player when falling")]
    public float gravityAmount;
    [Tooltip("Max amount of gravity force when falling")]
    public float gravityMin;
    private float playerGravity;

    [Tooltip("")]
    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Stance")]
    [Tooltip("The current player stance")]
    public PlayerStance playerStance;
    private float playerStanceSmoothing = 0.1f;
    [Tooltip("Player height when standing and it's collider")]
    public CharacterStance playerStandStance;
    [Tooltip("Player height when crouching and it's collider")]
    public CharacterStance playerCrouchStance;
    [Tooltip("Player height when proning and it's collider")]
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

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
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();
        defaultInput.Character.Sprinting.performed += e => ToggleSprint();

        defaultInput.Enable();

        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;
    }

    private void Update()
    {
        CalculateMovement();
        CalculateView();
        CalculateJump();
        CalculateStance();

        CheckGrounded();

        //Set animator values
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
        animator.SetBool("WalkingLeft", isWalkingLeft);
        animator.SetBool("WalkingRight", isWalkingRight);
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

    private void CalculateView()
    {
        if (PauseMenu.isPaused)
        {
            return;
        }

        #region Horizontal Rotation

        if (!ThirdPersonCameraHolder.activeSelf)
        {
            //Camera rotation left-right
            newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
            //Turning rotation vector, back to quaternion
            transform.localRotation = Quaternion.Euler(newCharacterRotation);
        }
        else 
        {
            //Rotate the Follow Target transform based on the input
            cameraHolder.transform.rotation *= Quaternion.AngleAxis(playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.x : input_View.x) * Time.deltaTime, Vector3.up);
        }

        #endregion

        #region Vertical Rotation
        cameraHolder.transform.rotation *= Quaternion.AngleAxis(playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? -input_View.y : -input_View.y) * Time.deltaTime, Vector3.right);

        var angles = cameraHolder.transform.localEulerAngles;
        angles.z = 0;

        var angle = cameraHolder.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        //if (angle > 180 && angle < viewClampYMin)
        //{
        //    angles.x = viewClampYMin;
        //}
        //else if (angle < 180 && angle > viewClampYMax)
        //{
        //    angles.x = viewClampYMax;
        //}
        if (angle > 180 && angle < viewClampYMin)
        {
            angles.x = viewClampYMin;
        }
        else if (angle < 180 && angle > viewClampYMax)
        {
            angles.x = viewClampYMax;
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

    private void CalculateStance()
    {
        var currentStance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }

        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
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

    private void Crouch()
    {
        if(playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        } 
        
        if (StanceCheck(playerCrouchStance.StanceCollider.height))
            {
                return;
            }

        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        if(playerStance == PlayerStance.Prone)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerProneStance.StanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
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
