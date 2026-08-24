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
    public static System.Action<bool> ShowHold;
    public static System.Action<float> UpdateHold;

    Vector3 boxSize;

    Vector3 boxTraceLocation;

    IInteract interactable = null;
    bool isHolding;

    private void Start()
    {
        boxSize = new Vector3(InteractionSize, InteractionSize, InteractionSize);
    }

    private void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            if (isHolding)
            {
                isHolding = false;
                interactable.StopHold();

                ShowHold?.Invoke(isHolding);
            }

            interactable = null;
        }

        if (isHolding)
        {
            TryInteract();
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

            ShowHold?.Invoke(isHolding);
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

            ShowHold?.Invoke(isHolding);

            float holdAmmount = interactable.DoHold();

            UpdateHold?.Invoke(holdAmmount);

            if (holdAmmount >= 1)
            {
                isHolding = false;

                ShowHold?.Invoke(isHolding);

                interactable.Interact(gameObject);
            }
        }
    }
}
