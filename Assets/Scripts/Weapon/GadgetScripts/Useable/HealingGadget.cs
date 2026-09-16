using UnityEngine;

public class HealingGadget : UseableGadget
{
    [SerializeField][Min(0)] int HealAmmount = 25;
    [SerializeField] bool OverHeal = false;

    public override bool UseGadget(GameObject player)
    {
        IPlayer pInterface = player.GetComponent<IPlayer>();

        if(pInterface.HealPlayer(HealAmmount, OverHeal))
        {
            Destroy(gameObject);

            return true;
        }

        return false;
    }
}
