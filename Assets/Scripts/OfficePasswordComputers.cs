
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfficePasswordComputers : MonoBehaviour
{
    public enum StationType
    {
        Boss,
        Clue
    }

    [Serializable]
    public class ComputerStation
    {
        public string name;
        public Transform interactionPoint;
        public StationType type;
        public string clue;
    }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionRadius = 1.0f;

    [Header("Computers")]
    [SerializeField] private ComputerStation[] computers;

    [Header("UI")]
    [SerializeField] private GameObject computerWindow;
    [SerializeField] private GameObject bossLoginPage;
    [SerializeField] private GameObject cluePage;

    [Header("Boss Login")]
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button loginButton;

    [Header("Clue Screen")]
    [SerializeField] private TMP_Text clueText;

    [Header("Password")]
    [SerializeField] private string correctPassword = "BerN";

    // Currently visited computer
    private ComputerStation currentComputer;

    // Boss login state
    private bool bossUnlocked = false;

    // False: PCs display numbers
    // True: PCs display letters
    private bool lettersRevealed = false;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        // Computer UI starts hidden.
        if (computerWindow != null)
        {
            computerWindow.SetActive(false);
        }

        passwordInput.text = "";
        feedbackText.text = "";

}

// ==========================================
// COMPUTER PROXIMITY
// ==========================================

private void Update()
    {
        if (player == null || computerWindow == null)
            return;

        ComputerStation nearest = FindNearestComputer();

        // Do nothing while staying at the same PC.
        if (nearest == currentComputer)
            return;

        currentComputer = nearest;

        // Player walked away from all computers.
        if (currentComputer == null)
        {
            computerWindow.SetActive(false);
            return;
        }

        // Player approached a computer.
        OpenComputer(currentComputer);
    }

    private ComputerStation FindNearestComputer()
    {
        ComputerStation nearest = null;
        float bestDistance = interactionRadius;

        if (computers == null)
            return null;

        foreach (ComputerStation computer in computers)
        {
            if (computer == null ||
                computer.interactionPoint == null)
                continue;

            float distance = Vector2.Distance(
                player.position,
                computer.interactionPoint.position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = computer;
            }
        }

        return nearest;
    }

    private void OpenComputer(ComputerStation computer)
    {
        computerWindow.SetActive(true);

        bool isBoss = computer.type == StationType.Boss;

        bossLoginPage.SetActive(isBoss);
        cluePage.SetActive(!isBoss);

        if (isBoss)
        {
            OpenBossLogin();
        }
        else
        {
            ShowComputerClue(computer);
        }
    }

    // ==========================================
    // BOSS COMPUTER LOGIN
    // ==========================================


    private void OpenBossLogin()
    {
        if (bossUnlocked)
        {
            // Boss computer already unlocked.
            passwordInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            ShowSuccess();
        }
        else
        {
            // Show login controls.
            passwordInput.gameObject.SetActive(true);
            loginButton.gameObject.SetActive(true);

            // IMPORTANT:
            // Do not clear passwordInput.text.
            // Do not clear feedbackText.text.
            // Everything stays as Catsby left it.
        }
    }


    // Called ONLY when the LOGIN button is clicked.
    public void CheckPassword()
    {
        Debug.Log("LOGIN BUTTON CLICKED!");

        if (currentComputer == null)
        {
            Debug.LogWarning("No computer selected!");
            return;
        }

        if (currentComputer.type != StationType.Boss)
        {
            Debug.LogWarning("Not at boss computer!");
            return;
        }

        if (bossUnlocked)
            return;

        if (passwordInput == null || feedbackText == null)
        {
            Debug.LogError("Missing password UI references!");
            return;
        }

        string attempt = passwordInput.text.Trim();

        Debug.Log("Password submitted: " + attempt);

        // ======================================
        // EMPTY PASSWORD
        // ======================================

        if (attempt.Length == 0)
        {
            ShowError("Enter a password.");
            return;
        }

        // ======================================
        // SPECIAL PUZZLE TRIGGER: EXACTLY 3142
        // ======================================

        if (attempt == "3142")
        {
            // Switch all four computers to letters.
            if (!lettersRevealed)
            {
                lettersRevealed = true;

                Debug.Log(
                    "PUZZLE ACTIVATED! " +
                    "All computers now show letters."
                );
            }

            ShowError(
                "ERROR: ALPHABETIC CHARACTERS ONLY!\n" +
                "Numbers must mean something else...\nMaybe you should check those computers again..."
            );

            return;
        }

        // ======================================
        // CORRECT PASSWORD: BerN
        // ======================================

        if (string.Equals(
            attempt,
            correctPassword,
            StringComparison.Ordinal))
        {
            bossUnlocked = true;

            Debug.Log("SUCCESS! Boss computer unlocked!");

            // Hide login elements.
            passwordInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            ShowSuccess();

            // Later we'll open the boss desktop here.
            return;
        }

        // ======================================
        // ALL OTHER PASSWORDS
        // ======================================

        // 1, 31, 1234, ABCD, etc. are incorrect.
        // None of them switches the computers.

        ShowError("Incorrect password!");
    }

    // ==========================================
    // FEEDBACK MESSAGES
    // ==========================================

    private void ShowError(string message)
    {
        ShowFeedback(
            message,
            new Color32(255, 140, 140, 255)
        );
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null)
            return;

        feedbackText.gameObject.SetActive(true);
        feedbackText.enabled = true;

        feedbackText.text = message;
        feedbackText.color = color;

        // Make sure text renders above other
        // objects inside LoginPage.
        feedbackText.transform.SetAsLastSibling();

        feedbackText.ForceMeshUpdate();
    }

    private void ShowSuccess()
    {
        if (feedbackText == null)
            return;

        // Place success message in the center.
        feedbackText.rectTransform.anchoredPosition =
            Vector2.zero;

        feedbackText.rectTransform.sizeDelta =
            new Vector2(650, 100);

        feedbackText.fontSize = 48;
        feedbackText.alignment =
            TextAlignmentOptions.Center;

        ShowFeedback(
            "ACCESS GRANTED!",
            new Color32(160, 240, 174, 255)
        );
    }

    // ==========================================
    // NUMBERED COMPUTER CLUES
    // ==========================================

    private void ShowComputerClue(ComputerStation computer)
    {
        string displayedClue = "";

        if (!lettersRevealed)
        {
            // ==================================
            // PHASE 1: NUMBERS
            // ==================================

            switch (computer.name)
            {
                case "PC1":
                    displayedClue = "3";
                    break;

                case "PC2":
                    displayedClue = "1";
                    break;

                case "PC3":
                    displayedClue = "4";
                    break;

                case "PC4":
                    displayedClue = "2";
                    break;

                default:
                    displayedClue = computer.clue;
                    break;
            }
        }
        else
        {
            // ==================================
            // PHASE 2: LETTERS
            // ==================================

            switch (computer.name)
            {
                case "PC1":
                    displayedClue = "e";
                    break;

                case "PC2":
                    displayedClue = "N";
                    break;

                case "PC3":
                    displayedClue = "B";
                    break;

                case "PC4":
                    displayedClue = "r";
                    break;

                default:
                    displayedClue = computer.clue;
                    break;
            }
        }

        clueText.text = displayedClue;

        Debug.Log(
            computer.name + " displays: " + displayedClue
        );
    }

    public void CloseComputerWindow()
    {
        computerWindow.SetActive(false);
    }

    // ==========================================
    // SCENE GIZMOS
    // ==========================================

    private void OnDrawGizmosSelected()
    {
        if (computers == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (ComputerStation computer in computers)
        {
            if (computer == null ||
                computer.interactionPoint == null)
                continue;

            Gizmos.DrawWireSphere(
                computer.interactionPoint.position,
                interactionRadius
            );
        }
    }
}
