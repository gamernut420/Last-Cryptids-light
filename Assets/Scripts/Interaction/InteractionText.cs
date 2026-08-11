using TMPro;
using UnityEngine;

public class InteractionText : MonoBehaviour
{
    TextMeshProUGUI interationText;

    private void OnEnable()
    {
        interationText = GetComponent<TextMeshProUGUI>();

        PlayerInteraction.UpdateScreenText += UpdateText;
    }

    private void OnDisable()
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
            interationText.text = "E To " + input;
        }
    }
}
