using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour {

    private int currentHealth;
    public int healthMax = 100;
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    private void Start()
    {
        currentHealth = healthMax;
        healthText.text = "Health: " + currentHealth;
        healthBar.value = currentHealth;
    }

    // Gets your current health
    public int GetHealth()
    {
        return currentHealth;
    }

    // A method for Damage
    public void TakeDamage(int damageAmount)
    {
        // Taking health from current health
        currentHealth -= damageAmount;

        // Keeping it not going under 0
        if(currentHealth < 0)
        {
            currentHealth = 0;
        }

        healthText.text = "Health: " + currentHealth;
        healthBar.value = currentHealth;
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

        healthText.text = "Health: " + currentHealth;
        healthBar.value = currentHealth;
    }
}
