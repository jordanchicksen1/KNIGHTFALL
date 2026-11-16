
using UnityEngine;

public class TalkingStatue : Interactable
{
    [Header("Dialogue")]
    [TextArea(3, 8)]
    public string firstDialogue;

    [TextArea(3, 8)]
    public string secondDialogue;

    [Header("Statue Lights")]
    public Light[] statueLights;

    private int dialogueStep;
    private bool isCompleted;

    private PlayerInteraction activePlayer;

    private void Awake()
    {
        // Start with all statue lights switched off.
        foreach (Light statueLight in statueLights)
        {
            if (statueLight != null)
                statueLight.enabled = false;
        }
    }

    public override void Interact()
    {
        if (isCompleted)
            return;

        // First interaction: begin the conversation.
        if (dialogueStep == 0)
        {
            activePlayer = FindFirstObjectByType<PlayerInteraction>();

            if (activePlayer == null)
                return;

            activePlayer.isReadingMessage = true;
            activePlayer.activeTalkingStatue = this;

            PlayerMovement movement =
                activePlayer.GetComponent<PlayerMovement>();

            if (movement != null)
                movement.moveInput = Vector2.zero;

            InteractionUI.Instance.ShowMessage(firstDialogue);

            dialogueStep = 1;
        }
        // Second interaction: show the next paragraph.
        else if (dialogueStep == 1)
        {
            InteractionUI.Instance.ShowMessage(secondDialogue);

            dialogueStep = 2;
        }
        // Third interaction: finish the conversation.
        else
        {
            InteractionUI.Instance.HideMessage();

            activePlayer.isReadingMessage = false;
            activePlayer.activeTalkingStatue = null;

            // Switch on the statue lights.
            foreach (Light statueLight in statueLights)
            {
                if (statueLight != null)
                    statueLight.enabled = true;
            }

            isCompleted = true;
        }
    }
}
