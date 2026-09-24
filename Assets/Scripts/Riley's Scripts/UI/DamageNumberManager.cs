using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager instance;

    [SerializeField] private GameObject damageNumberPrefab;
    
    private void Awake()
    {
        instance = this;
    }
    
    public void ShowDamage(Vector3 position, int damage)
    {
        if (damageNumberPrefab == null)
            return;

        GameObject number = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        DamageNumber damageNumber = number.GetComponent<DamageNumber>();
        damageNumber.SetDamage(damage);
    }
}
