using UnityEngine;

public class DifficultySelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject difficultyPanel;

    public void OpenDifficultyPanel()
    {
        if (difficultyPanel != null)
        {
            difficultyPanel.transform.SetAsLastSibling();
            difficultyPanel.SetActive(true);
        }
    }

    public void CloseDifficultyPanel()
    {
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(false);
        }
    }
}
