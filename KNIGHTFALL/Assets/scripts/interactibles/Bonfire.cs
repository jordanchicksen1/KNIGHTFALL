using UnityEngine;

public class Bonfire : Interactable
{
    [Header("Checkpoint")]
    public Transform spawnPoint;

    private PlayerHealth playerHealth;
    private PlayerItems playerItems;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerItems = player.GetComponent<PlayerItems>();
        }
    }

    public override void Interact()
    {
        Debug.Log("Rested at Bonfire");

        // Different checkpoint?
        bool isNewCheckpoint = CheckpointManager.Instance.currentCheckpoint != spawnPoint;

        // Save checkpoint
        CheckpointManager.Instance.SetCheckpoint(spawnPoint);

        if (isNewCheckpoint)
        {
            playerHealth.vitality += playerHealth.vitalityRestore;
            playerHealth.vitality = Mathf.Min(playerHealth.vitality,playerHealth.maxVitality);
        }

        Debug.Log("Checkpoint Saved: " + spawnPoint.position);

        // Restore player
        playerHealth.health = playerHealth.GetEffectiveMaxHealth();
        playerHealth.stamina = playerHealth.maxStamina;
        playerHealth.mp = playerHealth.maxMP;
        playerItems.RefillFlasks();
        Debug.Log(playerItems.currentHealingFlasks);

        // Respawn every enemy
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.RespawnEnemy();
        }
    }
}