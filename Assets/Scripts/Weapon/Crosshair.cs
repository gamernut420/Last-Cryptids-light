using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField][Min(0)] float baseScale = 10f;
    [SerializeField] float SpreadToScale = 0.1f;
    [SerializeField] float MaxSpreadSize = 2.5f;
    [SerializeField] float SpreadReturnSpeed = 10f;
    [SerializeField] float ReturnDelay = 0.25f;

    float spreadScale;
    float returnDelayTimer;

    private void OnEnable()
    {
        returnDelayTimer = ReturnDelay;
        GunController.ShotFired += OnSHotFired;
    }

    private void OnDisable()
    {
        GunController.ShotFired -= OnSHotFired;
    }

    private void Update()
    {
        returnDelayTimer -= Time.deltaTime;

        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.transform.GetChild(0).GetComponent<RectTransform>().localPosition = Vector3.up * (baseScale + spreadScale);
        }

        if(returnDelayTimer <= 0)
        {
            spreadScale = Mathf.Lerp(spreadScale, 0, SpreadReturnSpeed * Time.deltaTime);
        }
    }

    void OnSHotFired(float spreadAmmount)
    {
        returnDelayTimer = ReturnDelay;

        float spread = 1f + spreadAmmount * SpreadToScale;

        spreadScale = Mathf.Clamp(Mathf.Max(spreadScale, spread), 0, MaxSpreadSize) * 10;
    }
}
