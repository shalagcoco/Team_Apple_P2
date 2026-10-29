using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public GameObject gameOverText;

    private bool isDead = false;

    void Start()
    {
        gameOverText.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDead)
        {
            isDead = true;
            GameOver();
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

    void GameOver()
    {
        gameOverText.SetActive(true);
        Time.timeScale = 0f;
    }
}