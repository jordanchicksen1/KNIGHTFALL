using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    public EnemyHealth enemyHealth;

    public Image healthFill;

    public Canvas canvas;

    private Camera mainCamera;

    private PlayerLockOn playerLockOn;

    public GameObject healthBarVisual;

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerLockOn =
                player.GetComponent<PlayerLockOn>();
        }
    }

    void Update()
    {
        if (enemyHealth == null)
            return;

        // Face camera
        canvas.transform.forward =
            mainCamera.transform.forward;

        // Health fill
        healthFill.fillAmount =
            enemyHealth.health /
            enemyHealth.maxHealth;

        // Only show while locked on
        if (playerLockOn != null)
        {
            bool isLockedTarget =
                playerLockOn.currentTarget ==
                enemyHealth.transform;

            healthBarVisual.SetActive(isLockedTarget);
        }
    }
}