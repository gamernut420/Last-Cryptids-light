using UnityEngine;

public class TestThrowable : ThrowableGadget
{
    public override void Impact(RaycastHit hit)
    {
        transform.position = hit.point;

        Debug.Log($"Sucsesfully hit at {hit.point}");
    }
}
