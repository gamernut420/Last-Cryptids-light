using UnityEngine;

public interface IProjectile
{
    public void UpdatePos(Vector3 pos);

    public void Impact(RaycastHit hit);
}
