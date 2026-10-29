using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Automatic Shooting")]
    public GameObject bulletPrefab;
    [Min(0.1f)] public float attackInterval = 1f;
    [Min(0f)] public float muzzleOffset = 0.45f;

    private float nextShotTime;

    void Update()
    {
        if (Time.time < nextShotTime || bulletPrefab == null)
            return;

        Transform target = FindClosestEnemy();
        if (target == null)
            return;

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        Vector3 spawnPosition = transform.position + (Vector3)(direction * muzzleOffset);
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogError("PlayerAttack: The Bullet prefab needs a Bullet component.", bulletObject);
            Destroy(bulletObject);
            nextShotTime = Time.time + 1f;
            return;
        }

        // Bullet.cs destroys a projectile that has no target; assign it immediately.
        bullet.SetTarget(target);

        float interval = Mathf.Max(0.1f, attackInterval);
        if (RewardManager.Instance != null && RewardManager.Instance.RapidFireActive)
            interval *= 0.4f;

        nextShotTime = Time.time + interval;
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float bestDistanceSquared = float.PositiveInfinity;

        foreach (GameObject enemy in enemies)
        {
            float distanceSquared = ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude;
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                closest = enemy.transform;
            }
        }

        return closest;
    }
}
