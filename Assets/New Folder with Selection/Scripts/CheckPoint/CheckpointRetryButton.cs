using UnityEngine;

public class CheckpointRetryButton : MonoBehaviour
{
    public void LoadCheckpoint()
    {
        Debug.Log("Try Again button clicked.", this);

        if (gameManager.instance == null)
        {
            Debug.LogError("Try Again: Game Manager wasn't found.", this); return ;
        }
       gameManager.instance.LoadCheckpoint();
    }
}
