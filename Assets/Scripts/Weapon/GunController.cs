using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour, IWeapon
{
    [Header("Gun Settings")]
    [SerializeField][Min(0f)] float FireRate = 0.5f;
    [SerializeField][Min(1)] int MagSize = 30;
    [SerializeField][Min(1)] int MaxReserveAmmo = 120;
    [SerializeField][Min(0)] float SpreadAmmount = 0;
    [SerializeField] ProjectileData BulletData;

    [Header("Aim")]
    [SerializeField] GameObject AimingObject;
    [SerializeField] float AimSmoothing = 10;
    
    [Header("Recoil")]
    [SerializeField] Camera PlayerCamera;
    [SerializeField] float VerticleRecoil;
    [SerializeField] float HorizontalRecoil;

    ICamera camera;

    [Header("VFX")]
    [SerializeField] GameObject Muzzle;
    [SerializeField] GameObject[] Flashes;
    [SerializeField] float SwayAmmount = 10;

    [Header("Audio")]
    [SerializeField] AudioSource gunAudio;
    [SerializeField] AudioClip gunShootSound;
    public float gunshotHearingRadius = 20f;

    //Event speakers
    public static System.Action<float> SendReticleSpread;
    public static System.Action<int> CurrentAmmoChanged;
    public static System.Action<int> ReserveAmmoChanged;

    //Shooting Variables
    ProjectileManager projectileManager;
    bool canShoot;
    bool tryingShoot;
    bool isShooting;
    int currentAmmo;
    int currentReserveAmmo;

    //Position variables
    Vector3 basePosition;
    Quaternion baseRotation;
    Vector3 activePosition;
    Quaternion activeRotation;
    Vector3 aimLocation;


    private void Start()
    {
        projectileManager = gameObject.GetComponent<ProjectileManager>();
        currentAmmo = MagSize;
        currentReserveAmmo = MaxReserveAmmo;
        canShoot = true;
        tryingShoot = false;
        isShooting = false;

        camera = PlayerCamera.GetComponent<ICamera>();

        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;

        aimLocation = AimingObject.transform.localPosition * -1;

        if (CurrentAmmoChanged != null)
        {
            CurrentAmmoChanged(currentAmmo);
        }

        if (ReserveAmmoChanged != null)
        {
            ReserveAmmoChanged(currentReserveAmmo);
        }
    }

    private void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            DetermineAim();

            WeaponSway();

            if (Input.GetMouseButtonDown(0))
            {
                tryingShoot = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                tryingShoot = false;
            }

            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < MagSize && currentReserveAmmo > 0)
            {
                Reload();
            }

            if (tryingShoot && canShoot)
            {
                StartCoroutine(ShootGun());
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    NoiseManager.MakeNoise(transform.position, gunshotHearingRadius);
                }
            }
        }
    }

    IEnumerator ShootGun()
    {
        projectileManager.ShootProjectile(Muzzle.transform.position, Muzzle.transform.rotation, BulletData);

        if (gunAudio != null && gunShootSound != null)
        {
            gunAudio.PlayOneShot(gunShootSound);
        }

        if (SendReticleSpread != null)
        {
            SendReticleSpread(SpreadAmmount);
        }

        MuzzleFlash();

        DetermainRecoil();

        canShoot = false;

        isShooting = true;

        currentAmmo--;

        if (CurrentAmmoChanged != null)
        {
            CurrentAmmoChanged(currentAmmo);
        }

        yield return new WaitForSeconds(FireRate);

        isShooting = false;

        CheckAmmo();
    }

    void CheckAmmo()
    {
        if(currentAmmo > 0 && !isShooting)
        {
            canShoot = true;
        }
        else
        {
            canShoot = false;
        }
    }

    void Reload()
    {
        int ammoNeeded = MagSize - currentAmmo;

        if (ammoNeeded < currentReserveAmmo)
        {
            currentAmmo = MagSize;

            currentReserveAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += currentReserveAmmo;

            currentReserveAmmo = 0;
        }

        if (CurrentAmmoChanged != null)
        {
            CurrentAmmoChanged(currentAmmo);
        }

        if (ReserveAmmoChanged != null)
        {
            ReserveAmmoChanged(currentReserveAmmo);
        }

        CheckAmmo();
    }

    void DetermainRecoil()
    {
        float randomHorizontalRecoil = Random.Range(-HorizontalRecoil, HorizontalRecoil);

        transform.localPosition -= Vector3.forward * 0.1f;

        transform.localRotation = Quaternion.Euler(-VerticleRecoil, randomHorizontalRecoil, 0);

        float camRotX = PlayerCamera.transform.localRotation.x;

        camera.ModifyCameraPitch(VerticleRecoil);
        camera.ModifyCameraYaw(randomHorizontalRecoil);
    }

    void MuzzleFlash()
    {
        GameObject flash = Instantiate(Flashes[Random.Range(0, Flashes.Length)], Muzzle.transform.position, Muzzle.transform.rotation, Muzzle.transform);

        Destroy(flash, 0.1f);
    }

    //This is also used to reset the weapons rotation
    void DetermineAim()
    {
        Vector3 target = basePosition;

        if (Input.GetMouseButton(1))
        {
            target = aimLocation;
        }

        activePosition = Vector3.Lerp(transform.localPosition, target, AimSmoothing * Time.deltaTime);

        activeRotation = Quaternion.Lerp(transform.localRotation, baseRotation, AimSmoothing * Time.deltaTime);

        transform.localPosition = activePosition;

        transform.localRotation = activeRotation;
    }

    void WeaponSway()
    {
        Vector2 mouseAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        transform.localPosition += (Vector3)mouseAxis * SwayAmmount / 1000;
    }

    public bool WeaponRefillAmmo(int amount)
    {
        if (currentReserveAmmo >= MaxReserveAmmo)
        {
            return false;
        }
        else
        {
            currentReserveAmmo = Mathf.Clamp(currentReserveAmmo + amount, 0, MaxReserveAmmo);

            if(ReserveAmmoChanged != null)
            {
                ReserveAmmoChanged(currentReserveAmmo);
            }

            return true;
        }
    }
}
