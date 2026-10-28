using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Min(1)] public int maxHealth = 3;

    private int currentHealth;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log(
            gameObject.name + " health: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Prevent further collisions before destruction.
        gameObject.SetActive(false);

        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.EnemyDefeated();
        }

        Destroy(gameObject);
    }
}
