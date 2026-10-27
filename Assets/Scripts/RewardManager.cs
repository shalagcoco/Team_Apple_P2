using TMPro;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("HUD (drag your TextMeshPro labels here)")]
    public TMP_Text scoreText;
    public TMP_Text rewardText;

    [Header("Rewards")]
    [Min(0)] public int pointsPerEnemy = 10;
    [Min(0)] public int coinsPerEnemy = 1;
    [Min(1)] public int killsPerRapidFireReward = 5;
    [Min(0.1f)] public float rapidFireDuration = 6f;

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public int Kills { get; private set; }
    public bool RapidFireActive => Time.time < rapidFireUntil;

    private float rapidFireUntil;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreDisplay();
    }

    void Update()
    {
        if (rewardText != null)
            rewardText.text = RapidFireActive ? "REWARD: RAPID FIRE!" : "";
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    [ContextMenu("Test Reward")]
    public void EnemyDefeated()
    {
        Kills++;
        Score += pointsPerEnemy;
        Coins += coinsPerEnemy;

        if (killsPerRapidFireReward > 0 && Kills % killsPerRapidFireReward == 0)
        {
            rapidFireUntil = Time.time + rapidFireDuration;
            Debug.Log("Reward unlocked: Rapid Fire!");
        }

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Score}    Coins: {Coins}    Kills: {Kills}";
    }
}
