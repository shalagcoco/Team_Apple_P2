using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float checkDistance = 1f;

    [Header("Obstacle Detection")]
    [Tooltip("Radius used to check for obstacles.")]
    public float obstacleCheckRadius = 0.2f;

    [Header("Avoidance")]
    [Tooltip("Minimum time to keep an avoidance direction before reconsidering.")]
    public float avoidanceCommitTime = 0.4f;

    [Tooltip("How strongly the enemy prefers continuing its current direction.")]
    public float directionPersistence = 0.5f;

    [Header("Animation")]
    public Animator animator;

    private Transform player;
    private Rigidbody2D rb;

    private Vector2 currentMoveDirection;

    private bool isAvoidingObstacle;
    private float avoidanceTimer;

    private readonly float[] avoidanceAngles =
    {
        30f, -30f,
        60f, -60f,
        90f, -90f,
        120f, -120f,
        150f, -150f,
        180f
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
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
        if (player == null || rb == null)
        {
            return;
        }

        Vector2 toPlayer =
            (Vector2)player.position - rb.position;

        if (toPlayer.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }

        Vector2 desiredDirection = toPlayer.normalized;

        // If the direct route is clear, resume chasing the player.
        if (IsPathClear(desiredDirection))
        {
            currentMoveDirection = desiredDirection;
            isAvoidingObstacle = false;
            avoidanceTimer = 0f;
        }
        else
        {
            avoidanceTimer -= Time.fixedDeltaTime;

            // Choose a new direction only when:
            // 1. We have not started avoiding an obstacle, or
            // 2. Our current direction is blocked, or
            // 3. Our commitment time has expired.
            bool currentDirectionBlocked =
                currentMoveDirection.sqrMagnitude < 0.001f ||
                !IsPathClear(currentMoveDirection);

            if (!isAvoidingObstacle ||
                currentDirectionBlocked ||
                avoidanceTimer <= 0f)
            {
                Vector2 newDirection =
                    FindBestAvoidanceDirection(desiredDirection);

                // Only replace the current direction if a valid
                // alternative was found.
                if (newDirection.sqrMagnitude > 0.001f)
                {
                    currentMoveDirection = newDirection;
                    isAvoidingObstacle = true;
                    avoidanceTimer = avoidanceCommitTime;
                }
            }
        }

        rb.linearVelocity =
            currentMoveDirection * moveSpeed;

        UpdateAnimation(currentMoveDirection);
    }

    bool IsPathClear(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            rb.position,
            obstacleCheckRadius,
            direction.normalized,
            checkDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null &&
                hit.collider.CompareTag("Obstacle"))
            {
                return false;
            }
        }

        return true;
    }

    Vector2 FindBestAvoidanceDirection(Vector2 desiredDirection)
    {
        Vector2 bestDirection = Vector2.zero;
        float bestScore = float.NegativeInfinity;

        foreach (float angle in avoidanceAngles)
        {
            Vector2 candidateDirection =
                RotateDirection(desiredDirection, angle);

            if (!IsPathClear(candidateDirection))
            {
                continue;
            }

            // Prefer directions that make progress toward the player.
            float targetScore =
                Vector2.Dot(candidateDirection, desiredDirection);

            // Prefer continuing in approximately the same direction.
            float persistenceScore = 0f;

            if (currentMoveDirection.sqrMagnitude > 0.001f)
            {
                persistenceScore = Vector2.Dot(
                    candidateDirection,
                    currentMoveDirection.normalized
                );
            }

            float score =
                targetScore +
                persistenceScore * directionPersistence;

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidateDirection;
            }
        }

        return bestDirection;
    }

    Vector2 RotateDirection(Vector2 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    void UpdateAnimation(Vector2 direction)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
}