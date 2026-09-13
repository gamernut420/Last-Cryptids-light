using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    private const string DifficultyKey = "SelectedDifficulty";
    private static DifficultyManager instance;

    [Header("Difficulty")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("Multipliers")]
    [SerializeField] private float easyMultiplier = 0.5f;
    [SerializeField] private float normalMultiplier = 1f;
    [SerializeField] private float hardMultiplier = 1.5f;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDifficulty();
    }

    public static DifficultyManager GetInstance()
    {
        return instance;
    }

    public void SetEasyDifficulty()
    {
        SetDifficulty(DifficultyLevel.Easy);
    }

    public void SetNormalDifficulty()
    {
        SetDifficulty(DifficultyLevel.Normal);
    }

    public void SetHardDifficulty()
    {
        SetDifficulty(DifficultyLevel.Hard);
    }

    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        PlayerPrefs.SetInt(DifficultyKey, (int)currentDifficulty);
        PlayerPrefs.Save();

        Debug.Log($"Difficulty changed to {GetDifficultyName()} ({GetDifficultyMultiplier():0.0}x).");
    }

    public void LoadDifficulty()
    {
        int savedDifficulty = PlayerPrefs.GetInt(
            DifficultyKey,
            (int)DifficultyLevel.Normal);

        currentDifficulty = (DifficultyLevel)Mathf.Clamp(
            savedDifficulty,
            (int)DifficultyLevel.Easy,
            (int)DifficultyLevel.Hard);
    }

    public DifficultyLevel GetCurrentDifficulty()
    {
        return currentDifficulty;
    }

    public string GetDifficultyName()
    {
        return currentDifficulty.ToString();
    }

    public float GetDifficultyMultiplier()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                return easyMultiplier;

            case DifficultyLevel.Hard:
                return hardMultiplier;

            default:
                return normalMultiplier;
        }
    }

    public float GetScaledEnemyHealth(float baseHealth)
    {
        return Mathf.Max(1f, baseHealth * GetDifficultyMultiplier());
    }

    public float GetScaledEnemySpeed(float baseSpeed)
    {
        return Mathf.Max(0f, baseSpeed * GetDifficultyMultiplier());
    }

    public int GetScaledEnemyDamage(int baseDamage)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseDamage * GetDifficultyMultiplier()));
    }

    public int GetScaledEnemyCount(int baseCount)
    {
        if (baseCount <= 0)
        {
            return 0;
        }

        return Mathf.Max(
            1,
            Mathf.CeilToInt(baseCount * GetDifficultyMultiplier()));
    }
}
