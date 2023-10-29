using Cinemachine;
using StarterAssets;
using UnityEngine;

public class ThirdPersonShooterController : MonoBehaviour
{

    [Header("Camera Sensitivity")]
    [SerializeField] private float normalSensitivity;

    [Tooltip("When aimed, should be lower for higher accuracy")]
    [SerializeField] private float aimSensitivity;

    [Tooltip("What the aim ray CAN hit, MUST be every object that the weapon should hit")]
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    [Header("References")]
    [Tooltip("Aim Virtual Camera (type:Cinemachine) that will be used when player aims")]
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;

    [Tooltip("Projectile prefab reference goes here")]
    [SerializeField] private Transform pfBulletProjectile;

    [Tooltip("Point from which to launch the projectile (ex:gun muzzle)")]
    [SerializeField] private Transform spawnBulletPosition;

    [Header("Debug")]
    [Tooltip("Show a sphere at the point where aim ray hits")]
    [SerializeField] private Transform debugTransform;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        //Find the point on a surface that the player is aiming with the reticle
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);

            //Set the Aim animation to 1 on the 2nd animation layer
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);

            //Set the Aim animation to 0 on the 2nd animation layer
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        if (starterAssetsInputs.shoot)
        {
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;

            //This creates a physical projectile that hits the target, use a different method if you want hitscan
            Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            starterAssetsInputs.shoot = false;
        }
    }
}
