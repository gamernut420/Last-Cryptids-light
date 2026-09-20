using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class JunkPile : MonoBehaviour, IInteract
{
    [System.Serializable]
    public struct LootItem
    {
        public ScriptableItem itemData;
        public int minQuantity;
        public int maxQuantity;


    }
    [Header("Junk Loot Table")]
    [SerializeField] private List<LootItem> possibleLoot;
    [SerializeField] private int minUniqueItemsToPull = 1;
    [SerializeField] private int maxUniqueItemsToPull = 3;
   
    [Header("State")]
    [SerializeField] private bool canBeSearchMultipleTimes = false;
    private bool hasBeenSearched = false;
    [SerializeField][Min(0)] float HoldTimer;

    [Header("References")]
    [SerializeField] string PromptMessage = "Press E to pick up";

    float currentHoldTimer;
    string currentPropmt;

    private void Start()
    {
        currentHoldTimer = 0;
        currentPropmt = PromptMessage;
    }
    public void SearchJunkPile()
    {
        if(hasBeenSearched && !canBeSearchMultipleTimes)
        {
            Debug.Log("This junk pile has already been scavehged.");
            return;
        }
        if(possibleLoot == null || possibleLoot.Count == 0 || PlayerInventory.Instance == null)
        {
            Debug.LogWarning("Junk pile loot table is empty or PlayerInventory is missing.");
            return;
        }
        
        // Determine how many different types of items to roll (e.g., between 1 and 3 types)
        int itemToFind = Random.Range(minUniqueItemsToPull, maxUniqueItemsToPull + 1);
        List<LootItem> availableLoot = new List<LootItem>(possibleLoot);

        for(int i = 0; i < itemToFind && availableLoot.Count > 0; i++)
        {
            // Pick a random item from the remaining pool so we don't get duplicates in the same search
            int randomIndex = Random.Range(0, availableLoot.Count);
            LootItem selectedLoot = availableLoot[randomIndex];
            availableLoot.RemoveAt(randomIndex);

            // Roll a random quantily for this specific item (e.g., between 1 and 3)
            int quantity = Random.Range(selectedLoot.minQuantity, selectedLoot.maxQuantity);

            // Add directly to the PlayerInventory singleton
            PlayerInventory.Instance.AddItem(selectedLoot.itemData, quantity);

            Debug.Log($"Scavenged {quantity}x {selectedLoot.itemData.itemName} form junk pile.");
        }
        hasBeenSearched = true;
    }

    private int LootCountSafe(List<LootItem> list) => list.Count;

    public bool Interact(GameObject interactor)
    {
        // return true if a search will happen now
        bool willSearch = !hasBeenSearched || canBeSearchMultipleTimes;
        if (willSearch)
            SearchJunkPile();
        return willSearch;
    }

    public float DoHold()
    {
        currentHoldTimer += Time.deltaTime;

        currentHoldTimer = Mathf.Clamp(currentHoldTimer, 0, HoldTimer);

        if (HoldTimer == 0)
        {
            return 1;
        }

        return currentHoldTimer / HoldTimer;
    }

    public void StopHold()
    {
        currentPropmt = PromptMessage;

        currentHoldTimer = 0;
    }

    public string ScreenMessage()
    {
        // message shown on UI when player looks at this object
        if (!hasBeenSearched || canBeSearchMultipleTimes)
            return "Search junk pile";
        return "Already scavenged";
    }
}
