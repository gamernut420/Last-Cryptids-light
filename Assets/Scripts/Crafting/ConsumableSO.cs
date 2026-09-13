using UnityEngine;

/// <summary>
/// Types of effects a consumable can apply.
/// </summary>
public enum ConsumableEffectType
{
    Heal,
    ReduceFear,
    RestoreStamina,
    MovementSpeed,
    AirPurification
}

/// <summary>
/// Defines a consumable item.
///
/// Examples:
/// - Health Potion
/// - Adrenaline Shot
/// - Anti-Fear Medication
/// </summary>
[CreateAssetMenu(
    fileName = "NewConsumable",
    menuName = "Operation Riftfall/Items/Consumable")]
public class ConsumableSO : ItemSO
{
    [Header("Consumable Effect")]

    [SerializeField]
    private ConsumableEffectType effectType;

    [SerializeField]
    private float effectAmount;

    [SerializeField]
    private float duration;


    public ConsumableEffectType EffectType => effectType;

    public float EffectAmount => effectAmount;

    public float Duration => duration;
}
