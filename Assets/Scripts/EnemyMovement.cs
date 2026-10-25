using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float checkDistance = 1f;
    public Animator animator;

    private Transform player;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction =
            ((Vector2)player.position - rb.position).normalized;

        Vector2 moveDirection = direction;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            direction,
            checkDistance
        );

        if (hit.collider != null &&
            !hit.collider.CompareTag("Player"))
        {
            Vector2 left =
                new Vector2(-direction.y, direction.x);

            Vector2 right =
                new Vector2(direction.y, -direction.x);

            RaycastHit2D leftHit = Physics2D.Raycast(
                rb.position,
                left,
                checkDistance
            );

            RaycastHit2D rightHit = Physics2D.Raycast(
                rb.position,
                right,
                checkDistance
            );

            if (leftHit.collider == null)
            {
                moveDirection = left;
            }
            else if (rightHit.collider == null)
            {
                moveDirection = right;
            }
        }

        rb.linearVelocity =
            moveDirection.normalized * moveSpeed;

        if (animator != null)
        {
            animator.SetFloat(
                "MoveX",
                moveDirection.x
            );

            animator.SetFloat(
                "MoveY",
                moveDirection.y
            );

            animator.SetFloat(
                "Speed",
                rb.linearVelocity.sqrMagnitude
            );
        }
    }
}