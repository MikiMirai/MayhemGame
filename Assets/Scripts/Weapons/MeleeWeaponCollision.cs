using UnityEngine;

public class MeleeWeaponCollision : MonoBehaviour
{
    public WeaponController m_WeaponController;
    public GameObject m_HitParticle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {

        }
    }
}
