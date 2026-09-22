using UnityEngine;

public class MessageInteractable : Interactable
{
    [TextArea(3, 8)]
    public string message;

    public override void Interact()
    {
        InteractionUI.Instance.ShowMessage(message);

        PlayerInteraction playerInteraction =
            FindFirstObjectByType<PlayerInteraction>();

        if (playerInteraction != null)
        {
            playerInteraction.isReadingMessage = true;
        }
    }
}