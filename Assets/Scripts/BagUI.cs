
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagUI : MonoBehaviour
{
    [Header("Main Bag Panel")]
    [SerializeField] private GameObject bagPanel;

    [Header("Pages")]
    [SerializeField] private GameObject mapPage;
    [SerializeField] private GameObject tasksPage;
    [SerializeField] private GameObject idPage;
    [SerializeField] private GameObject inventoryPage;

    [Header("Tab Buttons")]
    [SerializeField] private Button mapTabButton;
    [SerializeField] private Button tasksTabButton;
    [SerializeField] private Button idTabButton;
    [SerializeField] private Button inventoryTabButton;

    [Header("Task Board")]
    [SerializeField] private TaskBoardUI taskBoard;

    private readonly Color selectedColor =
        new Color32(52, 71, 93, 255);

    private readonly Color normalColor =
        new Color32(219, 207, 186, 255);

    private readonly Color selectedTextColor =
        new Color32(255, 248, 234, 255);

    private readonly Color normalTextColor =
        new Color32(52, 71, 93, 255);

    private void Start()
    {
        // Begin with Tasks as the default page.
        // This prevents the Map task from being
        // completed automatically at game startup.
        ShowTasksPage();

        bagPanel.SetActive(false);
    }

    public void ToggleBag()
    {
        bagPanel.SetActive(!bagPanel.activeSelf);
    }

    public void CloseBag()
    {
        bagPanel.SetActive(false);
    }


    public void ShowMapPage()
    {
        // Display the map page.
        ShowPage(0);

        // Complete the map task when the player views it.
        if (bagPanel.activeInHierarchy && taskBoard != null)
        {
            taskBoard.CompleteMapTask();
        }
    }


    public void ShowTasksPage()
    {
        ShowPage(1);
    }

    public void ShowIDPage()
    {
        ShowPage(2);
    }

    public void ShowInventoryPage()
    {
        ShowPage(3);
    }

    private void ShowPage(int pageIndex)
    {
        mapPage.SetActive(pageIndex == 0);
        tasksPage.SetActive(pageIndex == 1);
        idPage.SetActive(pageIndex == 2);
        inventoryPage.SetActive(pageIndex == 3);

        UpdateTab(mapTabButton, pageIndex == 0);
        UpdateTab(tasksTabButton, pageIndex == 1);
        UpdateTab(idTabButton, pageIndex == 2);
        UpdateTab(inventoryTabButton, pageIndex == 3);
    }

    private void UpdateTab(Button button, bool selected)
    {
        if (button == null)
            return;

        button.image.color =
            selected ? selectedColor : normalColor;

        TMP_Text label =
            button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.color = selected
                ? selectedTextColor
                : normalTextColor;
        }
    }
}
