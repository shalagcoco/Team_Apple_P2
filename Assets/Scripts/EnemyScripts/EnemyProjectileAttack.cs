using UnityEngine;

public class EnemyProjectileAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 15f;
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

        if (Time.time >= nextAttackTime)
        {
            ShootProjectile();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void ShootProjectile()
    {
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