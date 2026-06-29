using System;
using UnityEngine;

public enum PowerUpsEnum
{
    PowerUp1,
    PowerUp2,
}
[CreateAssetMenu(menuName = "Collectables/PowerUP")]
public class PowerUp : Collectables
{
    public PowerUpsEnum powerUp;

    public override void ApplyPowerUP(GameObject target)
    {
        
        switch (powerUp)
        {
            case PowerUpsEnum.PowerUp1:
                Debug.Log("PowerUp 1");
                break;
            case PowerUpsEnum.PowerUp2:
                Debug.Log("PowerUp 2");
                break;
        }
    }
}
