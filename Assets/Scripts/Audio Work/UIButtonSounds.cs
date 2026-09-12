using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, ISubmitHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            UISoundPlayer.Instance?.PlayHover();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable && eventData.button == PointerEventData.InputButton.Left)
        {
            UISoundPlayer.Instance?.PlayClick();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (button != null && button.interactable)
        {
            UISoundPlayer.Instance?.PlayClick();
        }
    }
}
