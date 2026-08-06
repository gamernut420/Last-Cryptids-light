using UnityEngine;
using System.Collections.Generic;

public class ScavengeItem : MonoBehaviour
{
    [SerializeField] string itemName;
    [SerializeField] float interactRange;
    [SerializeField] Transform playerCamera;

    // Update is called once per frame
    void Update()
    {
        if (playerCamera != null)
        {
            float distance = Vector3.Distance(transform.position, playerCamera.position);
            if (distance <= interactRange && Input.GetKeyDown(KeyCode.E)) CollectItem();
        }
    }

    void CollectItem()
    {
        Debug.Log("Collected: " + itemName);
        // TODO: Add to player's inventory/tracker script when we build it 

        // Destroy the world object once picked up
        Destroy(gameObject);
    }
}
