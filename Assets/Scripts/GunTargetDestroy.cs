using UnityEngine;

public class GunTargetDestroy : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
