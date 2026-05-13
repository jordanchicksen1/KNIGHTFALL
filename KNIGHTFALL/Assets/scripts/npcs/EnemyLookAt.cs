using UnityEngine;

public class EnemyLookAt : MonoBehaviour
{
    private Transform player;

    [Header("Detection")]
    public float detectionRange = 8f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= detectionRange)
        {
            LookAtPlayer();
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0;

        Quaternion targetRotation =
            Quaternion.LookRotation(-direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}