using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FlareThrowable : ThrowableGadget
{
    [SerializeField][Min(0)] float EffectDuration = 2.5f;
    [SerializeField][Min(0)] float EffectSize = 10;

    public override void Impact(RaycastHit hit)
    {
        transform.position = hit.point;

        StartCoroutine(CreateSafeZone());
    }

    IEnumerator CreateSafeZone()
    {
        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();

        obstacle.shape = NavMeshObstacleShape.Capsule;

        obstacle.radius = EffectSize;

        yield return new WaitForSeconds(EffectDuration);

        Destroy(gameObject);
    }
}
