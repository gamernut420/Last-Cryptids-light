using System.Collections;
using UnityEngine;

public class AdrenalineStim : UseableGadget
{
    [SerializeField][Min(1f)] float EffectDuration = 2;
    [SerializeField][Min(1f)] float SpeedMult = 1.2f;

    public override bool UseGadget(GameObject player)
    {
        StartCoroutine(ActivateStim());

        return true;
    }

    IEnumerator ActivateStim()
    {
        gameManager.instance.playerScript.SetStimulantMode(true, SpeedMult);

        gameObject.GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(EffectDuration);

        gameManager.instance.playerScript.SetStimulantMode(false, 1);

        Destroy(gameObject);
    }
}
