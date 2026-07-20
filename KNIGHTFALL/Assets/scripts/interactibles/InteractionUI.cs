using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    [Header("Prompt")]
    public static InteractionUI Instance;
    public GameObject promptObject;
    public TMP_Text interactionText;
    public Image buttonImage;

    [Header("Notification")]
    public GameObject notificationObject;
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    [Header("Death")]
    public GameObject deathObject;
    public float deathDuration = 2f;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPrompt(string text)
    {
        promptObject.SetActive(true);
        interactionText.text = text;
    }

    public void HidePrompt()
    {
        promptObject.SetActive(false);
    }

    public void ShowNotification(string message)
    {
        StopAllCoroutines();
        StartCoroutine(NotificationRoutine(message));
    }

    private IEnumerator NotificationRoutine(string message)
    {
        notificationObject.SetActive(true);
        notificationText.text = message;
        HidePrompt();
        yield return new WaitForSeconds(notificationDuration);
        notificationObject.SetActive(false);
    }

    public IEnumerator DeathRoutine()
    {
        deathObject.SetActive(true);
        yield return new WaitForSeconds(deathDuration);
        deathObject.SetActive(false);
    }
}