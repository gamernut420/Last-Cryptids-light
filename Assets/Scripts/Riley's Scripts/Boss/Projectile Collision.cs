using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void StartMoving()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }

    /*[Header("Targeting Layers")]
    public LayerMask groundLayers;

    [Header("Effects (Optional)")]
    public GameObject impactEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            HandleImpact(collision.contacts[0].point);
        }
    }

    private void HandleImpact(Vector3 impactPoint)
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, impactPoint, Quaternion.identity);
        }

        Destroy(gameObject);
    }*/
}
