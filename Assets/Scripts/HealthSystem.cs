using UnityEngine;


public class HealthSystem : MonoBehaviour {

    private int health;
    private int healthMax;

    public HealthSystem(int healthMax)
    {
        this.healthMax = healthMax;
        health = healthMax;
    }

    // Gets your current health
    public int GetHealth()
    {
        return health;
    }

    // A method for Damage
    public void Damage(int damageAmount)
    {
        // Taking health from current health
        health -= damageAmount;

        // Keeping it not going under 0
        if(health < 0)
        {
            healthMax = 0;
        }
    }

    
    public void Heal(int healAmount)
    {
        // Healing the current health
        health += healAmount;

        // Keeping it to not exceed above 100
        if(health > healthMax)
        {
            health = healthMax;
        }
    }
}
