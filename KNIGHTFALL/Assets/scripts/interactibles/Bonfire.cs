
using UnityEngine;
using System.Collections;

public class Bonfire : Interactable
{
    [Header("Checkpoint")]
    public Transform spawnPoint;

    [Header("Bonfire Flame")]
    public GameObject fireModel;
    private GameObject flameRestoredText;

    [Header("Bonfire Lighting")]
    public Color kindledLightColor = new Color(1f, 0.65f, 0.25f);
    private Light[] bonfireLights;

    private BonfireUI bonfireUI;
    private PlayerHealth playerHealth;
    private PlayerItems playerItems;
    private PlayerAudio playerAudio;

    private bool flameRestored = false;

    public bool IsKindled { get; private set; } = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        bonfireLights = GetComponentsInChildren<Light>(true);

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
            playerAudio = player.GetComponent<PlayerAudio>();
        }

        bonfireUI = FindFirstObjectByType<BonfireUI>();

        if (bonfireUI == null)
        {
            Debug.LogError("No BonfireUI found in the scene!");
        }

        if (flameRestoredText != null)
        {
            flameRestoredText.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Bonfire could not find FlameRestoredText in the scene."
            );
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

        // Automatically rest when interacting with a lit bonfire.
        RestAtBonfire();

        // Then open the bonfire menu.
        if (bonfireUI != null)
        {
            bonfireUI.Open(this);
        }
        else
        {
            Debug.LogWarning("BonfireUI could not be found!");
        }
    }

    private void LightBonfire()
    {
        flameRestored = true;

        if (playerAudio != null)
        {
            playerAudio.PlayBonfireLight();
        }

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

    public bool TryKindle()
    {
        if (IsKindled)
            return false;

        if (playerItems == null || playerHealth == null)
            return false;

        // Consume one Ember and restore vitality immediately.
        bool emberUsed = playerItems.UseEmberAtBonfire();

        if (!emberUsed)
            return false;

        // Play the bonfire lighting sound.
        if (playerAudio != null)
        {
            playerAudio.PlayBonfireLight();
        }

        IsKindled = true;

        UpdateBonfireLight();

        InteractionUI.Instance.ShowNotification(
            "Ember offered. Vitality restoration granted!"
        );

        Debug.Log("Bonfire Kindled!");

        return true;
    }

    private void UpdateBonfireLight()
    {
        if (bonfireLights == null)
            return;

        foreach (Light lightSource in bonfireLights)
        {
            lightSource.color = kindledLightColor;
            lightSource.intensity *= 1.25f;
        }
    }

    public void RestAtBonfire()
    {
        Debug.Log("Rested at Bonfire");

        if (playerAudio != null)
        {
            playerAudio.PlayBonfireRest();
        }

        // Check whether this is a new checkpoint.
        bool isNewCheckpoint =
            CheckpointManager.Instance.currentCheckpoint != spawnPoint;

        // Save checkpoint.
        CheckpointManager.Instance.SetCheckpoint(spawnPoint);

        // New checkpoints retain their original vitality restoration.
        // Kindled bonfires restore vitality every time the player rests.
        if (isNewCheckpoint || IsKindled)
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

        // Restore player resources.
        playerHealth.health = playerHealth.GetEffectiveMaxHealth();
        playerHealth.stamina = playerHealth.maxStamina;
        playerHealth.mp = playerHealth.maxMP;

        playerItems.RefillFlasks();

        Debug.Log(playerItems.currentHealingFlasks);

        // Respawn every enemy.
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
