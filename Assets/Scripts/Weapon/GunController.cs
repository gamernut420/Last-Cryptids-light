using UnityEngine;

public class GunController : MonoBehaviour, IWeapon
{
    [Header("Gun Settings")]
    [Min(0f)] public float FireRate = 0.5f;
    [Min(1)] public int MagSize = 30;
    [Min(1)] public int MaxReserveAmmo = 120;
    public ProjectileData BulletData;

    [Header("Aim")]
    public GameObject AimingObject;
    public float AimSmoothing = 10;
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
    int currentAmmo;
    int currentReserveAmmo;
    float shootTimer;


    private void Start()
    {
        projectileManager = gameObject.GetComponent<ProjectileManager>();
        currentAmmo = MagSize;
        currentReserveAmmo = MaxReserveAmmo;
        canShoot = true;
        shootTimer = 0f;

        normalLocalPosition = transform.localPosition;
        normalRotaion = transform.localRotation;

        aimLocation = AimingObject.transform.localPosition * -1;
    }

    private void Update()
    {
        DetermineAim();

        WeaponSway();

        if (shootTimer > 0f)
        { 
        shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f && currentAmmo > 0)
            {
                canShoot = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < MagSize && currentReserveAmmo > 0)
        {
            Debug.LogFormat("Ammo: {0}, Reserve: {1}", currentAmmo, currentReserveAmmo);
            int ammoNeeded = MagSize - currentAmmo;

            if (ammoNeeded <= currentReserveAmmo)
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

       if(Input.GetMouseButtonDown(0) && canShoot && currentAmmo >0)
        {
            Shootgun();
        }
    }

    void Shootgun()
    {
        

        if (currentAmmo <=0)
        {
            return;
        }

        canShoot=false;
        shootTimer = FireRate;
        currentAmmo--;
        Debug.LogFormat("Shot Fired!: {0}, Reserve{1}", currentAmmo, currentReserveAmmo);

        projectileManager.ShootProjectile(Muzzle.transform.position, Muzzle.transform.rotation, BulletData);

        if (gunAudio != null && gunShootSound != null)
        {
            gunAudio.PlayOneShot(gunShootSound);
        }

        if (ShotFired != null)
        {
            ShotFired(currentAmmo);
        }

        MuzzleFlash();
        DetermainRecoil();

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
