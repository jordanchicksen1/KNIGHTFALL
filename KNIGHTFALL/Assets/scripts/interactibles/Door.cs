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

    void Start()
    {
        closedRotation =
            doorPivot.localRotation;

        targetRotation = closedRotation;
    }

    void Update()
    {

        doorPivot.localRotation =
            Quaternion.Slerp(doorPivot.localRotation, targetRotation, openSpeed * Time.deltaTime);
    }

    public override void Interact()
    {
        if (!isOpen)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            Vector3 toPlayer = player.transform.position - doorPivot.position;

            float side = Vector3.Dot(transform.right, toPlayer);

            float angle = side > 0 ? -openAngle : openAngle;

            targetRotation = closedRotation * Quaternion.Euler(0, angle, 0);

            isOpen = true;
        }
        else
        {
            targetRotation = closedRotation;

            isOpen = false;
        }
    }
}