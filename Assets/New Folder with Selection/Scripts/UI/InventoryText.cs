using TMPro;
using UnityEngine;

public class InventoryText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI inventory;

    private void OnEnable()
    {
        inventory = gameObject.GetComponent<TextMeshProUGUI>();

        PlayerInventory.UpdateInventoryText += UpdateText;

        inventory.text = string.Empty;
    }

    private void OnDisable()
    {
        PlayerInventory.UpdateInventoryText -= UpdateText;
    }

    void UpdateText(string text)
    {
        inventory.text = text;
    }
}
