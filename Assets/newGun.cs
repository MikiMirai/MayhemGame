using UnityEditor.PackageManager;
using UnityEngine;

public class newGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DecalPainter decalPainter;

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float nextTimeToFire = 0f;
    public float ImpactForse = 30f;

    public Transform muzzle;
    //public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("FireGun") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        // automatic fire
        //if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        //{
        //    nextTimeToFire = Time.time + 1f / fireRate;
        //    Shoot();
        //}

    }
    void Shoot()
    {
        //muzzleFlash.play();

        RaycastHit hit;
        if (Physics.Raycast(muzzle.transform.position, muzzle.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            //Target target = hit.transform.GetComponent<Target>();
            //if (target != null)
            //{
            //    traget.TakeDamage(damage);
            //}
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
            decalPainter.PaintDecal(hit.point, hit.normal, hit.collider);

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(hit.normal * ImpactForse);
            }
            //GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            //Destroy(impactGO,2f);
        };
    }
}
