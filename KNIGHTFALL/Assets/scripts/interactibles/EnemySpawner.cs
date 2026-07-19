using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    private GameObject currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        currentEnemy = Instantiate(
            enemyPrefab,
            transform.position,
            transform.rotation
        );
    }

    public void RespawnEnemy()
    {
        if (currentEnemy != null)
        {
            Destroy(currentEnemy);
        }

        SpawnEnemy();
    }
}