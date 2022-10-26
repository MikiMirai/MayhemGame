using UnityEngine;

public class HealScript : MonoBehaviour
{
    public int heal = 10;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            FindObjectOfType<HealthSystem>().Heal(heal);
        }
    }
}
