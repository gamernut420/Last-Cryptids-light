using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Targeting Layers")]
    public LayerMask destroyLayers;


    [Header("Effects (Optional)")]
    public GameObject impactEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & destroyLayers) != 0)
        {
            HandleImpact(other.ClosestPoint(transform.position));
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
