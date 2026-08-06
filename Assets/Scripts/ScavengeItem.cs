using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScavengeItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] string itemName = "Item Name";
    [Range(1, 10)][SerializeField] int quantity;
    [Range(1, 10)][SerializeField] float interactRange;

    [Header("References")]
    [SerializeField] Transform playerCamera;
    [SerializeField] TextMeshProUGUI interactionPromptText;
    [SerializeField] string PromptMessage = "Message";

    // TODO: Added player inventory Reference 


    void Start()
    {
        // TODO: Added player inventory

        // Ensure promt id hidden at start
        if(interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null) return;
        
            float distance = Vector3.Distance(transform.position, playerCamera.position);
            if (distance <= interactRange)
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, interactRange))
                {
                    if (hit.transform == transform)
                    {
                        if (interactionPromptText != null)
                        {
                            interactionPromptText.gameObject.SetActive(true);
                            interactionPromptText.text = PromptMessage + " " + itemName;

                        }
                    if (Input.GetKeyDown(KeyCode.E)) CollectItem();

                    return;
                    }
                }
            }

            if(interactionPromptText != null && distance > interactRange) 
                interactionPromptText.gameObject.SetActive(false);
    }

    void CollectItem()
    {
        Debug.Log("Collected: " + itemName);
        // TODO: Add to player's inventory/tracker script when we build it 

        // Hide UI after collected
        if(interactionPromptText != null ) interactionPromptText.gameObject.SetActive(false);

        // Destroy the world object once picked up
        Destroy(gameObject);
    }
}
