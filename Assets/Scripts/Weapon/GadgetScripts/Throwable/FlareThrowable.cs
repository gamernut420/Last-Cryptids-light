using UnityEngine;
using UnityEngine.AI;

public class FlareThrowable : ThrowableGadget
{
    [SerializeField] float EffectSize = 10;

    public override void Impact(RaycastHit hit)
    {
        transform.position = hit.point;

        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();

        obstacle.shape = NavMeshObstacleShape.Capsule;

        obstacle.radius = EffectSize;
    }
}
