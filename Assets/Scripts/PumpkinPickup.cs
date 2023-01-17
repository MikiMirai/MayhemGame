using UnityEngine;

public class PumpkinPickup : MonoBehaviour
{
    public int value;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            FindObjectOfType<GameManager>().AddPumpkin(value);

            Destroy(gameObject);
        }
    }
}
