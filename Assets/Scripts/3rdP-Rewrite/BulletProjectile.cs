using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidbody;

    [Header("General")]
    [Tooltip("Radius of this projectile's collision detection")]
    public float Radius = 0.01f;

    [Tooltip("LifeTime of the projectile")]
    public float MaxLifeTime = 5f;

    [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
    public Transform Root;

    [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
    public Transform Tip;

    [Tooltip("Layers this projectile can collide with")]
    public LayerMask HittableLayers = -1;

    Vector3 m_LastRootPosition;
    List<Collider> m_IgnoredColliders;

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        float speed = 10f;
        bulletRigidbody.velocity = transform.forward * speed;
    }

    void OnEnable()
    {
        OnShoot();

        Destroy(gameObject, MaxLifeTime);
    }

    new void OnShoot()
    {
        m_LastRootPosition = Root.position;
        m_IgnoredColliders = new List<Collider>();

        // Ignore colliders of owner
        Collider[] ownerColliders = Root.GetComponentsInChildren<Collider>();
        m_IgnoredColliders.AddRange(ownerColliders);

    }

    private void Update()
    {
        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        bool foundHit = false;

        // Sphere cast
        Vector3 displacementSinceLastFrame = Tip.position - m_LastRootPosition;
        RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, Radius,
            displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
            k_TriggerInteraction);
        foreach (var hit in hits)
        {
            if (IsHitValid(hit) && hit.distance < closestHit.distance)
            {
                foundHit = true;
                closestHit = hit;
            }
        }

        if (foundHit)
        {
            // Handle case of casting while already inside a collider
            if (closestHit.distance <= 0f)
            {
                closestHit.point = Root.position;
                closestHit.normal = -transform.forward;
            }

            OnHit(closestHit.point, closestHit.normal, closestHit.collider);
        }
    }

    bool IsHitValid(RaycastHit hit)
    {
        // ignore hits with triggers that don't have a Damageable component
        if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
        {
            return false;
        }

        // ignore hits with specific ignored colliders (self colliders, by default)
        if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
        {
            return false;
        }

        return true;
    }

    void OnHit(Vector3 point, Vector3 normal, Collider collider)
    {
        //Damageable damageable = collider.GetComponent<Damageable>();
        //if (damageable)
        //{
        //    damageable.InflictDamage(Damage, false, m_ProjectileBase.Owner);
        //}
        //decalPainter.PaintDecal(point, normal, collider);
        // Self destruct
        Destroy(this.gameObject);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Destroy(this.gameObject);
    //}
}
