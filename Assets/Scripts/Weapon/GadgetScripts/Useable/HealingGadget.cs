using UnityEngine;

public class HealingGadget : UseableGadget
{
    [SerializeField][Min(0)] int HealAmmount = 5;
    [SerializeField] bool OverHeal = false;

    public override bool UseGadget(IPlayer player)
    {
        if(player.HealPlayer(HealAmmount, OverHeal))
        {
            Destroy(gameObject);

            return true;
        }

        return false;
    }
}
