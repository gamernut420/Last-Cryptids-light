using Unity.Mathematics;
using UnityEngine;

public class Doors : MonoBehaviour, IInteract
{
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private string openMessage = "Press E to open the door";
    [SerializeField] private string closeMessage = "Press E to close the door";
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;

        private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen;



    void Awake()
    {
        if (doorHinge == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }
        closedRotation = doorHinge.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.RotateTowards(doorHinge.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public bool Interact(GameObject interactor)
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            PlayDoorSound(doorOpenSound);
        }
        else
        {
            PlayDoorSound(doorCloseSound);
        }
        return true;
    }

    public string ScreenMessage()
    {
        return isOpen ? closeMessage : openMessage;
    }
    private void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource == null || clip == null) return;
        doorAudioSource.PlayOneShot(clip);
    }



}
