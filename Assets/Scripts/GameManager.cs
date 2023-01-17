using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int currentPumpkin;
    public TextMeshProUGUI pumpkinText;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AddPumpkin(int pumpkinToAdd)
    {
        currentPumpkin += pumpkinToAdd;
        pumpkinText.text = "Pumpkin: " + currentPumpkin;
    }
}
