using Unity.VisualScripting;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    [Header("Targeting Layers")]
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
    }
}
