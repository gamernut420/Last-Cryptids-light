using UnityEngine;
using System.Collections;
using TMPro;

public class InteractionPromptUI
{
    private static TextMeshProUGUI activeLabel;
    private static Object activeOwner;

    public static void Show(TextMeshProUGUI label, Object owner, string message)
    {
        if (label == null || owner == null) return;
        if (activeLabel != null && activeLabel != label) activeLabel.gameObject.SetActive(false);
        activeLabel = label;
        activeOwner = owner;
        label.text = message;
        label.gameObject.SetActive(true);
    }

    public static void Hide(TextMeshProUGUI label, Object owner)
    {
        if (label == null || owner == null) return;
        if (activeLabel == label && activeOwner == owner)
        {
            label.gameObject.SetActive(false);
            activeLabel = null;
            activeOwner = null;
        }
    }


}
