using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public PlayerInputHandler m_InputHandler;
    public GameObject pauseMenu;
    public GameObject optionsMenu;

    [Header("Public")]
    public static bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
        if (m_InputHandler == null)
        {
            Debug.Log($"Input Handler reference not found on {this.name}");
        }

        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_InputHandler.GetPauseInputDown())
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        try
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        try
        {
            pauseMenu.SetActive(false);
            optionsMenu.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowOptions()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
