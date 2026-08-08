using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomPathFinder : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Movement Settings")]
    [Range(20f, 40f)] [SerializeField] float minPatrolRadius;
    [Range(1f, 5f)] [SerializeField] float arrivalDistance;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
