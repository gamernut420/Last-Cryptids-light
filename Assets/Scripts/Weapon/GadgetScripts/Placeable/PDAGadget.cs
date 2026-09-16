using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PDAGadget : PlaceableGadget
{
    [SerializeField][Min(0)] float EffectDuration = 2.5f;
    [SerializeField][Min(0)] float EffectSize = 10;

    public override bool UseGadget(GameObject player)
    {
        if (PlaceGadget())
        {
            StartCoroutine(CreateSafeZone());

            return true;
        }

        return false;
    }

    IEnumerator CreateSafeZone()
    {
        gameObject.AddComponent<SafeZoneTrigger>();

        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();

        obstacle.shape = NavMeshObstacleShape.Capsule;

        obstacle.radius = EffectSize;

        SphereCollider safeZone = gameObject.AddComponent<SphereCollider>();

        safeZone.isTrigger = true;

        safeZone.radius = EffectSize;

        yield return new WaitForSeconds(EffectDuration);

        Destroy(gameObject);
    }
}
