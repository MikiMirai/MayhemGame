using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Tooltip("Multiplier to apply received damage to object")]
    public float DamageMultiplier = 1f;

    [Range(0, 1)] [Tooltip("Multiplier to apply self damage to object")]
    public float SelfDamageMultiplier = 0.2f;

    public Health Health { get; private set; }

    void Awake()
    {
        // Get Health component from object or higher in hierarchy
        Health = GetComponent<Health>();
        if (!Health)
        {
            Health = GetComponentInParent<Health>();
        }
    }

    public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
    {
        if (Health)
        {
            var totalDamage = damage;

            // Don't add crit if it's explosion
            if (!isExplosionDamage)
            {
                totalDamage *= DamageMultiplier;
            }

            // Reduce damage if it's self damage
            if (Health.gameObject == damageSource)
            {
                totalDamage *= SelfDamageMultiplier;
            }

            // Apply the taken damage
            Health.TakeDamage(totalDamage, damageSource);
        }
    }
}
