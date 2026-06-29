using UnityEngine;
[DisallowMultipleComponent]

public class PowerUpsTrigger : MonoBehaviour
{
    public PowerUp powerUp;
    private PowerUpsPoolingSystem powerUpsPool;

    public void SetPool(PowerUpsPoolingSystem pool)
    {
        powerUpsPool = pool;
    }

    public void ReturnToPool()
    {
        if (powerUpsPool != null)
        {
            powerUpsPool.ReturnPowerUp(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (powerUpsPool == null)
        {
            powerUpsPool = GetComponentInParent<PowerUpsPoolingSystem>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        if (powerUp != null)
        {
            powerUp.ApplyPowerUP(other.gameObject);
        }

        ReturnToPool();
    }
}
