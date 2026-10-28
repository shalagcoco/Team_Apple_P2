using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float lifeTime = 4f;
    public Color bulletColor = new Color(0.518f, 0f, 0.082f, 1f);


    private Vector2 moveDirection;

    void Awake()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = bulletColor;
    }

    void Start()
    {
        //Debug.Log("EnemyProjectile: Start called!", this);

        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            //Debug.LogWarning("EnemyProjectile: Target is null!", this);
            return;
        }

        moveDirection = (
            (Vector2)newTarget.position -
            (Vector2)transform.position
        ).normalized;

        //Debug.Log("EnemyProjectile: Target assigned! Direction: " +moveDirection,this);
    }

    void Update()
    {
        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log( "EnemyProjectile hit: " + other.name +" | Tag: " + other.tag,this);

        if (other.CompareTag("Player") ||
            other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Debug.Log("EnemyProjectile destroyed at time: " +Time.time +" | Position: " +transform.position, this);
    }
}
