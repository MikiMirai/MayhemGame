using TMPro;
using UnityEngine;

public class HealthSystem : MonoBehaviour {

    private int currentHealth;
    public int healthMax = 100;
    public TextMeshProUGUI healthBar;

    private void Start()
    {
        currentHealth = healthMax;
        healthBar.text = "Health: " + currentHealth;
    }

    // Gets your current health
    public int GetHealth()
    {
        return currentHealth;
    }

    // A method for Damage
    public void Damage(int damageAmount)
    {
        // Taking health from current health
        currentHealth -= damageAmount;

        // Keeping it not going under 0
        if(currentHealth < 0)
        {
            currentHealth = 0;
        }

        healthBar.text = "Health: " + currentHealth;
    }

    
    public void Heal(int healAmount)
    {
        // Healing the current health
        currentHealth += healAmount;

        // Keeping it to not exceed above 100
        if(currentHealth > healthMax)
        {
            currentHealth = healthMax;
        }

        healthBar.text = "Health: " + currentHealth;
    }
}
