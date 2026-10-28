using UnityEngine;

/// <summary>
/// Add to the ROOT of your ENEMY PREFAB (not just one enemy in the scene).
/// Assign the existing chasing/movement MonoBehaviour in the Inspector.
/// While investigating, this disables that chase script temporarily and
/// moves the enemy toward the phone. When the timer ends, chase resumes.
/// </summary>
public class PhoneEnemyResponder : MonoBehaviour
{
    [Header("Drag the enemy's existing CHASE / MOVEMENT component here")]
    public MonoBehaviour chaseMovementScript;

    [Header("Investigation movement")]
    [Min(0.1f)] public float moveSpeed = 2.5f;
    [Min(0.05f)] public float stopDistance = 0.45f;

    private Rigidbody2D body;
    private RingingPhone currentPhone;
    private float investigationEnds;
    private bool alreadyReachedPhone;
    private bool originalScriptWasEnabled;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public bool Investigate(RingingPhone phone, float duration)
    {
        if (phone == null || currentPhone != null || !isActiveAndEnabled)
            return false;

        // Never disable the responder itself, or a script that is already off.
        if (chaseMovementScript == null || chaseMovementScript == this ||
            !chaseMovementScript.enabled)
        {
            Debug.LogWarning("PhoneEnemyResponder: assign the ACTIVE enemy chase/movement component in its Inspector.", this);
            return false;
        }

        originalScriptWasEnabled = chaseMovementScript.enabled;
        chaseMovementScript.enabled = false;

        currentPhone = phone;
        investigationEnds = Time.time + duration;
        alreadyReachedPhone = false;
        if (body != null)
            body.linearVelocity = Vector2.zero;
        return true;
    }

    private void Update()
    {
        if (currentPhone == null)
            return;

        if (Time.time >= investigationEnds)
        {
            EndInvestigation();
            return;
        }

        Vector2 current = transform.position;
        Vector2 destination = currentPhone.transform.position;
        if (!alreadyReachedPhone && Vector2.Distance(current, destination) <= stopDistance)
        {
            alreadyReachedPhone = true;
            if (body != null)
                body.linearVelocity = Vector2.zero;
            currentPhone.OnEnemyReachedPhone();
        }
    }

    private void FixedUpdate()
    {
        if (currentPhone == null || alreadyReachedPhone)
            return;

        Vector2 destination = currentPhone.transform.position;
        Vector2 current = body != null ? body.position : (Vector2)transform.position;
        if (Vector2.Distance(current, destination) <= stopDistance)
            return;

        Vector2 next = Vector2.MoveTowards(current, destination, moveSpeed * Time.fixedDeltaTime);
        if (body != null && body.simulated)
            body.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    private void EndInvestigation()
    {
        currentPhone = null;
        if (chaseMovementScript != null)
            chaseMovementScript.enabled = originalScriptWasEnabled;
    }

    private void OnDisable()
    {
        // Restore the previous behavior if the enemy is disabled before the timer ends.
        if (currentPhone != null)
            EndInvestigation();
    }
}
