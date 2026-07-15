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

    public override void Interact()
    {
        if (isOpen)
            return;

        if (requiresKey)
        {
            if (!inventory.UseKey())
            {
                Debug.Log("Door is locked.");

                return;
            }
        }

        if (isOneWayDoor)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            float distance = Vector3.Distance(player.transform.position, allowedSide.position);

            if (distance > 1f)
                return;
        }

        isOpen = true;

        targetRotation = closedRotation * Quaternion.Euler(0, openAngle,0);
    }
}