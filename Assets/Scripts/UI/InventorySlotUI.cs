using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public enum SlotType
    {
        Weapon,
        Crafted
    }

    [Header("Item Display")]
    public Image iconImage;
    public TextMeshProUGUI stackSizeText;

    // ADDED FOR FUTURISTIC HUD:
    [Header("HUD Display")]
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image selectionFrame;
    [SerializeField] private Image slotGlow;

    // ADDED FOR FUTURISTIC HUD:
    [Header("Slot Settings")]
    [SerializeField] private SlotType slotType;
    [SerializeField] private int slotNumber = 1;

    // ADDED FOR FUTURISTIC HUD:
    [Header("HUD Colors")]
    [SerializeField]
    private Color normalFrameColor =
        new Color(0.24f, 0.72f, 0.9f, 0.55f);

    [SerializeField]
    private Color selectedFrameColor =
        new Color(0.45f, 0.9f, 1f, 1f);

    [SerializeField]
    private Color weaponSlotColor =
        new Color(0.24f, 0.72f, 0.9f, 0.8f);

    [SerializeField]
    private Color craftedSlotColor =
        new Color(0.61f, 0.36f, 1f, 0.8f);


    public ScriptableItem CurrentItem
    {
        get;
        private set;
    }


    public int CurrentQuantity
    {
        get;
        private set;
    }


    public bool IsEmpty =>
        CurrentItem == null;


    public SlotType Type =>
        slotType;


    public int SlotNumber =>
        slotNumber;


    private void Awake()
    {
        RefreshSlotNumber();
        ApplySlotTheme();
        SetSelected(false);
    }


    public void ConfigureSlot(
        int number,
        SlotType type)
    {
        slotNumber = number;
        slotType = type;

        RefreshSlotNumber();
        ApplySlotTheme();
    }


    public void SetItem(
        ScriptableItem item,
        int quantity)
    {
        CurrentItem = item;
        CurrentQuantity = quantity;


        if (item == null)
        {
            ClearSlot();
            return;
        }


        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.enabled = true;
            iconImage.sprite = item.itemIcon;
        }


        if (itemNameText != null)
        {
            itemNameText.text =
                item.itemName;

            itemNameText.gameObject.SetActive(true);
        }


        if (stackSizeText != null)
        {
            if (quantity > 1)
            {
                stackSizeText.gameObject.SetActive(true);
                stackSizeText.enabled = true;
                stackSizeText.text =
                    quantity.ToString();
            }
            else
            {
                stackSizeText.text = "";
                stackSizeText.gameObject.SetActive(false);
                stackSizeText.enabled = false;
            }
        }
    }


    public void ClearSlot()
    {
        CurrentItem = null;
        CurrentQuantity = 0;


        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
            iconImage.enabled = false;
        }


        if (stackSizeText != null)
        {
            stackSizeText.text = "";
            stackSizeText.gameObject.SetActive(false);
            stackSizeText.enabled = false;
        }


        if (itemNameText != null)
        {
            itemNameText.text = "";
            itemNameText.gameObject.SetActive(false);
        }
    }


    public void UpdateSlot(
        Sprite itemImage,
        string itemText,
        int itemCount)
    {
        if (iconImage != null)
        {
            if (itemImage != null)
            {
                iconImage.enabled = true;
                iconImage.gameObject.SetActive(true);
                iconImage.sprite =
                    itemImage;
            }
            else
            {
                iconImage.enabled = false;
                iconImage.gameObject.SetActive(false);
            }
        }


        if (itemNameText != null)
        {
            itemNameText.text =
                itemText ?? "";

            itemNameText.gameObject.SetActive(
                !string.IsNullOrEmpty(itemText)
            );
        }


        if (stackSizeText != null)
        {
            if (itemCount > 1)
            {
                stackSizeText.enabled = true;
                stackSizeText.gameObject.SetActive(true);
                stackSizeText.text =
                    itemCount.ToString();
            }
            else
            {
                stackSizeText.text = "";
                stackSizeText.enabled = false;
                stackSizeText.gameObject.SetActive(false);
            }
        }
    }


    // ADDED FOR FUTURISTIC HUD:
    public void SetSelected(
        bool selected)
    {
        if (selectionFrame != null)
        {
            selectionFrame.color =
                selected ?
                selectedFrameColor :
                normalFrameColor;
        }


        if (slotGlow != null)
        {
            slotGlow.gameObject.SetActive(
                selected
            );
        }
    }


    private void RefreshSlotNumber()
    {
        if (slotNumberText == null)
        {
            return;
        }


        slotNumberText.text =
            slotNumber.ToString();
    }


    private void ApplySlotTheme()
    {
        if (selectionFrame == null)
        {
            return;
        }


        if (slotType ==
            SlotType.Weapon)
        {
            selectionFrame.color =
                weaponSlotColor;
        }
        else
        {
            selectionFrame.color =
                craftedSlotColor;
        }
    }
}