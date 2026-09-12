using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionText : MonoBehaviour
{
    //This is used to append something like "Press E to" to "Pickup item"
    [SerializeField] string PreText;
    [SerializeField] Image RadialWheel;
    [SerializeField] Image FillWheel;

    TextMeshProUGUI interationText;

    private void OnValidate()
    {
        interationText = GetComponent<TextMeshProUGUI>();

        interationText.text = $"{PreText}(Interaction Text)";
    }

    private void OnEnable()
    {
        interationText = GetComponent<TextMeshProUGUI>();

        PlayerInteraction.UpdateScreenText += UpdateText;
        PlayerInteraction.ShowHold += ShowHoldWheel;
        PlayerInteraction.UpdateHold += UpdateHold;
    }

    private void OnDestroy()
    {
        PlayerInteraction.UpdateScreenText -= UpdateText;
        PlayerInteraction.ShowHold -= ShowHoldWheel;
        PlayerInteraction.UpdateHold -= UpdateHold;
    }

    void UpdateText(string input = "")
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

    void ShowHoldWheel(bool show)
    {
        interationText.enabled = !show;
        RadialWheel.enabled = show;
        FillWheel.enabled = show;
    }

    void UpdateHold(float fillAmmount)
    {
        FillWheel.fillAmount = fillAmmount;
    }
}
