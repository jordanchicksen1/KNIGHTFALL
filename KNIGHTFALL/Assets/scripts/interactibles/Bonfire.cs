using UnityEngine;
using System.Collections;

public class Bonfire : Interactable
{

    [Header("Checkpoint")]
    public Transform spawnPoint;

    [Header("Bonfire Flame")]
    public GameObject fireModel;
    private GameObject flameRestoredText;

    private PlayerHealth playerHealth;
    private PlayerItems playerItems;

    private bool flameRestored = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "FlameRestoredText")
            {
                flameRestoredText = obj;
                break;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerItems = player.GetComponent<PlayerItems>();
        }

        if (flameRestoredText != null)
        {
            flameRestoredText.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Bonfire could not find FlameRestoredText in the scene.");
        }

        interactionText = "Light";
    }

    public override void Interact()
    {
        if (!flameRestored)
        {
            LightBonfire();
            return;
        }

        RestAtBonfire();
    }

    private void LightBonfire()
    {
        flameRestored = true;

        if (fireModel != null)
        {
            fireModel.SetActive(true);
        }

        if (flameRestoredText != null)
        {
            flameRestoredText.SetActive(true);
            StartCoroutine(HideFlameRestoredText());
        }

        interactionText = "Rest";

        Debug.Log("Bonfire Lit!");
    }

    private void RestAtBonfire()
    {
        Debug.Log("Rested at Bonfire");

        // Different checkpoint?
        bool isNewCheckpoint =
            CheckpointManager.Instance.currentCheckpoint != spawnPoint;

        // Save checkpoint
        CheckpointManager.Instance.SetCheckpoint(spawnPoint);

        if (isNewCheckpoint)
        {
            float previousVitality = playerHealth.vitality;

            playerHealth.vitality += playerHealth.vitalityRestore;
            playerHealth.vitality = Mathf.Min(
                playerHealth.vitality,
                playerHealth.maxVitality
            );

            float restoredAmount =
                playerHealth.vitality - previousVitality;

            if (restoredAmount > 0)
            {
                InteractionUI.Instance.ShowNotification(
                    "Vitality Restored +" + restoredAmount
                );
            }
        }

        Debug.Log("Checkpoint Saved: " + spawnPoint.position);

        // Restore player
        playerHealth.health = playerHealth.GetEffectiveMaxHealth();
        playerHealth.stamina = playerHealth.maxStamina;
        playerHealth.mp = playerHealth.maxMP;

        playerItems.RefillFlasks();

        Debug.Log(playerItems.currentHealingFlasks);

        // Respawn every enemy
        EnemySpawner[] spawners =
            FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.RespawnEnemy();
        }
    }

    private IEnumerator HideFlameRestoredText()
    {
        yield return new WaitForSeconds(2f);

        if (flameRestoredText != null)
        {
            flameRestoredText.SetActive(false);
        }
    }
}