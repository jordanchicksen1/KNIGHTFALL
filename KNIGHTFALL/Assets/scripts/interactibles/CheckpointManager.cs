using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public Transform currentCheckpoint;
    public Transform startingCheckpoint;

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
        currentCheckpoint = startingCheckpoint;
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

        playerHealth.isDead = false;

        Debug.Log("Checkpoint Position: " + currentCheckpoint.position);
        Debug.Log("Player Position Before: " + playerHealth.transform.position);

        // Clear any active interaction before respawning.
        PlayerInteraction interaction =
            playerHealth.GetComponent<PlayerInteraction>();

        if (interaction != null)
        {
            interaction.currentInteractable = null;
            InteractionUI.Instance.HidePrompt();
        }

        // Temporarily disable the CharacterController.
        CharacterController controller =
            playerHealth.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        // Teleport the player to the checkpoint.
        playerHealth.transform.position = currentCheckpoint.position;

        // Re-enable the CharacterController.
        if (controller != null)
            controller.enabled = true;

        Debug.Log("Player Position After: " + playerHealth.transform.position);

        if (controller != null)
            controller.enabled = true;

        playerHealth.health = playerHealth.GetEffectiveMaxHealth();
        playerHealth.stamina = playerHealth.maxStamina;
        playerHealth.mp = playerHealth.maxMP;

        PlayerMovement movement = playerHealth.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.currentState = PlayerState.Idle;
            movement.StopAllCoroutines();
            movement.ResetMovement();
        }


        PlayerItems items = playerHealth.GetComponent<PlayerItems>();

        if (items != null)
        {
            items.RefillFlasks();
            items.StopAllCoroutines();
        }

        playerHealth.StopAllCoroutines();

      

        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.RespawnEnemy();
        }

        Debug.Log("Final Respawn Position: " + playerHealth.transform.position);

    }
}