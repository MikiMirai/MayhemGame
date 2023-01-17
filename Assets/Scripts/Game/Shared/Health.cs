using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Tooltip("Max health of the enemy")]
    public float MaxHealth = 10f;

    [Tooltip("Enemy is invincible?")]
    public bool Invincible { get; set; }

    public UnityAction<float, GameObject> OnDamaged;
    public UnityAction<float> OnHealed;
    public UnityAction OnDie;

    public float CurrentHealth { get; set; }

    bool _IsDead;

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void Heal(float healAmount)
    {
        // Heal without overhealing
        float healthBefore = CurrentHealth;
        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        float trueHealAmount = CurrentHealth - healthBefore;
        if (trueHealAmount > 0)
        {
            // Call OnHealed with the real heal amount
            OnHealed?.Invoke(trueHealAmount);
        }
    }

    public void TakeDamage(float damage, GameObject damageSource)
    {
        // If invincible return and don't take damage
        if (Invincible)
            return;

        // Damage without going below 0
        float healthBefore = CurrentHealth;
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        float trueDamageAmount = healthBefore - CurrentHealth;
        if (trueDamageAmount > 0f)
        {
            // Call OnDamaged with the real damage dealt and the damage source
            OnDamaged?.Invoke(trueDamageAmount, damageSource);
        }

        HandleDeath();
    }

    // Destroy object by setting health to 0
    public void Kill()
    {
        CurrentHealth = 0f;

        OnDamaged?.Invoke(MaxHealth, null);

        HandleDeath();
    }

    void HandleDeath()
    {
        // If dead return
        if (_IsDead)
            return;

        // If health reaches 0 or less destroy object
        if (CurrentHealth <= 0f)
        {
            // Call OnDie and destroy object
            _IsDead = true;
            OnDie?.Invoke();
        }
    }
}
