using UnityEngine;

//this class handles the code used by all of the placeable gadgets
//derive from this class to make a custom placeable
public abstract class PlaceableGadget : GadgetBase
{
    public override bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if (player != null)
        {
            player.PlayerAddItem(gameObject);

            gameObject.GetComponent<Collider>().enabled = false;

            return true;
        }

        return false;
    }

    public bool PlaceGadget()
    {
        RaycastHit hit;

        Vector3 traceStart = Camera.main.transform.position;
        Vector3 traceEnd = traceStart + (Camera.main.transform.forward * 5);

        Debug.DrawLine(traceStart, traceEnd, Color.red, 10);

        if(Physics.Linecast(traceStart, traceEnd, out hit, 1))
        {
            Debug.Log($"Hit at {hit.point}");

            transform.SetParent(null);

            transform.position = hit.point;

            gameObject.GetComponent<Collider>().enabled = true;

            float posOffset = gameObject.GetComponent<Collider>().bounds.extents.y;

            gameObject.GetComponent<Collider>().enabled = false;

            transform.position += new Vector3(0, posOffset, 0);

            transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);

            return true;
        }

        return false;
    }
}
