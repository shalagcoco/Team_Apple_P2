using UnityEngine;
using System.Collections;

public class EnemyChargeAttack : MonoBehaviour
{
    [Header("Charge Settings")]
    public float chargeRange = 4f;
    public float preparationTime = 1f;
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.5f;
    public float fatigueTime = 0.5f;
    public float chargeCooldown = 2f;

    [Header("Warning Flash")]
    public Color normalColor = Color.yellow;
    public Color warningColor = new Color(1f, 0.5f, 0f);// Orange color
    public Color fatigueColor = Color.gray;
    public float flashInterval = 0.15f;

    private EnemyMovement enemyMovement;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    private bool isCharging = false;
    private bool isOnCooldown = false;

    void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Support SpriteRenderers on child objects too.
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    void Update()
    {
        if (player == null || isCharging)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= chargeRange &&
            !isOnCooldown &&
            IsPathClear())
        {
            StartCoroutine(ChargeAttack());
        }
    }

    bool IsPathClear()
    {
        Vector2 direction =
            ((Vector2)player.position - rb.position).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            rb.position,
            direction,
            chargeRange
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

    IEnumerator ChargeAttack()
    {
        isCharging = true;
        isOnCooldown = true;

        if (enemyMovement != null)
            enemyMovement.enabled = false;

        rb.linearVelocity = Vector2.zero;

        // WINDUP: Flash yellow and orange as a warning.
        float timer = 0f;
        bool showWarningColor = false;

        while (timer < preparationTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = showWarningColor
                    ? warningColor
                    : normalColor;
            }

            showWarningColor = !showWarningColor;

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // CHARGE: Stay orange.
        if (spriteRenderer != null)
            spriteRenderer.color = warningColor;

        Vector2 chargeDirection =
            ((Vector2)player.position - rb.position).normalized;

        timer = 0f;

        while (timer < chargeDuration)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // FATIGUE: Stop and turn gray.
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = fatigueColor;
        }

        yield return new WaitForSeconds(fatigueTime);

        // Restore the normal appearance and movement.
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        if (enemyMovement != null)
            enemyMovement.enabled = true;

        isCharging = false;

        // COOLDOWN: Wait before another charge.
        yield return new WaitForSeconds(chargeCooldown);

        isOnCooldown = false;
    }
}