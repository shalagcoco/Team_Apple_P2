using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float lifeTime = 4f;

    private Vector2 moveDirection;

    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        // Calculate direction once when fired
        moveDirection = (
            (Vector2)newTarget.position -
            (Vector2)transform.position
        ).normalized;
    }

    void Update()
    {
        // Move in a straight line
        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: Apply damage to the player here.
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
