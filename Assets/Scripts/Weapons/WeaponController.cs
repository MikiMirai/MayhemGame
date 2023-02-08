using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
}

public enum AmmoType
{
    Pistol,
    Rifle,
    Shotgun,
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

    [Tooltip("The type of ammo the weapon uses")]
    public AmmoType AmmoType;

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

    [Tooltip("The projectile prefab")] 
    public ProjectileBase ProjectilePrefab;

    //Ammo params
    [Header("Ammo Parameters")]
    [Tooltip("Should the player manually reload")]
    public bool AutomaticReload = true;

    [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
    public bool HasPhysicalBullets = false;

    [Tooltip("Maximum amount of ammo carried by the player")]
    public int MaxCarriableAmmo = 30;

    [Tooltip("Bullet Shell Casing")]
    public GameObject ShellCasing;

    [Tooltip("Weapon Ejection Port for physical ammo")]
    public Transform EjectionPort;

    [Range(0.0f, 5.0f)]
    [Tooltip("Force applied on the shell when ejecting")]
    public float ShellCasingEjectionForce = 2.0f;

    [Range(1, 30)]
    [Tooltip("Maximum number of shells that can be spawned before reuse")]
    public int ShellPoolSize = 1;

    [Tooltip("Amount of ammo reloaded per second")]
    public float AmmoReloadRate = 1f;

    [Tooltip("Max number of bullets in gun")]
    public int MaxWeaponAmmo = 8;

    //Charge params
    [Header("Charging weapon parameters (charging weapons only)")]
    [Tooltip("Trigger a shot when maximum charge is reached")]
    public bool AutomaticReleaseOnCharged;

    [Tooltip("Duration to reach maximum charge (in sec)")]
    public float MaxChargeDuration = 2f;

    [Tooltip("Ammo used when starting to charge")]
    public float AmmoUsedOnStartCharge = 1f;

    [Tooltip("Additional ammo used when charge reaches its maximum (if any)")]
    public float AmmoUsageRateWhileCharging = 1f;

    //Audio & Visual effects
    [Header("Audio & Visual")]
    [Tooltip("Translation to apply to weapon arm when aiming")]
    public Vector3 AimOffset;

    [Tooltip("Optional weapon animator for shooting animations")]
    public Animator WeaponAnimator;

    const string k_AnimAttackParameter = "Attack";

    [Tooltip("sound played when shooting")]
    public AudioClip ShootSfx;

    [Tooltip("sound played when reloading")]
    public AudioClip ReloadSfx;

    [Tooltip("Sound played when changing to this weapon")]
    public AudioClip ChangeWeaponSfx;

    public UnityAction OnShoot;
    public event Action OnShootProcessed;

    bool m_WantsToShoot = false;
    public float m_CurrentAmmo;
    public int m_CarriedPhysicalBullets;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    Vector3 m_LastMuzzlePosition;

    public GameObject Owner { get; set; }

    [field: SerializeField]
    public bool isOwnerPlayer { get; set; }
    public PlayerWeaponsManager m_PlayerWeaponManager { get; set; }
    public EnemyWeaponManager m_EnemyWeaponManager { get; set; }
    public GameObject SourcePrefab { get; set; }
    //Charging
    public bool IsCharging { get; private set; }
    public float CurrentCharge { get; private set; }
    public float LastChargeTriggerTimestamp { get; private set; }
    //Reload
    public bool IsReloading { get; private set; }
    public float CurrentAmmoRatio { get; private set; }
    //Active weapon?
    public bool IsWeaponActive { get; private set; }
    public Vector3 MuzzleWorldVelocity { get; private set; }
    //Carried ammo
    private Queue<Rigidbody> m_PhysicalAmmoPool;

    private AudioSource m_WeaponAudioSource;

    public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);
    public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
    public float GetAmmoNeededToShoot() =>
            (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxWeaponAmmo * BulletsPerShot);

    private void Awake()
    {
        //player save?
        m_CurrentAmmo = MaxWeaponAmmo;
        m_CarriedPhysicalBullets = HasPhysicalBullets ? MaxCarriableAmmo : 0;
        m_LastMuzzlePosition = WeaponMuzzle.position;
        m_WeaponAudioSource = GetComponent<AudioSource>();
        m_PlayerWeaponManager = FindObjectOfType<PlayerWeaponsManager>();

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

        //TODO: Add audio for the guns
    }

    private void Start()
    {
        if (Owner.GetComponent<PlayerWeaponsManager>() != null)
        {
            isOwnerPlayer = true;
        }
        else if (Owner.GetComponent<EnemyWeaponManager>() != null)
        {
            isOwnerPlayer = false;
        }
    }

    //change it so it doesnt go higher than MaxCarriableAmmo

    public void AddPhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxCarriableAmmo);

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
        if (isOwnerPlayer)
        {
            switch (AmmoType)
            {
                case AmmoType.Pistol:
                    if (m_PlayerWeaponManager.PistolAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_PlayerWeaponManager.PistolAmmo, MaxWeaponAmmo);

                        m_PlayerWeaponManager.PistolAmmo = 0;
                    }
                    else
                    {
                        m_PlayerWeaponManager.PistolAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Pistol Bullets: {m_PlayerWeaponManager.PistolAmmo}");
                    };
                    break;
                case AmmoType.Rifle:
                    if (m_PlayerWeaponManager.RifleAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_PlayerWeaponManager.RifleAmmo, MaxWeaponAmmo);

                        m_PlayerWeaponManager.RifleAmmo = 0;
                    }
                    else
                    {
                        m_PlayerWeaponManager.RifleAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Rifle Bullets: {m_PlayerWeaponManager.RifleAmmo}");
                    }
                    break;
                case AmmoType.Shotgun:
                    if (m_PlayerWeaponManager.ShotgunAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_PlayerWeaponManager.ShotgunAmmo, MaxWeaponAmmo);

                        m_PlayerWeaponManager.ShotgunAmmo = 0;
                    }
                    else
                    {
                        m_PlayerWeaponManager.ShotgunAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Shotgun Bullets: {m_PlayerWeaponManager.ShotgunAmmo}");
                    }
                    break;
                default:
                    break;
            }
        }
        else if (isOwnerPlayer)
        {
            var m_CurrentManager = Owner.GetComponent<EnemyWeaponManager>();
            switch (AmmoType)
            {
                case AmmoType.Pistol:
                    if (m_CurrentManager.PistolAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_CurrentManager.PistolAmmo, MaxWeaponAmmo);

                        m_CurrentManager.PistolAmmo = 0;
                    }
                    else
                    {
                        m_CurrentManager.PistolAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Pistol Bullets: {m_CurrentManager.PistolAmmo}");
                    };
                    break;
                case AmmoType.Rifle:
                    if (m_CurrentManager.RifleAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_CurrentManager.RifleAmmo, MaxWeaponAmmo);

                        m_CurrentManager.RifleAmmo = 0;
                    }
                    else
                    {
                        m_CurrentManager.RifleAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Rifle Bullets: {m_CurrentManager.RifleAmmo}");
                    }
                    break;
                case AmmoType.Shotgun:
                    if (m_CurrentManager.ShotgunAmmo < MaxWeaponAmmo)
                    {
                        m_CurrentAmmo = Mathf.Min(m_CurrentManager.ShotgunAmmo, MaxWeaponAmmo);

                        m_CurrentManager.ShotgunAmmo = 0;
                    }
                    else
                    {
                        m_CurrentManager.ShotgunAmmo -= MaxWeaponAmmo;
                        m_CurrentAmmo = MaxWeaponAmmo;
                        Debug.Log($"Carried Shotgun Bullets: {m_CurrentManager.ShotgunAmmo}");
                    }
                    break;
                default:
                    break;
            }
        }

        //if (m_CarriedPhysicalBullets < MaxWeaponAmmo)
        //{
        //    m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, MaxWeaponAmmo);

        //    m_CarriedPhysicalBullets = 0;
        //}
        //else
        //{
        //    m_CarriedPhysicalBullets -= MaxWeaponAmmo;
        //    m_CurrentAmmo = MaxWeaponAmmo;
        //    Debug.Log($"Carried Bullets: {m_CarriedPhysicalBullets}");
        //}

        IsReloading = false;
    }

    public void StartReloadAnimation(bool playerHasAmmo)
    {
        if (m_CurrentAmmo < MaxWeaponAmmo && playerHasAmmo)
        {
            //GetComponent<Animator>().SetTrigger("Reload");
            IsReloading = true;
            if (ReloadSfx)
            {
                m_WeaponAudioSource.PlayOneShot(ReloadSfx);
            }

            Invoke("Reload", 3f);
            //Reload();
        }
    }

    private void Update()
    {
        //UpdateAmmo();
        UpdateCharge();

        if (Time.deltaTime > 0)
        {
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }
    }

    //void UpdateAmmo()
    //{
    //    //Ammo updates on weapon manager, use for something else
    //    ammoText.text = $"{m_CurrentAmmo} / {m_CarriedPhysicalBullets}";
    //}

    void UpdateCharge()
    {
        if (IsCharging)
        {
            if (CurrentCharge < 1f)
            {
                float chargeLeft = 1f - CurrentCharge;

                float chargeAdded = 0f;
                if (MaxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }
                else
                {
                    chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                }

                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // Check if the charge can actually be added
                float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo that the charge requires
                    UseAmmo(ammoThisChargeWouldRequire);

                    // Set the current charge
                    CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                }
            }
        }
    }

    public void ShowWeapon(bool show)
    {
        WeaponRoot.SetActive(show);

        IsWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxWeaponAmmo);
        m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
        m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxCarriableAmmo);
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

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    TryBeginCharge();
                }

                // Either player shoots by releasing button or weapon shoots automatically on full charge
                if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                {
                    return TryReleaseCharge();
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

    bool TryBeginCharge()
    {
        if (!IsCharging
            && m_CurrentAmmo >= AmmoUsedOnStartCharge
            && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            UseAmmo(AmmoUsedOnStartCharge);

            LastChargeTriggerTimestamp = Time.time;
            IsCharging = true;

            return true;
        }

        return false;
    }

    bool TryReleaseCharge()
    {
        if (IsCharging)
        {
            HandleShoot();

            CurrentCharge = 0f;
            IsCharging = false;

            return true;
        }

        return false;
    }

    void HandleShoot()
    {      
        int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;

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
        }

        m_LastTimeShot = Time.time;

        // Play shooting SFX
        if (ShootSfx)
        {
            m_WeaponAudioSource.PlayOneShot(ShootSfx);
        }

        //Trigger attack animation if there is an animator
        if (WeaponAnimator)
        {
            WeaponAnimator.SetTrigger(k_AnimAttackParameter);
        }

        OnShoot?.Invoke();
        OnShootProcessed?.Invoke();
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRation = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRation);

        return spreadWorldDirection;
    }
}

