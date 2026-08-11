using TMPro;
using UnityEngine;

public class InteractionText : MonoBehaviour
{
    //This is used to append something like "Press E to" to "Pickup item"
    [SerializeField] string PreText = "Press E to ";

    TextMeshProUGUI interationText;

    private void OnEnable()
    {
        interationText = GetComponent<TextMeshProUGUI>();

        PlayerInteraction.UpdateScreenText += UpdateText;
    }

    private void OnDestroy()
    {
        PlayerInteraction.UpdateScreenText -= UpdateText;
    }

    void UpdateText(string input)
    {
        if (input == null || input == "")
        {
            interationText.text = input;
        }
        else
        {
            interationText.text = PreText + input;
        }
    }
}
