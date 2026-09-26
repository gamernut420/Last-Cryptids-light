using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    [Header("Movement")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.75f;

    [Header("Fade")]
    [SerializeField] private float fadeStartTime = 0.5f;

    [Header("Damage Number Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private FontStyles normalStyle = FontStyles.Normal;

    [SerializeField] private float criticalScale = 1.5f;
    [SerializeField] private Color critColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private FontStyles criticalStyle = FontStyles.Bold;

    private float timer;
    private Color textColor;
    private Camera playerCamera;

    private void Start()
    {
        playerCamera = Camera.main;
        textColor = damageText.color;
    }

    public void SetDamage(int damage, bool isCritical)
    {
        damageText.text = damage.ToString();

        if (isCritical)
        {
            damageText.transform.localScale = Vector3.one * criticalScale;
            damageText.color = critColor;
            damageText.fontStyle = criticalStyle;
        }
        else
        {
            damageText.transform.localScale = Vector3.one * normalScale;
            damageText.color = normalColor;
            damageText.fontStyle = normalStyle;
        }

        textColor = damageText.color;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (playerCamera != null)
        {
            transform.forward = playerCamera.transform.forward;
        }

        if (timer >= fadeStartTime)
        {
            float fadeTime = lifetime - fadeStartTime;
            if (fadeTime > 0f)
            {
                float alpha = 1f - ((timer - fadeStartTime) / fadeTime);
                Color fadeColor = textColor;
                fadeColor.a = alpha;
                damageText.color = textColor;
            }
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
