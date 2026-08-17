using System.Collections;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float InteractionRange = 5f;
    [Tooltip("Size of the box that traces for items")]
    [SerializeField] float InteractionSize = 0.1f;
    [SerializeField] LayerMask IgnoreLayer = 8;
    [SerializeField] bool DebugInteractionTraces = false;

    public static System.Action<string> UpdateScreenText;

    Vector3 boxSize;

    GameObject player;

    Vector3 boxTraceLocation;

    IInteract interactable = null;
    bool isHolding;

    private void Start()
    {
        boxSize = new Vector3(InteractionSize, InteractionSize, InteractionSize);

        player = gameObject;
    }

    private void Update()
    {
        CheckForInteractable();

        if (Input.GetKey(KeyCode.E))
        {
            TryInteract();
        }
        else
        {
            if (isHolding)
            {
                isHolding = false;
                interactable.StopHold();
            }

            interactable = null;
        }
    }

    void CheckForInteractable()
    {
        Vector3 startPoint = Camera.main.transform.position;
        Vector3 rayEnd = Camera.main.transform.position + (Camera.main.transform.forward * InteractionRange);

        RaycastHit lineHit;

        if(Physics.Linecast(startPoint, rayEnd, out lineHit, ~IgnoreLayer))
        {
            boxTraceLocation = lineHit.point;
        }
        else
        {
            boxTraceLocation = rayEnd;
        }

        IInteract tempInteract = null;


        foreach (Collider hit in Physics.OverlapBox(boxTraceLocation, boxSize, Quaternion.identity))
        {
            tempInteract = hit.GetComponent<IInteract>();

            if (tempInteract != null)
            {
                break;
            }
        }

        if(tempInteract == null && isHolding && interactable != null)
        {
            isHolding = false;
            interactable.StopHold();
        }

        interactable = tempInteract;

        if (UpdateScreenText != null)
        {
            if (interactable != null)
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
            Debug.DrawLine(startPoint, rayEnd, Color.red);
        }
    }

    private void OnDrawGizmos()
    {
        if (DebugInteractionTraces)
        {
            Gizmos.DrawWireCube(boxTraceLocation, new Vector3(InteractionSize, InteractionSize, InteractionSize));
        }
    }

    void TryInteract()
    {
        if (interactable != null)
        {
            isHolding = true;
            if (interactable.DoHold())
            {
                interactable.Interact(player);
            }
        }
    }
}
