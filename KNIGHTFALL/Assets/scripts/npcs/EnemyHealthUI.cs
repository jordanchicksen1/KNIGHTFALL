using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    public EnemyHealth enemyHealth;
    public Image healthFill;
    public Canvas canvas;
    private Camera mainCamera;
    private PlayerLockOn playerLockOn;
    public GameObject healthBarVisual;
    [SerializeField] private TMP_Text damageText;
    private int displayedDamage;
    private Coroutine damageRoutine;

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        damageText.gameObject.SetActive(false);

        if (player != null)
        {
            playerLockOn = player.GetComponent<PlayerLockOn>();
        }
    }

    void Update()
    {
        if (enemyHealth == null)
            return;

        // Face camera
        canvas.transform.forward = mainCamera.transform.forward;

        // Health fill
        healthFill.fillAmount = enemyHealth.health / enemyHealth.maxHealth;

        // Only show while locked on
        if (playerLockOn != null)
        {
            bool isLockedTarget = playerLockOn.currentTarget == enemyHealth.transform;

            healthBarVisual.SetActive(isLockedTarget);
        }
    }

    public void ShowDamage(int damage)
    {
        displayedDamage += damage;
        damageText.text = displayedDamage.ToString();
        damageText.gameObject.SetActive(true);

        if (damageRoutine != null) StopCoroutine(damageRoutine);
        damageRoutine = StartCoroutine(HideDamage());
    }

    IEnumerator HideDamage()
    {
        yield return new WaitForSeconds(2f);
        displayedDamage = 0;
        damageText.text = "";
        damageText.gameObject.SetActive(false);
        damageRoutine = null;
    }
}