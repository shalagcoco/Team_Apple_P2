using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Transform[] spawnPoints;

    public float spawnInterval = 1f;
    public int maxEnemies = 5;

    public float minimumSpawnDistance = 4f;

    private Transform player;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        InvokeRepeating(
            nameof(SpawnEnemy),
            0f,
            spawnInterval
        );
    }

    void SpawnEnemy()
    {
        if (player == null)
            return;

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length >= maxEnemies)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        // 尝试几个出生点
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point =
                spawnPoints[
                    Random.Range(0, spawnPoints.Length)
                ];

            if (Vector2.Distance(
                point.position,
                player.position
            ) < minimumSpawnDistance)
            {
                continue;
            }

            Instantiate(
                enemyPrefab,
                point.position,
                Quaternion.identity
            );

            return;
        }
    }
}