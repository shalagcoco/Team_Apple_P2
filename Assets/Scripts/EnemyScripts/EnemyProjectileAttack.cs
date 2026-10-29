using UnityEngine;

public class EnemyProjectileAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 15;
    public float attackCooldown = 2f;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    private Transform player;
    private float nextAttackTime;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null || projectilePrefab == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > attackRange)
            return;

        // Only shoot if there is a clear path to the player.
        if (!IsPathClear())
            return;

        if (Time.time >= nextAttackTime)
        {
            ShootProjectile();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    bool IsPathClear()
    {
        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)transform.position).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            attackRange
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Ignore the enemy's own colliders.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            // The player has been reached, so the path is clear.
            if (hit.collider.CompareTag("Player"))
                return true;

            // An obstacle before the player blocks the shot.
            if (hit.collider.CompareTag("Obstacle"))
                return false;
        }

        // The ray did not reach the player.
        return false;
    }

    void ShootProjectile()
    {
        Debug.Log("shooting");
        GameObject projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        EnemyProjectile projectileScript =
            projectile.GetComponent<EnemyProjectile>();

        if (projectileScript != null)
        {
            projectileScript.SetTarget(player);
        }
        else
        {
            Debug.LogError(
                "Projectile prefab is missing EnemyProjectile.",
                projectile
            );
        }
    }
}
