using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public float jumpSpeed = 2.0f;

    public Rigidbody rd;

    private Animator animator;
    
    private bool onGround;

    private const float laneStep = 2.5f;
    private const float minLaneX = -2.5f;
    private const float maxLaneX = 2.5f;

    public Hud hud;
    
    private bool IsInvinsible;

    
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("Run", 1);
        }

        IsInvinsible = false;
    }

    private void FixedUpdate()
    {
        transform.position += Vector3.forward * (Time.fixedDeltaTime * speed);
    }

    public void MoveRight()
    {
        MoveByLane(laneStep);
    }

    public void MoveLeft()
    {
        MoveByLane(-laneStep);
    }

    private void MoveByLane(float laneOffset)
    {
        float targetX = Mathf.Clamp(transform.position.x + laneOffset, minLaneX, maxLaneX);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
    
    public void Jump()
    {
        if (onGround)
        {
            rd.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
            onGround = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        TryCollectCoin(other.gameObject);
        //KillBox(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCollectCoin(collision.gameObject);
        KillBox(collision.gameObject);
        IsOnGround(collision.gameObject);
    }

    void IsOnGround(GameObject ground)
    {
        if (ground.CompareTag("Platform") || ground.CompareTag("Box"))
        {
            onGround = true;
        }
    }
    
    void KillBox(GameObject box)
    {
        Debug.Log($"KillBox called on {box.name}, invincible={IsInvinsible}");
        if (box.CompareTag("KillBox"))
        {
            if (!IsInvinsible)
            {
                // Player dies when not invincible
                CanvasManager.instance.Death();
                AudioManager.Instance.PlaySFX3D(SFXType.GameOver, transform.position);
            }
            else
            {
                box.SetActive(false);
            }
        }
    }
    public void TryCollectCoin(GameObject otherObject)
    {
        if (otherObject == null || !otherObject.activeSelf || !otherObject.CompareTag("Coin")) return;

        otherObject.SetActive(false);
        if (hud != null)
        {
            AudioManager.Instance.PlaySFX3D(SFXType.CoinPickup, transform.position,2);
            hud.AddCoin();
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddProgress(MissionType.CoinsCollected, 1);
            }
        }
    }
    public void ApplyPowerUp(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ApplyPowerUpTimer(duration));
    }

    IEnumerator ApplyPowerUpTimer(float duration)
    {
        Debug.Log("Invinsible");
        IsInvinsible = true;

        yield return new WaitForSeconds(duration);

        IsInvinsible = false;
    }
    

}
