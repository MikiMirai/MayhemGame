using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int currentPumpkin;
    public int currentGold;
    public TextMeshProUGUI pumpkinText;
    public TextMeshProUGUI goldText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddGold(int goldToAdd)
    {
        currentGold += goldToAdd;
        goldText.text = "Gold: " + currentGold;
    }

    public void AddPumpkin(int pumpkinToAdd)
    {
        currentPumpkin += pumpkinToAdd;
        pumpkinText.text = "Pumpkin: " + currentPumpkin;
    }
}
