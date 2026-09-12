using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class FearSystem : MonoBehaviour
{

   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    void IncreaseFear(float amount)
    {
      
    }
    void DecreaseFear(float amount)
    {
        
    }
   
    void TriggerPanicState()
    {
        gameManager GameManager = FindAnyObjectByType<gameManager>();
    }
}
