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

    private float timer;
    private Color textColor;
    private Camera playerCamera;

    private void Start()
    {
        playerCamera = Camera.main;
        textColor = damageText.color;
    }

    public void SetDamage(int damage)
    {
        damageText.text = damage.ToString();
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
            float alpha = 1f - ((timer - fadeStartTime) / fadeTime);
            textColor.a = alpha;
            damageText.color = textColor;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
