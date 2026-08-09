using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] AudioSource gunAudio;
    [SerializeField] AudioClip gunShootSound;
    [Min(0f)] public float FireRate = 0.5f;
    [Min(1)] public int MagSize = 30;
    [Min(1)] public int MaxReserveAmmo = 120;
    public ProjectileData BulletData;

    [Header("VFX")]
    public GameObject Muzzle;
    public GameObject[] Flashes;
    public float SwayAmmount = 10;

    [Header("Aim")]
    public GameObject AimingObject;
    public float AimSmoothing = 10;
    Vector3 normalLocalPosition;
    Vector3 aimLocation;
    Quaternion normalRotaion;

    [Header("Recoil")]
    public Camera PlayerCamera;
    public float VerticleRecoil;
    public float HorizontalRecoil;

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
            Debug.LogFormat("Ammo: {0}, Reserve: {1}", currentAmmo, currentReserveAmmo);
            int ammoNeeded = MagSize - currentAmmo;

            if (ammoNeeded < currentReserveAmmo)
            {
                Debug.Log("Had spare ammo");
                currentAmmo = MagSize;

                currentReserveAmmo -= ammoNeeded;
            }
            else
            {
                Debug.Log("No more spare ammo");
                currentAmmo += currentReserveAmmo;

                currentReserveAmmo = 0;
            }

            canShoot = true;

            Debug.LogFormat("Current Ammo: {0}, Current reserve: {1}", currentAmmo, currentReserveAmmo);
        }

        if (tryingShoot && canShoot)
        {
            StartCoroutine(Shootgun());
        }
    }

    IEnumerator Shootgun()
    {
        projectileManager.ShootProjectile(Muzzle.transform.position, Muzzle.transform.rotation, BulletData);

        if(gunAudio !=null && gunShootSound != null)
        {
            gunAudio.PlayOneShot(gunShootSound);
        }

        MuzzleFlash();

        DetermainRecoil();

        canShoot = false;

        currentAmmo--;

        yield return new WaitForSeconds(FireRate);

        Debug.LogFormat("Ammo Left: {0}", currentAmmo);

        if (currentAmmo > 0)
        {
            canShoot = true;
        }
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

        Debug.LogFormat("Verticle Recoil For cam: {0}", camRotX);

        PlayerCamera.transform.localRotation = Quaternion.Euler(recoil.x, 0, 0);

        PlayerCamera.transform.parent.Rotate(0, recoil.y, 0);

    }

    void MuzzleFlash()
    {
        GameObject flash = Instantiate(Flashes[Random.Range(0, Flashes.Length)], Muzzle.transform.position, Muzzle.transform.rotation, Muzzle.transform);

        Destroy(flash, 0.1f);
    }

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
}
