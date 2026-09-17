using System.Collections;
using UnityEngine;

public class Purifier : UseableGadget
{
    [SerializeField][Min(2.5f)] float BlockingTimer = 2.5f;

    ExposureSystem Exposure;

    public override bool UseGadget(GameObject player)
    {
        Exposure = player.GetComponent<ExposureSystem>();
        
        StartCoroutine(CreateSafeZone());

        GetComponent<MeshRenderer>().enabled = false;

        return true;
    }

    IEnumerator CreateSafeZone()
    {
        Exposure.SetBlockingStatus(true);

        yield return new WaitForSeconds(BlockingTimer);

        gameManager.instance.ShowExposurePrompt();

        Exposure.SetBlockingStatus(false);

        Destroy(gameObject);
    }
}
