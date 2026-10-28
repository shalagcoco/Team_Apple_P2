using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject startMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    private bool gameStarted = false;
    private bool isPaused = false;
    private bool isGameOver = false;

    void Start()
    {
        // Show start menu when the game begins
        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Freeze game until player presses Start
        Time.timeScale = 0f;
    }

    void Update()
    {
        // Do not allow pause before game starts
        if (!gameStarted)
            return;

        // Do not allow pause when game is over
        if (isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        isPaused = false;
        isGameOver = false;

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        if (!gameStarted || isGameOver)
            return;

        isPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        if (isGameOver)
            return;

        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        isGameOver = true;
        isPaused = false;

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Important: restore time scale before reloading
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}