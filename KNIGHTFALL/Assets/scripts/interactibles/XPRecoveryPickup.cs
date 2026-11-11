
using UnityEngine;

public class XPRecoveryPickup : Interactable
{
    private PlayerXP playerXP;

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerXP = player.GetComponent<PlayerXP>();
        }

        interactionText = "Recover Lost XP";
    }

    public override void Interact()
    {
        if (playerXP == null)
            return;

        playerXP.RecoverXP();

        InteractionUI.Instance.ShowNotification(
            "Lost XP recovered!"
        );

        InteractionUI.Instance.HidePrompt();

        Destroy(gameObject);
    }
}
