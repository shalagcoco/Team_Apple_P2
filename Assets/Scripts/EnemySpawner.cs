using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Difficulty")]
    [Min(0.5f)] public float spawnInterval = 4f;
    [Min(0f)] public float firstSpawnDelay = 3f;
    [Min(1)] public int maxEnemies = 5;
    [Min(0f)] public float minimumSpawnDistance = 4f;

    [Header("Enemy Unlocks")]
    [Min(0.1f)] public float variantUnlockInterval = 30f;

    private Transform player;
    private float gameStartTime;

    void Start()
    {
        gameStartTime = Time.time;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "EnemySpawner: No GameObject tagged 'Player' was found.",
                this
            );
            return;
        }

        player = playerObject.transform;

        if (enemyPrefabs == null ||
            enemyPrefabs.Length == 0 ||
            enemyPrefabs[0] == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "EnemySpawner: Assign at least the first Enemy Prefab " +
                "and your Spawn Points in the Inspector.",
                this
            );
            return;
        }

        // Preserve the minimum spawn interval.
        // The existing SampleScene stores '1' in its Inspector, so the old
        // serialized value otherwise overrides the new 4-second code default.
        spawnInterval = Mathf.Max(4f, spawnInterval);

        InvokeRepeating(
            nameof(SpawnEnemy),
            firstSpawnDelay,
            spawnInterval
        );
    }

    void OnDisable()
    {
        CancelInvoke(nameof(SpawnEnemy));
    }

    void SpawnEnemy()
    {
        if (player == null || enemyPrefabs == null)
            return;

        // Stop spawning when the enemy limit is reached.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        //start: NEW CODE FOR VARIET UNLOCKING
        //------------------------------------
        // Determine how many variants are currently unlocked.
        float elapsedTime = Time.time - gameStartTime;

        int unlockedCount = 1 + Mathf.FloorToInt(
            elapsedTime / variantUnlockInterval
        );

        unlockedCount = Mathf.Min(
            unlockedCount,
            enemyPrefabs.Length
        );

        // Build a list of available enemy prefabs.
        List<GameObject> availableEnemies = new List<GameObject>();

        for (int i = 0; i < unlockedCount; i++)
        {
            if (enemyPrefabs[i] != null)
            {
                availableEnemies.Add(enemyPrefabs[i]);
            }
        }

        if (availableEnemies.Count == 0)
            return;

        // Choose one of the currently unlocked enemy variants.
        GameObject selectedEnemy = availableEnemies[
            Random.Range(0, availableEnemies.Count)
        ];
        //end: NEW CODE FOR VARIET UNLOCKING
        //----------------------------------

        // Try all of the spawn locations, starting at a random one.
        int startIndex = Random.Range(0, spawnPoints.Length);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point =
                spawnPoints[(startIndex + i) % spawnPoints.Length];

            if (point == null)
                continue;

            if (Vector2.Distance(
                point.position,
                player.position
            ) < minimumSpawnDistance)
            {
                continue;
            }
            // Spawn the selected enemy at the chosen spawn point.
            Instantiate(
                selectedEnemy,
                point.position,
                Quaternion.identity
            );

            return;
        }
    }
}