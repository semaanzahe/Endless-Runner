using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public float jumpSpeed = 2.0f;

    public Rigidbody rd;

    private Animator animator;

    private const float laneStep = 2.5f;
    private const float minLaneX = -2.5f;
    private const float maxLaneX = 2.5f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat("Run", 1);
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
        rd.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    }
}
