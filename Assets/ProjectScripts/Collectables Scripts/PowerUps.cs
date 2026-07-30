using System;
using UnityEngine;

public enum PowerUpsEnum
{
    Invincible,
    Magnet,
}
[CreateAssetMenu(menuName = "Collectables/PowerUP")]
public class PowerUp : Collectables
{
    public PowerUpsEnum powerUp;

    public override void ApplyPowerUP(GameObject target)
    {
        // 1. Notify UI Manager to show or update the slider UI
        if (PowerUpUIManager.Instance != null)
        {
            PowerUpUIManager.Instance.ShowPowerUpUI(powerUp, collectableName, collectableDuration);
        }

        // 2. Apply the actual gameplay effect to the player
        switch (powerUp)
        {
            case PowerUpsEnum.Invincible:
                PlayerMovement player = target.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.ApplyPowerUp(collectableDuration);
                }
                break;

            case PowerUpsEnum.Magnet:
                if (MagBox.instance != null)
                {
                    Debug.Log("magnet");
                    MagBox.instance.ApplyPowerUp(collectableDuration);
                }
                break;
        }
    }
}
