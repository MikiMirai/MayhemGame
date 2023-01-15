using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
    Melee,
}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image of this weapon's crosshair")]
    public Sprite CrosshairSprite;

    [Tooltip("The size of the crosshair")]
    public int CrosshairSize;

    [Tooltip("The color of the crosshair")]
    public Color CrosshairColor;
}

public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("The name of the weapon displayed in the UI")]
    public string WeaponName;

    [Tooltip("The icon of the weapon displayed in the UI")]
    public Sprite WeaponIcon;

    [Header("References")] [Tooltip("Parent object of the weapon to be deactived")]
    public GameObject WeaponRoot;

    [Tooltip("Tip of the weapon, where the projectiles are shot")]
    public Transform WeaponMuzzle;

    [Tooltip("Default parameters for the crosshair")]
    public CrosshairData CrosshairDataDefault;

    [Tooltip("Parameters for the crosshair when targeting an enemy")]
    public CrosshairData CrosshairDataTargetInSight;

    //Shoot params
    [Header("Shoot Parameters")]
    [Tooltip("The type of weapon will affect how it shoots")]
    public WeaponShootType ShootType;

    [Tooltip("Minimum time between 2 shots")]
    public float DelayBetweenShots = 0.5f;

    [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    public float BulletSpreadAngle = 0f;

    [Tooltip("Amount of bullets per shot")]
    public int BulletsPerShot = 1;

    [Range(0f, 2f)]
    [Tooltip("Force that will push back the weapon after each shot")]
    public float RecoilForce = 1;

    [Range(0f, 1f)]
    [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
    public float AimZoomRatio = 1f;

    //Ammo params
    [Tooltip("Ammo UI reference")]
    public TextMeshProUGUI ammoText;

    [Tooltip("The projectile prefab")] 
    public ProjectileBase ProjectilePrefab;

    [Header("Ammo Parameters")]
    [Tooltip("Should the player manually reload")]
    public bool AutomaticReload = true;

    [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
    public bool HasPhysicalBullets = false;

    [Tooltip("Number of bullets in a clip")]
    public int ClipSize = 30;

    [Tooltip("Bullet Shell Casing")]
    public GameObject ShellCasing;

    [Tooltip("Weapon Ejection Port for physical ammo")]
    public Transform EjectionPort;

    [Range(0.0f, 5.0f)]
    [Tooltip("Force applied on the shell")]
    public float ShellCasingEjectionForce = 2.0f;

    [Range(1, 30)]
    [Tooltip("Maximum number of shell that can be spawned before reuse")]
    public int ShellPoolSize = 1;

    [Tooltip("Amount of ammo reloaded per second")]
    public float AmmoReloadRate = 1f;

    [Tooltip("Maximum amount of ammo on the player")]
    public int MaxAmmo = 8;

    //Audio & Visual effects
    [Header("Audio & Visual")]
    [Tooltip("Optional weapon animator for OnShoot animations")]
    public Animator WeaponAnimator;

    public UnityAction OnShoot;
    public event Action OnShootProcessed;

    bool m_WantsToShoot = false;
    float m_CurrentAmmo;
    int m_CarriedPhysicalBullets;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    Vector3 m_LastMuzzlePosition;
    const string k_AnimAttackParameter = "Attack";

    public GameObject Owner { get; set; }
    public GameObject SourcePrefab { get; set; }
    public bool IsReloading { get; private set; }
    public float CurrentAmmoRatio { get; private set; }

    public bool IsWeaponActive { get; private set; }
    public Vector3 MuzzleWorldVelocity { get; private set; }

    private Queue<Rigidbody> m_PhysicalAmmoPool;

    public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
    
    public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

    private void Awake()
    {
        m_CurrentAmmo = MaxAmmo;
        m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
        m_LastMuzzlePosition = WeaponMuzzle.position;

        if (HasPhysicalBullets)
        {
            m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

            for (int i = 0; i < ShellPoolSize; i++)
            {
                GameObject shell = Instantiate(ShellCasing, transform);
                shell.SetActive(false);
                m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
            }
        }
    }

    public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

    void ShootShell()
    {
        Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

        nextShell.transform.position = EjectionPort.transform.position;
        nextShell.transform.rotation = EjectionPort.transform.rotation;
        nextShell.gameObject.SetActive(true);
        nextShell.transform.SetParent(null);
        nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
        nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

        m_PhysicalAmmoPool.Enqueue(nextShell);
    }

    void Reload()
    {
        if (m_CarriedPhysicalBullets > 0)
        {
            m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
        }

        IsReloading = false;
    }

    //TODO: Reloading animation

    private void Update()
    {
        UpdateAmmo();

        if (Time.deltaTime > 0)
        {
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }
    }

    void UpdateAmmo()
    {
        //TODO: Update UI ammo
        ammoText.text = $"{m_CurrentAmmo} / {m_CarriedPhysicalBullets}";
    }

    public void ShowWeapon(bool show)
    {
        WeaponRoot.SetActive(show);

        IsWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
        m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
        m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
        m_LastTimeShot = Time.time;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        m_WantsToShoot = inputDown || inputHeld;
        switch (ShootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }

                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }

                return false;

            default:
                return false;
        }
    }

    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1f;

            return true;
        }

        return false;
    }

    void HandleShoot()
    {
        int bulletsPerShotFinal = BulletsPerShot;

        //Spawn all bullets with random directions
        for (int i = 0; i < bulletsPerShotFinal; i++)
        {
            Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
            ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position, 
                Quaternion.LookRotation(shotDirection));
            newProjectile.Shoot(this);
        }

        //TODO: Muzzle flash

        if (HasPhysicalBullets)
        {
            ShootShell();
            m_CarriedPhysicalBullets--;
        }

        m_LastTimeShot = Time.time;

        //TODO: Play shooting SFX

        //Trigger attack animation if any
        if (WeaponAnimator)
        {
            WeaponAnimator.SetTrigger(k_AnimAttackParameter);
        }

        OnShoot?.Invoke();
        OnShootProcessed?.Invoke();
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shooTransform)
    {
        float spreadAngleRation = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shooTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRation);

        return spreadWorldDirection;
    }
}

