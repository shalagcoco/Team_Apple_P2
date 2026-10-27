using TMPro;
using UnityEngine;

/// <summary>
/// Add to a phone GameObject. Press E when close to make nearby enemies
/// temporarily investigate this phone instead of chasing the player.
/// Works with both Unity's old Input Manager and new Input System.
/// </summary>
public class RingingPhone : MonoBehaviour
{
    [Header("Interaction")]
    public Transform player;
    [Min(0.1f)] public float useDistance = 2f;
    [Min(0f)] public float cooldown = 10f;
    public TMP_Text promptText;  // Optional: UI TextMeshPro label

    [Header("Distraction")]
    [Min(0.1f)] public float hearingDistance = 8f;
    [Min(0.1f)] public float distractionDuration = 6f;
    [Min(0)] public int distractionPoints = 5;
    public AudioSource ringAudio; // Optional; attach included phone_ring.wav

    private float nextAllowedRing;
    private float ringUntil;
    private bool rewardGivenForThisRing;

    private void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
        }

        if (ringAudio == null)
            ringAudio = GetComponent<AudioSource>();

        if (promptText != null)
            promptText.text = "";
    }

    private void Update()
    {
        bool closeEnough = player != null &&
            Vector2.Distance(transform.position, player.position) <= useDistance;
        bool canRing = Time.time >= nextAllowedRing;

        if (promptText != null)
            promptText.text = closeEnough && canRing ? "Press E to ring phone" : "";

        if (closeEnough && canRing && InteractPressed())
            Ring();

        if (ringAudio != null && ringAudio.isPlaying && Time.time >= ringUntil)
            ringAudio.Stop();
    }

    private static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
            return true;
#endif
        return false;
    }

    private void Ring()
    {
        nextAllowedRing = Time.time + cooldown;
        ringUntil = Time.time + distractionDuration;
        rewardGivenForThisRing = false;

        if (ringAudio != null && ringAudio.clip != null)
        {
            ringAudio.loop = true;
            ringAudio.Play();
        }
        else
        {
            Debug.Log("RingingPhone: No audio clip assigned. Distraction still works.", this);
        }

        int investigating = 0;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (Vector2.Distance(enemy.transform.position, transform.position) > hearingDistance)
                continue;

            PhoneEnemyResponder responder = enemy.GetComponent<PhoneEnemyResponder>();
            if (responder != null && responder.Investigate(this, distractionDuration))
                investigating++;
        }

        Debug.Log("Phone rang: " + investigating + " enemy/enemies investigating.", this);
        if (investigating == 0)
            Debug.Log("Tip: Enemy prefab needs PhoneEnemyResponder and its Chase / Movement Script field assigned.", this);
    }

    /// <summary>
    /// Invoked by the first enemy to actually reach the phone.
    /// Gives +5 points ONCE per ring, not once for each enemy.
    /// </summary>
    public void OnEnemyReachedPhone()
    {
        if (rewardGivenForThisRing || Time.time > ringUntil)
            return;

        rewardGivenForThisRing = true;
        if (RewardManager.Instance == null)
        {
            Debug.LogWarning("Phone reached, but no RewardManager was found in the scene.", this);
            return;
        }

        // Compatible with both the original RewardManager and the updated version.
        // If RewardManager hasn't been updated to include AwardDistraction(int),
        // the phone can still ring without breaking Unity compilation.
        System.Reflection.MethodInfo award = RewardManager.Instance.GetType().GetMethod(
            "AwardDistraction", new System.Type[] { typeof(int) });
        if (award != null)
            award.Invoke(RewardManager.Instance, new object[] { distractionPoints });
        else
            Debug.LogWarning("Add AwardDistraction(int) to RewardManager to earn +5 phone points. An updated RewardManager is in the optional folder of the OfficePhone add-on.", this);
    }

    private void OnDisable()
    {
        if (promptText != null)
            promptText.text = "";
        if (ringAudio != null)
            ringAudio.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, useDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingDistance);
    }
}
