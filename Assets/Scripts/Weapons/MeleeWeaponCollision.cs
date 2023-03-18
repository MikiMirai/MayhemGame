using UnityEngine;

public class MeleeWeaponCollision : MonoBehaviour
{
    [Header("Stats")]
    public int Damage = 1;

    [Header("References")]
    public WeaponController m_WeaponController;
    public GameObject m_HitParticle;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.tag == "Enemy" && m_WeaponController.m_IsAttacking)
        {
            Debug.Log(other.name);

            //other.GetComponent<Animator>().SetTrigger("Hit");
            //Instantiate(m_HitParticle, new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z), other.transform.rotation);

            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.InflictDamage(Damage, false, m_WeaponController.Owner);
            }
        }
    }
}
