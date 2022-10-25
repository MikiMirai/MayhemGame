using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int currentPumpkin;
    public int currentGold;
    public TextMeshProUGUI pumpkinText;
    public TextMeshProUGUI goldText;

    // Start is called before the first frame update
    private void Start()
    {
        HealthSystem healthSystem = new HealthSystem(100);

        Debug.Log("Health: " + healthSystem.GetHealth());
        healthSystem.Damage(10);
        Debug.Log("Health: " + healthSystem.GetHealth());
        healthSystem.Heal(10);
        Debug.Log("Health: " + healthSystem.GetHealth());

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
