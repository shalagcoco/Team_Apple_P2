using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Min(0.1f)] public float speed = 8f;
    [Min(0.1f)] public float lifetime = 6f;
    [Min(1)] public int damage = 1;

    public Color bulletColor = new Color(1f, 0.85f, 0.2f, 1f);

    private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
            sprite.color = bulletColor;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FixedUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            target.position,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Enemy") ||
            !other.gameObject.activeInHierarchy)
        {
            return;
        }

        // Check whether this enemy has a health component.
        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            // Damage the enemy instead of instantly destroying it.
            enemyHealth.TakeDamage(damage);
        }
        else
        {
            // Preserve the original one-hit behavior.
            // Disable first so two bullets hitting in one frame cannot
            // award the same kill twice.
            other.gameObject.SetActive(false);

            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.EnemyDefeated();
            }

            Destroy(other.gameObject);
        }

        // The bullet disappears after hitting an enemy.
        Destroy(gameObject);
    }
}