
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

        // Each computer remembers its own input and feedback.
        [NonSerialized] public string savedInput;
        [NonSerialized] public string savedFeedback;
        [NonSerialized] public Color savedFeedbackColor;
    }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionRadius = 1.0f;

    [Header("Computers")]
    [SerializeField] private ComputerStation[] computers;

    [Header("Computer UI")]
    [SerializeField] private GameObject computerWindow;
    [SerializeField] private GameObject bossLoginPage;
    [SerializeField] private GameObject cluePage;

    [Header("Login UI")]
    [SerializeField] private TMP_Text employeeNameText;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button loginButton;

    [Header("Clue Screen")]
    [SerializeField] private TMP_Text clueText;

    [Header("Boss Reminder - Optional")]
    [SerializeField] private GameObject bossHintPanel;

    [Header("Passwords - Case Sensitive")]
    [SerializeField] private string correctPassword = "BerN";
    [SerializeField] private string catsbyPassword = "CatG";

    [Header("Split Screen Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float computerCameraSize = 3.2f;
    [SerializeField] private Vector2 cameraOffset = Vector2.zero;

    [Header("Optional Bag UI")]
    [SerializeField] private GameObject bagPanel;

    // Game progression
    private bool lettersRevealed = false;
    private bool bossUnlocked = false;
    private bool catsbyUnlocked = false;

    // Interaction state
    private ComputerStation currentComputer;

    // Camera state
    private bool splitViewActive = false;
    private Rect previousCameraRect;
    private float previousCameraSize;
    private Vector3 previousCameraPosition;

    private readonly Color errorColor =
        new Color32(255, 140, 140, 255);

    private readonly Color successColor =
        new Color32(160, 240, 174, 255);

    // =========================================
    // INITIALIZATION
    // =========================================

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (computerWindow != null)
            computerWindow.SetActive(false);

        if (bossHintPanel != null)
            bossHintPanel.SetActive(false);

        // Initialize saved login states for each PC.
        if (computers != null)
        {
            foreach (ComputerStation pc in computers)
            {
                if (pc == null) continue;

                pc.savedInput = "";
                pc.savedFeedback = "";
                pc.savedFeedbackColor = errorColor;
            }
        }
    }

    // =========================================
    // DETECT COMPUTERS
    // =========================================

    private void Update()
    {
        if (player == null || computerWindow == null)
            return;

        ComputerStation nearest = FindNearestComputer();

        if (nearest == currentComputer)
            return;

        // Save the previous computer before switching.
        SaveCurrentLogin();

        currentComputer = nearest;

        if (currentComputer == null)
        {
            CloseComputerWindow();
            return;
        }

        OpenComputer(currentComputer);
    }

    private ComputerStation FindNearestComputer()
    {
        ComputerStation nearest = null;
        float bestDistance = interactionRadius;

        if (computers == null)
            return null;

        foreach (ComputerStation pc in computers)
        {
            if (pc == null || pc.interactionPoint == null)
                continue;

            float distance = Vector2.Distance(
                player.position,
                pc.interactionPoint.position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = pc;
            }
        }

        return nearest;
    }

    // =========================================
    // OPEN COMPUTER
    // =========================================

    private void OpenComputer(ComputerStation pc)
    {
        if (bagPanel != null)
            bagPanel.SetActive(false);

        EnterSplitView();
        computerWindow.SetActive(true);

        // BOSS COMPUTER
        if (pc.type == StationType.Boss)
        {
            ShowPage(bossLoginPage);
            OpenBossComputer();
            return;
        }

        // BEFORE BOSS UNLOCK:
        // PCs show puzzle numbers or letters.
        if (!bossUnlocked)
        {
            ShowPage(cluePage);
            ShowComputerClue(pc);
            return;
        }

        // AFTER BOSS UNLOCK:
        // All four PCs show employee login screens.
        ShowPage(bossLoginPage);
        OpenEmployeeLogin(pc);
    }

    private void ShowPage(GameObject selected)
    {
        bossLoginPage.SetActive(selected == bossLoginPage);
        cluePage.SetActive(selected == cluePage);

        if (bossHintPanel != null)
            bossHintPanel.SetActive(false);
    }

    // =========================================
    // BOSS COMPUTER
    // =========================================

    private void OpenBossComputer()
    {
        if (employeeNameText != null)
            employeeNameText.text = "Beryl Nocat";

        if (bossUnlocked)
        {
            ShowBossUnlocked();
        }
        else
        {
            ShowLoginControls();
            RestoreSavedLogin(currentComputer);
        }
    }

    private void ShowBossUnlocked()
    {
        HideLoginControls();

        if (bossHintPanel != null)
        {
            // Show Beryl's reminder.
            feedbackText.gameObject.SetActive(false);
            bossHintPanel.SetActive(true);
        }
        else
        {
            // Fallback if no reminder panel exists.
            ShowFeedback("ACCESS GRANTED!", successColor);
        }
    }

    // =========================================
    // EMPLOYEE COMPUTERS
    // =========================================

    private void OpenEmployeeLogin(ComputerStation pc)
    {
        // Deliberately hide employee identity.
        // The player must use the missing-plant clue.
        if (employeeNameText != null)
            employeeNameText.text = "EMPLOYEE LOGIN - " + pc.name;

        if (pc.name == "PC1" && catsbyUnlocked)
        {
            HideLoginControls();

            ShowFeedback(
                "ACCESS GRANTED!\nWelcome, Catsby Great.",
                successColor
            );

            return;
        }

        ShowLoginControls();
        RestoreSavedLogin(pc);
    }

    private void ShowLoginControls()
    {
        passwordInput.gameObject.SetActive(true);
        loginButton.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
    }

    private void HideLoginControls()
    {
        passwordInput.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
    }

    // =========================================
    // PASSWORD VALIDATION
    // =========================================

    public void CheckPassword()
    {
        if (currentComputer == null ||
            !computerWindow.activeSelf ||
            !bossLoginPage.activeSelf)
            return;

        string attempt = passwordInput.text;

        // No trimming: passwords must match exactly.
        if (string.IsNullOrEmpty(attempt))
        {
            ShowError("Enter a password.");
            return;
        }

        // BOSS LOGIN
        if (currentComputer.type == StationType.Boss)
        {
            CheckBossPassword(attempt);
        }
        // EMPLOYEE LOGIN
        else if (bossUnlocked)
        {
            CheckEmployeePassword(attempt);
        }
    }

    private void CheckBossPassword(string attempt)
    {
        if (bossUnlocked)
            return;

        // Only exactly 3142 activates the puzzle.
        if (attempt == "3142")
        {
            lettersRevealed = true;

            ShowError(
                           "ALPHABETIC CHARACTERS ONLY!\n" +
                           "Numbers must mean something else...\nMaybe you should check those computers again..."
                       );

            Debug.Log("Numbered PCs switched to letters.");
            return;
        }

        // Case-sensitive boss password.
        if (string.Equals(
            attempt,
            correctPassword,
            StringComparison.Ordinal))
        {
            bossUnlocked = true;

            Debug.Log("Beryl Nocat's PC unlocked.");

            // The four numbered PCs now become
            // employee login computers.
            ShowBossUnlocked();
            return;
        }

        ShowError("Incorrect password!");
    }

    private void CheckEmployeePassword(string attempt)
    {
        // Only PC1 belongs to Catsby.
        if (currentComputer.name == "PC1" &&
            string.Equals(
                attempt,
                catsbyPassword,
                StringComparison.Ordinal))
        {
            catsbyUnlocked = true;

            HideLoginControls();

            ShowFeedback(
                "ACCESS GRANTED!\nWelcome, Catsby Great.",
                successColor
            );

            Debug.Log("Catsby's computer unlocked!");

            // Later: add the next chore or win screen.
            return;
        }

        // Every other workstation rejects CatG.
        ShowError("Incorrect password!");
    }

    // =========================================
    // PRESERVE COMPUTER STATE
    // =========================================

    private void SaveCurrentLogin()
    {
        if (currentComputer == null ||
            computerWindow == null ||
            !computerWindow.activeSelf ||
            !bossLoginPage.activeSelf ||
            !passwordInput.gameObject.activeSelf)
            return;

        currentComputer.savedInput = passwordInput.text;
        currentComputer.savedFeedback = feedbackText.text;
        currentComputer.savedFeedbackColor =
            feedbackText.color;
    }

    private void RestoreSavedLogin(ComputerStation pc)
    {
        if (pc == null)
            return;

        passwordInput.text = pc.savedInput ?? "";

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = pc.savedFeedback ?? "";
        feedbackText.color = pc.savedFeedbackColor;
    }

    // =========================================
    // FEEDBACK
    // =========================================

    private void ShowError(string message)
    {
        ShowFeedback(message, errorColor);
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.enabled = true;
        feedbackText.text = message;
        feedbackText.color = color;

        feedbackText.ForceMeshUpdate();
    }

    // =========================================
    // NUMBERED PC CLUES
    // =========================================

    private void ShowComputerClue(ComputerStation pc)
    {
        string displayed = pc.clue;

        if (!lettersRevealed)
        {
            switch (pc.name)
            {
                case "PC1": displayed = "3"; break;
                case "PC2": displayed = "1"; break;
                case "PC3": displayed = "4"; break;
                case "PC4": displayed = "2"; break;
            }
        }
        else
        {
            switch (pc.name)
            {
                case "PC1": displayed = "E"; break;
                case "PC2": displayed = "N"; break;
                case "PC3": displayed = "B"; break;
                case "PC4": displayed = "R"; break;
            }
        }

        clueText.text = displayed;
    }

    // =========================================
    // SPLIT SCREEN CAMERA
    // =========================================

    private void EnterSplitView()
    {
        if (splitViewActive || mainCamera == null)
            return;

        previousCameraRect = mainCamera.rect;
        previousCameraSize = mainCamera.orthographicSize;
        previousCameraPosition =
            mainCamera.transform.position;

        // Left half of existing gameplay viewport.
        mainCamera.rect = new Rect(
            previousCameraRect.x,
            previousCameraRect.y,
            previousCameraRect.width * 0.5f,
            previousCameraRect.height
        );

        mainCamera.orthographicSize = computerCameraSize;
        splitViewActive = true;

        FocusCameraOnPlayer();
    }

    private void LateUpdate()
    {
        if (splitViewActive)
            FocusCameraOnPlayer();
    }

    private void FocusCameraOnPlayer()
    {
        if (player == null || mainCamera == null)
            return;

        mainCamera.transform.position = new Vector3(
            player.position.x + cameraOffset.x,
            player.position.y + cameraOffset.y,
            previousCameraPosition.z
        );
    }

    // =========================================
    // CLOSE COMPUTER WINDOW
    // =========================================

    public void CloseComputerWindow()
    {
        SaveCurrentLogin();

        if (computerWindow != null)
            computerWindow.SetActive(false);

        RestoreCamera();
    }

    private void RestoreCamera()
    {
        if (!splitViewActive || mainCamera == null)
            return;

        mainCamera.rect = previousCameraRect;
        mainCamera.orthographicSize = previousCameraSize;
        mainCamera.transform.position =
            previousCameraPosition;

        splitViewActive = false;
    }

    private void OnDisable()
    {
        RestoreCamera();
    }

    // =========================================
    // SCENE GIZMOS
    // =========================================

    private void OnDrawGizmosSelected()
    {
        if (computers == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (ComputerStation pc in computers)
        {
            if (pc == null || pc.interactionPoint == null)
                continue;

            Gizmos.DrawWireSphere(
                pc.interactionPoint.position,
                interactionRadius
            );
        }
    }
}
