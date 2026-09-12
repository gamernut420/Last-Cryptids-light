using UnityEngine;

public class TestPlaceableGadget : PlaceableGadget
{
    public override bool UseGadget(IPlayer player)
    {
        if (PlaceGadget())
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphere.GetComponent<Collider>().enabled = false;

            sphere.transform.localScale = new Vector3(25, 25, 25);

            sphere.transform.position = transform.position;

            return true;
        }

        return false;
    }
}
