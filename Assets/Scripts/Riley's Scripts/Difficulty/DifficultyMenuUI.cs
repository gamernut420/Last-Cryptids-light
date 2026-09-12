using UnityEngine;
using TMPro;

public class DifficultyMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI difficultyText;

    public void OnEnable()
    {
        RefreshDifficultyText();
    }

    public void SelectEasy()
    {
        DifficultyManager manager = DifficultyManager.GetInstance();
        if (manager != null)
        {
            manager.SetEasyDifficulty();
        }

        RefreshDifficultyText();
    }

    public void SelectNormal()
    {
        DifficultyManager manager = DifficultyManager.GetInstance();
        if (manager != null)
        {
            manager.SetNormalDifficulty();
        }

        RefreshDifficultyText();
    }

    public void SelectHard()
    {
        DifficultyManager manager = DifficultyManager.GetInstance();
        if (manager != null)
        {
            manager.SetHardDifficulty();
        }

        RefreshDifficultyText();
    }

    public void RefreshDifficultyText()
    {
        if (difficultyText == null)
        {
            return;
        }

        DifficultyManager manager = DifficultyManager.GetInstance();
        difficultyText.text = manager == null
            ? "Difficulty: Normal (1.0x)"
            : $"Difficulty: {manager.GetDifficultyName()} " +
              $"({manager.GetDifficultyMultiplier():0.0}x)";
    }
}
