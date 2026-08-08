using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float FireCooldown;

    [SerializeField] bool isAutomatic;

    [SerializeField] ProjectileData projData;

    [SerializeField] GameObject Muzzle;

    private ProjectileManager projManager;

    private float CurrentCooldown = 0;

    private bool repeatshoot = false;

    private void Awake()
    {
        projManager = gameObject.GetComponent<ProjectileManager>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (isAutomatic)
        {
            if (Input.GetMouseButton(0))
            {
                if (CurrentCooldown <= 0f)
                {
                    projManager.ShootProjectile(Muzzle.transform.position, gameObject.transform.rotation, projData);
                    CurrentCooldown = FireCooldown;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CurrentCooldown <= 0f)
                {
                    projManager.ShootProjectile(Muzzle.transform.position, gameObject.transform.rotation, projData);
                    CurrentCooldown = FireCooldown;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            repeatshoot = !repeatshoot;
        }

        if (repeatshoot)
        {
            if (CurrentCooldown <= 0f)
            {
                projManager.ShootProjectile(Muzzle.transform.position, gameObject.transform.rotation, projData);
                CurrentCooldown = FireCooldown;
            }
        }

        CurrentCooldown -= Time.deltaTime;
    }
}
