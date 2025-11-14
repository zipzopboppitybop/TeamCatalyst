using UnityEngine;
using UnityEngine.UIElements;

public class TabManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement inventoryMenu;
    private VisualElement shopMenu;
    private VisualElement achievementsMenu;

    private Button inventoryButton;
    private Button shopButton;
    private Button achieveButton;

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        inventoryMenu = root.Q<VisualElement>("InventoryMenu");
        shopMenu = root.Q<VisualElement>("ShopMenu");
        achievementsMenu = root.Q<VisualElement>("AchieveMenu");

        inventoryButton = root.Q<Button>("InventoryButton");
        shopButton = root.Q<Button>("ShopButton");
        achieveButton = root.Q<Button>("AchieveButton");

        shopButton.BringToFront();
        inventoryButton.BringToFront();
        achieveButton.BringToFront();

        shopButton.clicked += () => ShowShop();
        inventoryButton.clicked += () => ShowInventory();
        achieveButton.clicked += () => ShowAchievements();

    }

    private void ShowShop()
    {
        inventoryMenu.style.display = DisplayStyle.None;
        shopMenu.style.display = DisplayStyle.Flex;
        achievementsMenu.style.display = DisplayStyle.None;
    }

    private void ShowInventory()
    {
        inventoryMenu.style.display = DisplayStyle.Flex;
        shopMenu.style.display = DisplayStyle.None;
        achievementsMenu.style.display = DisplayStyle.None;
    }

    private void ShowAchievements()
    {
        inventoryMenu.style.display = DisplayStyle.None;
        shopMenu.style.display = DisplayStyle.None;
        achievementsMenu.style.display = DisplayStyle.Flex;
    }

}