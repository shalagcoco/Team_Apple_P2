
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

    // UI colors
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
        ShowMapPage();
        bagPanel.SetActive(false);
    }

    // Open or close the bag
    public void ToggleBag()
    {
        bagPanel.SetActive(!bagPanel.activeSelf);
    }

    public void CloseBag()
    {
        bagPanel.SetActive(false);
    }

    // Tab button methods
    public void ShowMapPage()
    {
        ShowPage(0);
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

    // Display only the selected page
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

    // Highlight the selected tab
    private void UpdateTab(Button button, bool selected)
    {
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
