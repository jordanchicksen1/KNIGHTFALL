using UnityEngine;

public class Door : Interactable
{
    [Header("Door")]
    public Transform doorPivot;
    public float openAngle = 90f;
    public float openSpeed = 4f;
    private Quaternion closedRotation;
    private bool isOpen;
    private Quaternion targetRotation;

    [Header("Door Type")]
    public bool isOneWayDoor;
    public Transform allowedSide;
    public bool requiresKey;
    private PlayerInventory inventory;

    [Header("Audio")]
    public AudioClip doorOpenSFX;
    public AudioClip doorLockedSFX;

    void Start()
    {
        closedRotation = doorPivot.localRotation;

        targetRotation = closedRotation;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            inventory = player.GetComponent<PlayerInventory>();
        }
    }

    void Update()
    {

        doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRotation, openSpeed * Time.deltaTime);
    }

    private void PlayDoorSound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, doorPivot.position);
        }
    }

    public override void Interact()
    {
        if (isOpen)
            return;

        if (requiresKey)
        {
            if (!inventory.UseKey())
            {
                PlayDoorSound(doorLockedSFX);
                InteractionUI.Instance.ShowNotification("The door is locked.");
                return;
            }
        }

        if (isOneWayDoor)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            float distance = Vector3.Distance(player.transform.position, allowedSide.position);

            if (distance > 1f)
            {
                PlayDoorSound(doorLockedSFX);
                InteractionUI.Instance.ShowNotification("Opens from the other side.");
                return;
            }


        }

        isOpen = true;

        targetRotation = closedRotation * Quaternion.Euler(0, openAngle,0);

        PlayDoorSound(doorOpenSFX);
    }
}