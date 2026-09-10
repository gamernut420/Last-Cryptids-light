using UnityEngine;

//this class handles the code used by all of the throwable gadgets
//derive from this class to make a custom throwable
public abstract class ThrowableGadget : GadgetBase, IProjectile
{
    [SerializeField][Min(0f)] float ThrowSpeed = 50;
    [SerializeField] float Gravity = 1;
    [SerializeField][Min(0f)] float LifeTime = 10;

    ProjectileManager projectileManager;

    public override bool UseGadget(IPlayer player)
    {
        if(player != null)
        {
            projectileManager = player.GetProjectileManager();

            if(projectileManager == null)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        Vector3 start = Camera.main.transform.position;

        Quaternion rotation = Camera.main.transform.rotation;

        projectileManager.ShootObject(start, rotation, ThrowSpeed, Gravity, LifeTime, this);

        return true;
    }

    public void UpdatePos(Vector3 pos)
    {
        transform.position = pos;
    }

    public abstract void Impact(RaycastHit hit);

    public override bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if(player != null)
        {
            player.PlayerAddItem(gameObject);

            gameObject.GetComponent<Collider>().enabled = false;

            return true;
        }

        return false;
    }
}
