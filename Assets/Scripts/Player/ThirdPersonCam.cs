using Cinemachine;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public CinemachineFreeLook ThirdPersonCamera;
    private DefaultInput defaultInput;
    private Vector2 input_Movement;

    public float rotationSpeed = 7;

    public Vector3 cameraRotation;
    public Vector3 orientationRotation;

    private void Awake()
    {
        defaultInput = new DefaultInput();
        Cursor.visible = false;

        //Get all input keys/axis
        defaultInput.PlayerMovement.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();

        defaultInput.Enable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // Rotate player object
        float horizontalInput = input_Movement.x;
        float verticalInput = input_Movement.y;
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
