using UnityEngine;

public class WeaponUpgrades : MonoBehaviour
{
    [Header("----- Increase Ammounts -----")]
    [SerializeField][Min(0f)] float DamageIncrease = 10f;
    [SerializeField][Min(0f)] float VelocityIncrease = 25f;
    [SerializeField][Min(0f)] float FireRateIncrease = 25f;
    [SerializeField][Min(0f)] int MaxAmmoIncrease = 5;
    [SerializeField][Min(0f)] int MagAmmoIncrease = 5;
}
