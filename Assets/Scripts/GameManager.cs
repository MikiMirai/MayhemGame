using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int currentPumpkin;
    public TextMeshProUGUI pumpkinText;

    public void AddPumpkin(int pumpkinToAdd)
    {
        currentPumpkin += pumpkinToAdd;
        pumpkinText.text = "Pumpkin: " + currentPumpkin;
    }
}
