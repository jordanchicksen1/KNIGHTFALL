using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    public Interactable interactable;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerInteraction interaction =
            other.GetComponent<PlayerInteraction>();

        if (interaction != null)
        {
            interaction.currentInteractable = interactable;
            InteractionUI.Instance.ShowPrompt(interactable.interactionText);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerInteraction interaction =
            other.GetComponent<PlayerInteraction>();

        if (interaction != null && interaction.currentInteractable == interactable)
        {
            InteractionUI.Instance.HidePrompt();
            interaction.currentInteractable = null;
        }
    }
}