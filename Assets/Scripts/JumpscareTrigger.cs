using System.Collections;
using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject jumpscareCanvas;
    [SerializeField] private AudioSource screamAudio;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;

    private bool hasTrigger = false;

    private void Awake()
    {
        
        if(jumpscareCanvas == null)
        {
            GameObject canvasObj = GameObject.FindGameObjectWithTag("JumpscareUI");
            if(canvasObj != null )
            {
                jumpscareCanvas = canvasObj;
            }
            
        }
        if(jumpscareCanvas != null )
                jumpscareCanvas.SetActive(false);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!hasTrigger && other.CompareTag("Player"))
        {
            hasTrigger = true;
            StartCoroutine(TriggerJumpscareRoutine());

        }
    }
   private IEnumerator TriggerJumpscareRoutine()
    {
        if (jumpscareCanvas != null)
            jumpscareCanvas.SetActive(true);
        if(screamAudio  != null)
            screamAudio.Play();
        yield return new WaitForSeconds(displayDuration);

        if(jumpscareCanvas != null)
            jumpscareCanvas.SetActive(false);
    }

}
