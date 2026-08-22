using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour, IWeapon, IInteract
{
    [SerializeField] GameObject WeaponModel;

    [Header("Gun Stats")]
    [SerializeField][Min(0f)] float FireRate = 0.5f;
    [SerializeField][Min(1)] int MagSize = 30;
    [SerializeField][Min(1)] int MaxReserveAmmo = 120;
    [SerializeField][Range(0, 90)] float SpreadAmmount = 0;
    [SerializeField] ProjectileData BulletData;

    [Header("Aim")]
    [SerializeField] GameObject AimObject;
    [SerializeField] float AimSmoothing = 10;

    Vector3 aimPoint;
    bool isAiming;

    [Header("Recoil")]
    [SerializeField] float VerticleRecoil;
    [SerializeField] float HorizontalRecoil;

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
    public static System.Action<bool> ChangedAim;
    public static System.Action<int> CurrentAmmoChanged;
    public static System.Action<int> ReserveAmmoChanged;

    //Object Variables
    ProjectileManager projectileManager;
    IPlayer owningPlayer;
    ICamera playerCamera;

    //Checks
    bool isInUse;

    //Shooting Variables
    bool canShoot;
    bool tryingShoot;
    bool isShooting;

    //Ammo Variables
    int currentAmmo;
    int currentReserveAmmo;

    //Position variables
    Vector3 basePosition;
    Quaternion baseRotation;
    Vector3 activePosition;
    Quaternion activeRotation;

    private void OnValidate()
    {
        CheckComponents();

        GetComponent<MeshFilter>().sharedMesh = WeaponModel.GetComponent<MeshFilter>().sharedMesh;

        GetComponent<MeshRenderer>().sharedMaterials = WeaponModel.GetComponent<MeshRenderer>().sharedMaterials;
    }

    void CheckComponents()
    {
        if (GetComponent<ProjectileManager>() == null)
        {
            projectileManager = gameObject.AddComponent<ProjectileManager>();
        }
        else
        {
            projectileManager = GetComponent<ProjectileManager>();
        }

        if (GetComponent<AudioSource>() == null)
        {
            gunAudio = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            gunAudio = GetComponent<AudioSource>();
        }

        if (GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }

        if(GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }

        if (gameObject.transform.Find("Muzzle") == null && Muzzle == null)
        {
            Muzzle = new GameObject("Muzzle");

            Muzzle.transform.SetParent(transform);

            Muzzle.transform.localPosition = Vector3.zero;
        }

        if (gameObject.transform.Find("AimPoint") == null && AimObject == null)
        {
            AimObject = new GameObject("AimPoint");

            AimObject.transform.SetParent(transform);

            AimObject.transform.localPosition = Vector3.zero;
        }
    }

    private void Start()
    {
        isInUse = false;

        currentAmmo = MagSize;
        currentReserveAmmo = MaxReserveAmmo;
        canShoot = true;
        tryingShoot = false;
        isShooting = false;

        isAiming = false;

        basePosition = Vector3.zero;
        baseRotation = Quaternion.identity;

        aimPoint = AimObject.transform.localPosition * -1;

        if (CurrentAmmoChanged != null)
        {
            CurrentAmmoChanged(currentAmmo);
        }

        if (ReserveAmmoChanged != null)
        {
            ReserveAmmoChanged(currentReserveAmmo);
        }
    }

    public void SetPlayerVariables(IPlayer player = null, ICamera camera = null, Vector3 gripLocation = default)
    {
        owningPlayer = player;
        playerCamera = camera;

        basePosition = gripLocation;

        transform.localPosition = basePosition;
    }

    public void SetWeaponUse(bool inUse)
    {
        isInUse = inUse;
    }

    private void Update()
    {
        if (!gameManager.instance.isPaused && isInUse)
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

            if (Input.GetMouseButtonDown(1))
            {
                isAiming = true;

                if (ChangedAim != null)
                {
                    ChangedAim(true);
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isAiming = false;

                if (ChangedAim != null)
                {
                    ChangedAim(false);
                }
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
        for(int i = 0; i < BulletData.Gauge; i++)
        {
            Quaternion bulletRotation = Muzzle.transform.rotation;

            float spreadMod = SpreadAmmount;

            if (isAiming)
            {
                spreadMod -= spreadMod * BulletData.SpreadReduction;
            }

            float yaw = Random.Range(-spreadMod, spreadMod) + bulletRotation.x;
            float pitch = Random.Range(-spreadMod, spreadMod) + bulletRotation.y;

            bulletRotation = Quaternion.Euler(yaw, pitch, bulletRotation.z);

            projectileManager.ShootProjectile(Muzzle.transform.position, bulletRotation, BulletData);
        }

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

        if(playerCamera != null)
        {
            playerCamera.ModifyCameraPitch(VerticleRecoil);
            playerCamera.ModifyCameraYaw(randomHorizontalRecoil);
        }
    }

    void MuzzleFlash()
    {
        if(Flashes.Length > 0)
        {
            GameObject flash = Instantiate(Flashes[Random.Range(0, Flashes.Length)], Muzzle.transform.position, Muzzle.transform.rotation, Muzzle.transform);

            Destroy(flash, 0.1f);
        }
    }

    //This is also used to reset the weapons rotation
    void DetermineAim()
    {
        Vector3 target = basePosition;

        if (isAiming)
        {
            target = aimPoint;
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

    public bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if (player != null)
        {
            player.PlayerAddWeapon(gameObject);

            return true;
        }

        return false;
    }

    public string ScreenMessage()
    {
        return "Pickup Weapon";
    }
}
