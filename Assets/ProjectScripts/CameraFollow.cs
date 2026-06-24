using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform playerTransform;
    private float startedZ;

    private void Awake()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            enabled = false;
            return;
        }

        playerTransform = playerObject.transform;
        startedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            playerTransform.position.z + startedZ
        );
    }
}
