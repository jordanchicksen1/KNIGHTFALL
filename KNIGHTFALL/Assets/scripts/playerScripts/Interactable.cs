using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public string interactionText = "Interact";

    public abstract void Interact();
}