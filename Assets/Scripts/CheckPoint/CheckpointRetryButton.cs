using UnityEngine;

public class CheckpointRetryButton : MonoBehaviour
{
    public void LoadCheckpoint()
    {
        if (gameManager.instance == null)
        {
            Debug.LogError("Try Again: Game Manager wasn't found.", this); return ;
        }
       // gameManager.instance.LoadCheckpoint();
    }
}
