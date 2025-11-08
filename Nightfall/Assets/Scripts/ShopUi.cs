using UnityEngine;
using UnityEngine.UIElements;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private ItemData[] itemsForSale;
    [SerializeField] private Chest chest;
    [SerializeField] private PlayerInventoryUI playerInventory;
    [SerializeField] private Catalyst.Player.PlayerData playerData;

    private VisualElement root;
    private ScrollView itemsContainer;
    public bool shopOpen = false;

    private void Start()
    {
        root = uiDocument.rootVisualElement.Q<VisualElement>("Root");
        itemsContainer = root.Q<ScrollView>("ItemsContainer");

        root.style.display = DisplayStyle.None; 

        PopulateShop();
    }

    private void Update()
    {
        // Press J to toggle shop
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (playerInventory.toggleInventory)
            {
                playerInventory.toggleInventory = false;
                playerInventory.Show(false);
            }

            root.BringToFront();
            ToggleShop();
        }

        if ((playerInventory.toggleInventory && shopOpen) || (Input.GetKeyDown(KeyCode.E) && shopOpen) || (Input.GetKeyDown(KeyCode.Escape)) && shopOpen)
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        shopOpen = !shopOpen;
        root.style.display = shopOpen ? DisplayStyle.Flex : DisplayStyle.None;

        if (shopOpen)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    private void PopulateShop()
    {
        itemsContainer.Clear(); 

        foreach (ItemData item in itemsForSale)
        {
            VisualElement slot = new VisualElement();
            slot.AddToClassList("slot");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("icon");
            icon.style.backgroundImage = new StyleBackground(item.Icon);
            slot.Add(icon);

            Label price = new Label($"{item.displayName}\n${item.price}");
            price.AddToClassList("count");
            slot.Add(price);

            slot.RegisterCallback<PointerDownEvent>(e => BuyItem(item));

            itemsContainer.Add(slot);
        }
    }

    private void BuyItem(ItemData item)
    {
        if (playerData.Currency < item.price)
        {
            return;
        }

        playerData.Currency -= item.price;
        AddItemToInventory(chest.PrimaryInventory, item, 1);
    }

    private void AddItemToInventory(Inventory inventory, ItemData item, int amount)
    {
        foreach (InventorySlot slot in inventory.InventorySlots)
        {
            if (slot.ItemData == item && slot.RoomLeftInStack(amount))
            {
                slot.AddToStack(amount);
                inventory.NotifySlotChanged(slot);
                return;
            }
        }

        foreach (InventorySlot slot in inventory.InventorySlots)
        {
            if (slot.ItemData == null)
            {
                slot.UpdateInventorySlot(item, amount);
                inventory.NotifySlotChanged(slot);
                return;
            }
        }

        Debug.Log("Inventory full!");
    }
}
