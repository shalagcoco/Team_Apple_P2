
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
    [SerializeField] private GameObject bossHintPanel;
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionRadius = 1.0f;

    [Header("Computers")]
    [SerializeField] private ComputerStation[] computers;

    [Header("Computer UI")]
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

    [Header("Split Screen Camera")]
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float computerCameraSize = 3.2f;

    [SerializeField]
    private Vector2 cameraOffset =
        Vector2.zero;

    [Header("Optional Bag UI")]
    [SerializeField] private GameObject bagPanel;

    // Current computer
    private ComputerStation currentComputer;

    // Puzzle state
    private bool bossUnlocked = false;
    private bool lettersRevealed = false;

    // Split-screen state
    private bool splitViewActive = false;

    // Previous camera settings
    private Rect previousCameraRect;
    private float previousCameraSize;
    private Vector3 previousCameraPosition;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (computerWindow != null)
            computerWindow.SetActive(false);

        // Clear UI only at the beginning of the game.
        if (passwordInput != null)
            passwordInput.text = "";

        if (feedbackText != null)
            feedbackText.text = "";
    }

    // ==========================================
    // DETECT NEARBY COMPUTERS
    // ==========================================

    private void Update()
    {
        if (player == null || computerWindow == null)
            return;

        ComputerStation nearest = FindNearestComputer();

        // Staying near the same computer.
        if (nearest == currentComputer)
            return;

        currentComputer = nearest;

        // Player walked away from computers.
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

    // ==========================================
    // OPEN COMPUTER
    // ==========================================

    private void OpenComputer(ComputerStation computer)
    {
        // Close backpack panel if it was open.
        if (bagPanel != null)
            bagPanel.SetActive(false);

        // Activate split-screen view.
        EnterSplitView();

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
    // SPLIT-SCREEN CAMERA
    // ==========================================

    private void EnterSplitView()
    {
        if (splitViewActive)
            return;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not assigned!");
            return;
        }

        // Save the normal office camera settings.
        previousCameraRect = mainCamera.rect;

        previousCameraSize = mainCamera.orthographicSize;

        previousCameraPosition =
            mainCamera.transform.position;

        // Render the world in the LEFT HALF.
        // Preserve the existing reserved HUD height.
        mainCamera.rect = new Rect(
            previousCameraRect.x,
            previousCameraRect.y,
            previousCameraRect.width * 0.5f,
            previousCameraRect.height
        );

        // Zoom closer to Catsby.
        mainCamera.orthographicSize = computerCameraSize;

        splitViewActive = true;

        FocusCameraOnPlayer();
    }

    private void LateUpdate()
    {
        // Keep Catsby visible while using a computer.
        if (splitViewActive)
        {
            FocusCameraOnPlayer();
        }
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

    // ==========================================
    // CLOSE COMPUTER
    // ==========================================

    public void CloseComputerWindow()
    {
        if (computerWindow != null)
            computerWindow.SetActive(false);

        RestoreCamera();
    }

    private void RestoreCamera()
    {
        if (!splitViewActive || mainCamera == null)
            return;

        // Restore full office view.
        mainCamera.rect = previousCameraRect;

        mainCamera.orthographicSize = previousCameraSize;

        mainCamera.transform.position =
            previousCameraPosition;

        splitViewActive = false;
    }

    private void OnDisable()
    {
        // Safety: restore camera if this script is disabled.
        RestoreCamera();
    }

    // ==========================================
    // BOSS LOGIN
    // ==========================================

    private void OpenBossLogin()
    {
        if (bossUnlocked)
        {
            passwordInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            ShowSuccess();
        }
        else
        {
            passwordInput.gameObject.SetActive(true);
            loginButton.gameObject.SetActive(true);

            // Do not reset the input or feedback.
            // Catsby sees the computer exactly as they left it.
        }
    }

    public void CheckPassword()
    {
        if (currentComputer == null ||
            currentComputer.type != StationType.Boss)
            return;

        if (bossUnlocked)
            return;

        string attempt = passwordInput.text;

        if (string.IsNullOrEmpty(attempt))
        {
            ShowError("Enter a password.");
            return;
        }

        // ======================================
        // EXACT PUZZLE TRIGGER: 3142
        // ======================================

        if (attempt == "3142")
        {
            lettersRevealed = true;

            ShowError(
                "ALPHABETIC CHARACTERS ONLY!\n" +
                "Numbers must mean something else...\nMaybe you should check those computers again..."
            );

            Debug.Log(
                "Puzzle activated: computers show letters."
            );

            return;
        }

        // ======================================
        // CORRECT CASE-SENSITIVE PASSWORD
        // ======================================

        if (string.Equals(
            attempt,
            correctPassword,
            StringComparison.Ordinal))
        {
            bossUnlocked = true;

            passwordInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            ShowSuccess();

            Debug.Log("Boss computer unlocked!");

            // Later: display boss desktop here.
            return;
        }

        // Every other attempt is incorrect.
        ShowError("Incorrect password!");
    }

    // ==========================================
    // LOGIN FEEDBACK
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

        feedbackText.transform.SetAsLastSibling();
        feedbackText.ForceMeshUpdate();
    }


    private void ShowSuccess()
    {
        // Hide password controls.
        passwordInput.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);

        // Hide the old feedback.
        feedbackText.gameObject.SetActive(false);

        // Display Beryl's reminder instead.
        bossHintPanel.SetActive(true);
    }


    // ==========================================
    // FOUR CLUE COMPUTERS
    // ==========================================

    private void ShowComputerClue(ComputerStation computer)
    {
        string displayedClue = computer.clue;

        if (!lettersRevealed)
        {
            // BEFORE entering 3142

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
            }
        }
        else
        {
            // AFTER entering 3142

            switch (computer.name)
            {
                case "PC1":
                    displayedClue = "E";
                    break;

                case "PC2":
                    displayedClue = "N";
                    break;

                case "PC3":
                    displayedClue = "B";
                    break;

                case "PC4":
                    displayedClue = "R";
                    break;
            }
        }

        clueText.text = displayedClue;
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
