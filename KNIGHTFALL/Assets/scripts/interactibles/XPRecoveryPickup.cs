
using UnityEngine;

public class XPRecoveryPickup : Interactable
{
    private PlayerXP playerXP;
    private PlayerAudio playerAudio;

    [Header("Recovery Effects")]
    public GameObject emberEffectPrefab;

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerXP = player.GetComponent<PlayerXP>();
            playerAudio = player.GetComponent<PlayerAudio>();
        }

        interactionText = "Recover Lost XP";
    }

    public override void Interact()
    {
        if (playerXP == null)
            return;

        // Recover the player's lost XP.
        playerXP.RecoverXP();

        // Play the recovery sound.
        if (playerAudio != null)
        {
            playerAudio.PlayXPRecovery();
        }

        // Spawn the Ember effect around the player.
        if (emberEffectPrefab != null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                Instantiate(
                    emberEffectPrefab,
                    player.transform.position,
                    Quaternion.identity,
                    player.transform
                );
            }


        }

        // Show the large recovery announcement.
        InteractionUI.Instance.ShowStrengthRegained();

        // Hide the interaction prompt.
        InteractionUI.Instance.HidePrompt();

        // Destroy the recovery orb.
        Destroy(gameObject);
    }
}
