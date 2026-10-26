
using UnityEngine;
using TMPro;

public class TaskBoardUI : MonoBehaviour
{
    [Header("Task Texts")]
    [SerializeField] private TMP_Text mapTaskText;
    [SerializeField] private TMP_Text bossTaskText;
    [SerializeField] private TMP_Text catsbyTaskText;

    // Task completion states
    private bool mapCompleted = false;
    private bool bossCompleted = false;
    private bool catsbyCompleted = false;

    private Color incompleteColor =
        new Color32(52, 71, 93, 255);

    private Color completeColor =
        new Color32(65, 140, 90, 255);

    private void Awake()
    {
        UpdateTaskBoard();
    }

    // Called when player views the office map.
    public void CompleteMapTask()
    {
        mapCompleted = true;
        UpdateTaskBoard();
    }

    // Called after entering BerN correctly.
    public void CompleteBossTask()
    {
        bossCompleted = true;
        UpdateTaskBoard();
    }

    // Called after entering CatG on PC1.
    public void CompleteCatsbyTask()
    {
        catsbyCompleted = true;
        UpdateTaskBoard();
    }

    private void UpdateTaskBoard()
    {
        UpdateTask(
            mapTaskText,
            "Take a look at the office map",
            mapCompleted
        );

        UpdateTask(
            bossTaskText,
            "Enter Boss's computer",
            bossCompleted
        );

        UpdateTask(
            catsbyTaskText,
            "Enter Catsby Great's computer",
            catsbyCompleted
        );
    }

    private void UpdateTask(
        TMP_Text taskText,
        string description,
        bool completed)
    {
        if (taskText == null)
            return;

        // ASCII checkmarks for reliable font support.
        taskText.text =
            (completed ? "[X] " : "[ ] ") + description;

        taskText.color =
            completed ? completeColor : incompleteColor;
    }
}
