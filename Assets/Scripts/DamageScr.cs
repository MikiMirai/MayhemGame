using UnityEngine;

public class DamageScr : MonoBehaviour
{
    public int damage = 10;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            FindObjectOfType<HealthSystem>().TakeDamage(damage);
        }
    }
}
