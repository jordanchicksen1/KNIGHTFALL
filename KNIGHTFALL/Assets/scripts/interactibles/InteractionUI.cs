using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;
    public GameObject promptObject;
    public TMP_Text interactionText;
    public Image buttonImage;

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
}