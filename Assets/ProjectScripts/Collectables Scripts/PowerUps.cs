using System;
using UnityEngine;

public enum PowerUpsEnum
{
    Invincible,
    Double_money,
}
[CreateAssetMenu(menuName = "Collectables/PowerUP")]
public class PowerUp : Collectables
{
    public PowerUpsEnum powerUp;

    public override void ApplyPowerUP(GameObject target)
    {
        
        switch (powerUp)
        {
            case PowerUpsEnum.Invincible:
                Debug.Log("PowerUp 1");
                break;
            case PowerUpsEnum.Double_money:
                Debug.Log("PowerUp 2");
                break;
        }
    }
}
