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
            interaction.currentInteractable =
                interactable;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerInteraction interaction =
            other.GetComponent<PlayerInteraction>();

        if (interaction != null &&
            interaction.currentInteractable ==
            interactable)
        {
            interaction.currentInteractable =
                null;
        }
    }
}