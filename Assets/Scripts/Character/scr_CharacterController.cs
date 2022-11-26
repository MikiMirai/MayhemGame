using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using static scr_Models;

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

    private Vector3 newCharacterRotation;

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

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing = 0.3f;
    public float cameraStandHeight = 1.3f;
    public float cameraCrouchHeight = 0.9f;
    public float cameraProneHeight = 0.6f;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private bool isMidAir;
    public float rotationLerp = 0.5f;
    public Vector3 nextPosition;
    public Quaternion nextRotation;

    private bool isSprinting;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        //Get all input keys/axis
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
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
        CalculateCameraHeight();

        //Set animator values
        anim.SetBool("isGrounded", characterController.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
        anim.SetBool("WalkingLeft", isWalkingLeft);
        anim.SetBool("WalkingRight", isWalkingRight);
    }

    private void CalculateView()
    {
        #region Horizontal Rotation

        if (!ThirdPersonCamera.activeSelf)
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
        if(input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningForwardSpeed;
        }

        isWalkingLeft = false;
        isWalkingRight = false;
        
        float verticalSpeed = 0;
        float horizontalSpeed = 0;

        
        if (Input.GetKeyDown(KeyCode.LeftShift) && playerSettings.RunToggle == false)
        {
            playerSettings.RunToggle = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && playerSettings.RunToggle == true)
        {
            playerSettings.RunToggle = false;
        }

        //Modify the speed by the player settings

        if (horizontalSpeed < 0)
        {
            isWalkingLeft = true;
        }
        else if (horizontalSpeed > 0)
        {
            isWalkingRight = true;
        }

        //Make new movement vector and transform it to world space
        var newMovementSpeed = new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

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
        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void CalculateCameraHeight()
    {

        var stanceHeight = cameraStandHeight;

        if (playerStance == PlayerStance.Crouch)
        {
            stanceHeight = cameraCrouchHeight;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            stanceHeight = cameraProneHeight;
        }

        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, stanceHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);
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
