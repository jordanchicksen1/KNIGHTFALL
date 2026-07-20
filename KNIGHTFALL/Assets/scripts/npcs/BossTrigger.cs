using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public BossController boss;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        boss.StartBossFight();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        boss.EndBossFight();
    }
}