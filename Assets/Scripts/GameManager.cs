using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour, IDataPersistence
{
    public int currentPumpkin;
    public TextMeshProUGUI pumpkinText;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AddPumpkin(int pumpkinToAdd)
    {
        currentPumpkin += pumpkinToAdd;
        pumpkinText.text = "Pumpkin: " + currentPumpkin;
    }

    public void LoadData(GameData data)
    {
        foreach (KeyValuePair<string, bool> pair in data.pumpkinsCollected)
        {
            if (pair.Value)
            {
                currentPumpkin++;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        // No data needs to be saved for this script
    }
}
