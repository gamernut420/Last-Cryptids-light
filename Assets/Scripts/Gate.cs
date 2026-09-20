using UnityEngine;

public class GateActivator : MonoBehaviour
{
    [SerializeField] private GameObject gate1;
    [SerializeField] private GameObject gate2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gate1.SetActive(true);
            gate2.SetActive(true);
        }
    }
}
