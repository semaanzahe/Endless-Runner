using System;
using UnityEngine;

public class MagBox : MonoBehaviour
{
    private PlayerMovement player;
    private void Start()
    {
        player = GetComponentInParent<PlayerMovement>();
    }

    private void OnCollisionEnter(Collision other)
    {
        player.TryCollectCoin(other.gameObject);
    }
    
    
}
