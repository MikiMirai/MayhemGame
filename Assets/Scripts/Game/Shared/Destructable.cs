using UnityEngine;

public class Destructable : MonoBehaviour
{
    Health _Health;

    void Start()
    {
        _Health = GetComponent<Health>();

        // Subscribe to Death and Damage actions
        _Health.OnDie += OnDie;
        _Health.OnDamaged += OnDamaged;
    }

    void OnDamaged(float damage, GameObject damageSource)
    {
        // TODO: what to do when taking damage
    }

    void OnDie()
    {
        // Destroy object
        Destroy(gameObject);

        Debug.Log("Enemy killed!");
    }
}
