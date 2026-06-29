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

    public GameObject controls;
    public GameObject DeathScreen;
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("Run", 1);
        }
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCollectCoin(collision.gameObject);

        if (collision.gameObject.tag == "Platform"||collision.gameObject.tag == "Box")
        {
            onGround = true;
        }

        if (collision.gameObject.CompareTag("KillBox")&&!IsInvinsible)
        {
            CanvasManager.instance.Death();
        }
        else if(collision.gameObject.CompareTag("KillBox") && IsInvinsible)
        {
            GameObject.FindWithTag("Box").SetActive(false);
            IsInvinsible = false;
        }
    }

    private void TryCollectCoin(GameObject otherObject)
    {
        if (otherObject == null || !otherObject.activeSelf || !otherObject.CompareTag("Coin")) return;

        otherObject.SetActive(false);
        if (hud != null)
        {
            hud.AddCoin();
        }
    }
    
    
}
