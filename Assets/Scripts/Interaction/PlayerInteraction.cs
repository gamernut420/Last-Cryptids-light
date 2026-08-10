using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float InteractionRange = 5f;
    [SerializeField] float InteractionSize = 0.1f;
    [SerializeField] LayerMask IngoreLayer = 0;
    [SerializeField] LayerMask InteractionIndex = 0;
    [SerializeField] bool DebugInteractionTraces = false;
    [SerializeField] bool DebugInteractionLogs = false;

    public static System.Action<string> UpdateScreenText;

    GameObject player;

    Vector3 boxTraceLocation;

    Collider[] boxHits = new Collider[4];

    IInteract interactable = null;

    private void Start()
    {
        player = gameObject;
    }

    private void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void CheckForInteractable()
    {
        Vector3 startPoint = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        RaycastHit lineHit;

        if(Physics.Raycast(startPoint, rayDirection, out lineHit, InteractionRange, ~IngoreLayer))
        {
            boxTraceLocation = lineHit.point;

            Vector3 boxSize = new Vector3(InteractionSize, InteractionSize, InteractionSize);

            interactable = null;

            foreach (Collider hit in Physics.OverlapBox(boxTraceLocation, boxSize, Quaternion.identity, InteractionIndex))
            {
                interactable = hit.GetComponent<IInteract>();

                if(interactable != null)
                {
                    break;
                }
            }

            if (interactable != null && UpdateScreenText != null)
            {
                UpdateScreenText(interactable.ScreenMessage());
            }
            else
            {
                UpdateScreenText(null);
            }
        }

        if (DebugInteractionTraces)
        {
            Debug.DrawRay(startPoint, rayDirection * InteractionRange, Color.red);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxTraceLocation, new Vector3(InteractionSize, InteractionSize, InteractionSize));
    }

    void TryInteract()
    {
        if (interactable != null)
        {
            interactable.Interact(player);
        }
    }
}
