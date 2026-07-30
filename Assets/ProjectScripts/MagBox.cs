using System.Collections;
using UnityEngine;

public class MagBox : MonoBehaviour
{
    public static MagBox instance;
    private PlayerMovement player;
    private Collider magCollider;

    public bool canCollect = false;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        player = GetComponentInParent<PlayerMovement>();
        
        
        magCollider = GetComponent<Collider>();
        
        SetMagnetActive(false);
    }

    public void ApplyPowerUp(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ApplyPowerUpTimer(duration));
    }

    IEnumerator ApplyPowerUpTimer(float duration)
    {
        SetMagnetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        SetMagnetActive(false);
    }

    private void SetMagnetActive(bool active)
    {
        canCollect = active;
        
        if (magCollider != null)
        {
            // Disabling the collider turns off the trigger zone completely when inactive
            magCollider.enabled = active; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only triggers when magCollider.enabled is true AND hits a coin
        if (canCollect && other.CompareTag("Coin"))
        {
            if (player != null)
            {
                player.TryCollectCoin(other.gameObject);  
            }
        }
    }
}