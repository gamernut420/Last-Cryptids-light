using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]

    [SerializeField]
    private GameObject bossHealthRoot;

    [SerializeField]
    private Image healthFill;

    [SerializeField]
    private TextMeshProUGUI bossNameText;


    [Header("Boss Information")]

    [SerializeField]
    private string bossName = "Rift Warden";


    private void OnEnable()
    {
        // ADDED FOR BOSS HEALTH BAR:
        FinalBoss.BossHealthChanged += UpdateHealthBar;

        // ADDED FOR BOSS HEALTH BAR:
        FinalBoss.BossFightStarted += ShowBossHealth;

        // ADDED FOR BOSS HEALTH BAR:
        FinalBoss.BossDefeated += HideBossHealth;
    }


    private void OnDisable()
    {
        // ADDED FOR BOSS HEALTH BAR:
        FinalBoss.BossHealthChanged -= UpdateHealthBar;
        FinalBoss.BossFightStarted -= ShowBossHealth;
        FinalBoss.BossDefeated -= HideBossHealth;
    }


    private void Start()
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }


        // ADDED FOR BOSS HEALTH BAR:
        // Keep the bar hidden until the boss fight begins.
        if (bossHealthRoot != null)
        {
            bossHealthRoot.SetActive(false);
        }
    }


    private void ShowBossHealth(
        float currentHealth,
        float maxHealth)
    {
        if (bossHealthRoot != null)
        {
            bossHealthRoot.SetActive(true);
        }


        UpdateHealthBar(
            currentHealth,
            maxHealth
        );
    }


    private void UpdateHealthBar(
        float currentHealth,
        float maxHealth)
    {
        if (healthFill == null ||
            maxHealth <= 0f)
        {
            return;
        }


        healthFill.fillAmount =
            Mathf.Clamp01(
                currentHealth / maxHealth
            );
    }


    private void HideBossHealth()
    {
        if (bossHealthRoot != null)
        {
            bossHealthRoot.SetActive(false);
        }
    }
}