using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    public EnemyHealth bossHealth;
    public Image healthFill;
    public GameObject bossHealthBar;
    [SerializeField] private TMP_Text damageText;
    private int displayedDamage;
    private Coroutine damageRoutine;

    [SerializeField]
    private TMP_Text bossNameText;
    private string currentBossName;

    [Header("Status Effects")]
    [SerializeField] private Image statusIcon;
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite iceSprite;
    [SerializeField] private Sprite lightningSprite;

    void Start()
    {
    

        damageText.gameObject.SetActive(false);

        statusIcon.gameObject.SetActive(false);

       
    }

    void Update()
    {
        if (bossHealth == null)
            return;

        

        // Health fill
        healthFill.fillAmount = bossHealth.health / bossHealth.maxHealth;

  
    }

    public void ShowBoss(EnemyHealth boss, string bossName)
    {
        bossHealth = boss;

        currentBossName = bossName;

        bossNameText.text = currentBossName;

        bossHealthBar.SetActive(true);
    }

    public void HideBoss()
    {
        bossHealth = null;

        bossHealthBar.SetActive(false);
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

    public void ShowStatus(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Burning:
                statusIcon.sprite = fireSprite;
                break;

            case StatusEffect.Frozen:
                statusIcon.sprite = iceSprite;
                break;

            case StatusEffect.Shocked:
                statusIcon.sprite = lightningSprite;
                break;

            default:
                statusIcon.gameObject.SetActive(false);
                return;
        }

        statusIcon.gameObject.SetActive(true);
    }

    public void HideStatus()
    {
        statusIcon.gameObject.SetActive(false);
    }


}