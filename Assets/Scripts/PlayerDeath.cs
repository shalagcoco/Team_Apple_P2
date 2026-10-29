using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Heart UI")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Game Menu")]
    public GameMenu gameMenu;

    [Header("Damage Settings")]
    public float invincibilityTime = 1f;

    private bool canTakeDamage = true;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")
            && canTakeDamage
            && !isDead)
        {
            TakeDamage();
        }
    }
    //the range attack
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile")
            && canTakeDamage
            && !isDead)
        {
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        currentHealth--;

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageCooldown());
        }
    }

    IEnumerator DamageCooldown()
    {
        canTakeDamage = false;

        yield return new WaitForSeconds(invincibilityTime);

        canTakeDamage = true;
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    void Die()
    {
        isDead = true;

        if (gameMenu != null)
        {
            gameMenu.GameOver();
        }
    }
}