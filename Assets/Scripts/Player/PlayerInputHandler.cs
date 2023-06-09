using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    DefaultInput playerControls;
    PlayerControllerScr m_PlayerCharacterController;
    bool m_FireInputWasHeld;

    private void Awake()
    {
        playerControls = new DefaultInput();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        m_PlayerCharacterController = GetComponent<PlayerControllerScr>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        m_FireInputWasHeld = GetFireInputHeld();
    }

    public bool CanProcessInput()
    {
        if (PauseMenu.isPaused)
        {
            return false;
        }
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public bool GetPauseInputDown()
    {
        return Input.GetButtonUp("Pause");
    }

    public bool GetFireInputDown()
    {
        return GetFireInputHeld() && !m_FireInputWasHeld;
    }

    public bool GetFireInputReleased()
    {
        return !GetFireInputHeld() && m_FireInputWasHeld;
    }

    public bool GetFireInputHeld()
    {
        if (CanProcessInput())
        {
            return playerControls.Player.Fire.ReadValue<float>() > 0.1f;
        }

        return false;
    }

    public bool GetAimInputHeld()
    {
        if (CanProcessInput())
        {
            return playerControls.Player.Aim.ReadValue<float>() > 0.1f;
        }

        return false;
    }

    public bool GetReloadButtonDown()
    {
        if (CanProcessInput())
        {
            return playerControls.Player.Reload.ReadValue<float>() > 0.1f;
        }

        return false;
    }

    public int GetSwitchWeaponInput()
    {
        if (CanProcessInput())
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                return -1;
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                return 1;
        }

        return 0;
    }

    public int GetSelectWeaponInput()
    {
        if (CanProcessInput())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                return 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                return 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                return 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                return 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                return 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                return 6;
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                return 7;
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                return 8;
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                return 9;
            else
                return 0;
        }

        return 0;
    }
}
