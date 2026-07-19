using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public Transform currentCheckpoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    public void RespawnPlayer(PlayerHealth playerHealth)
    {
        if (currentCheckpoint == null)
        {
            Debug.LogWarning("No checkpoint has been activated!");
            return;
        }

        Debug.Log("Checkpoint Position: " + currentCheckpoint.position);
        Debug.Log("Player Position Before: " + playerHealth.transform.position);

        CharacterController controller =
            playerHealth.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        playerHealth.transform.position =
            currentCheckpoint.position;

        Debug.Log("Player Position After: " + playerHealth.transform.position);

        if (controller != null)
            controller.enabled = true;

        playerHealth.health = playerHealth.maxHealth;
        playerHealth.stamina = playerHealth.maxStamina;
        playerHealth.mp = playerHealth.maxMP;

        PlayerMovement movement = playerHealth.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.currentState = PlayerState.Idle;
        }


        PlayerItems items = playerHealth.GetComponent<PlayerItems>();

        if (items != null)
        {
            items.RefillFlasks();
        }

        EnemySpawner[] spawners =
            FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.RespawnEnemy();
        }

        Debug.Log("Player Respawned");
    }
}