using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Difficulty")]
    [Min(0.5f)] public float spawnInterval = 4f;
    [Min(0f)] public float firstSpawnDelay = 3f;
    [Min(1)] public int maxEnemies = 5;
    [Min(0f)] public float minimumSpawnDistance = 4f;

    private Transform player;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("EnemySpawner: No GameObject tagged 'Player' was found.", this);
            return;
        }

        player = playerObject.transform;

        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: Set the Enemy Prefab and Spawn Points in the Inspector.", this);
            return;
        }

        // The existing SampleScene stores '1' in its Inspector, so the old
        // serialized value otherwise overrides the new 4-second code default.
        spawnInterval = Mathf.Max(4f, spawnInterval);
        InvokeRepeating(nameof(SpawnEnemy), firstSpawnDelay, spawnInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(SpawnEnemy));
    }

    void SpawnEnemy()
    {
        if (player == null || enemyPrefab == null)
            return;

        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        // Try all of the spawn locations, starting at a random one.
        int startIndex = Random.Range(0, spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[(startIndex + i) % spawnPoints.Length];
            if (point == null)
                continue;

            if (Vector2.Distance(point.position, player.position) < minimumSpawnDistance)
                continue;

            Instantiate(enemyPrefab, point.position, Quaternion.identity);
            return;
        }
    }
}
