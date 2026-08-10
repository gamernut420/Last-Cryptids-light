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
    Vector3 normalLocalPosition;
    Vector3 aimLocation;
    Quaternion normalRotaion;

    [Header("Recoil")]
    [SerializeField] Camera PlayerCamera;
    [SerializeField] float VerticleRecoil;
    [SerializeField] float HorizontalRecoil;

    [Header("VFX")]
    [SerializeField] GameObject Muzzle;
    [SerializeField] GameObject[] Flashes;
    [SerializeField] float SwayAmmount = 10;

    [Header("Audio")]
    [SerializeField] AudioSource gunAudio;
    [SerializeField] AudioClip gunShootSound;

    //Event speakers
    public static System.Action<float> ShotFired;
    public static System.Action<int> CurrentAmmoChanged;
    public static System.Action<int> ReserveAmmoChanged;

    //Shooting Variables
    ProjectileManager projectileManager;
    bool canShoot;
    bool tryingShoot;
    int currentAmmo;
    int currentReserveAmmo;



    private void Start()
    {
        projectileManager = gameObject.GetComponent<ProjectileManager>();
        currentAmmo = MagSize;
        currentReserveAmmo = MaxReserveAmmo;
        canShoot = true;
        tryingShoot = false;

        normalLocalPosition = transform.localPosition;
        normalRotaion = transform.localRotation;

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
            StartCoroutine(Shootgun());
        }
    }

    IEnumerator Shootgun()
    {
        projectileManager.ShootProjectile(Muzzle.transform.position, Muzzle.transform.rotation, BulletData);

        if (gunAudio != null && gunShootSound != null)
        {
            gunAudio.PlayOneShot(gunShootSound);
        }

        if (ShotFired != null)
        {
            ShotFired(SpreadAmmount);
        }

        MuzzleFlash();

        DetermainRecoil();

        canShoot = false;

        currentAmmo--;

        yield return new WaitForSeconds(FireRate);

        if (CurrentAmmoChanged != null)
        {
            CurrentAmmoChanged(currentAmmo);
        }

        if (currentAmmo > 0)
        {
            canShoot = true;
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

        canShoot = true;
    }

    void DetermainRecoil()
    {
        float randomHorizontalRecoil = Random.Range(-HorizontalRecoil, HorizontalRecoil);

        Vector2 recoil = new Vector2(VerticleRecoil, randomHorizontalRecoil);

        transform.localPosition -= Vector3.forward * 0.1f;

        transform.localRotation = Quaternion.Euler(-recoil.x, recoil.y, 0);

        float camRotX = PlayerCamera.transform.localRotation.x;

        camRotX += recoil.x;

        camRotX = Mathf.Clamp(camRotX, -90, 90);

        //Something else is needed to modify verticle recoil
        //PlayerCamera.transform.localRotation = Quaternion.Euler(recoil.x, 0, 0);

        PlayerCamera.transform.parent.Rotate(0, recoil.y, 0);

    }

    void MuzzleFlash()
    {
        GameObject flash = Instantiate(Flashes[Random.Range(0, Flashes.Length)], Muzzle.transform.position, Muzzle.transform.rotation, Muzzle.transform);

        Destroy(flash, 0.1f);
    }

    //This is also used to reset the weapons rotation
    void DetermineAim()
    {
        Vector3 target = normalLocalPosition;

        if (Input.GetMouseButton(1))
        {
            target = aimLocation;
        }

        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, target, AimSmoothing * Time.deltaTime);

        Quaternion desiredRotaion = Quaternion.Lerp(transform.localRotation, normalRotaion, AimSmoothing * Time.deltaTime);

        transform.localPosition = desiredPosition;

        transform.localRotation = desiredRotaion;
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

            ReserveAmmoChanged(currentReserveAmmo);

            return true;
        }
    }
}
