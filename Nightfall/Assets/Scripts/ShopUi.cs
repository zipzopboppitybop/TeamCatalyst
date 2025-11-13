using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public static ShopUI instance;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private ItemData[] itemsForSale;
    [SerializeField] private Chest chest;
    [SerializeField] private PlayerInventoryUI playerInventory;
    [SerializeField] private Catalyst.Player.PlayerData playerData;
    [SerializeField] private GameObject chickenCoop;
    [SerializeField] private GameObject doghouse;
    [SerializeField] private GameObject barn;
    [SerializeField] private GameObject[] livestock;

    private VisualElement root;
    private Queue<ItemData> deliveries = new Queue<ItemData>();
    private ScrollView itemsContainer;
    public bool shopOpen = false;

    private int boughtChickens = 0;

    private void Start()
    {
        instance = this;
        root = uiDocument.rootVisualElement.Q<VisualElement>("Root");
        itemsContainer = root.Q<ScrollView>("ItemsContainer");

        root.style.display = DisplayStyle.None; 

        PopulateShop();
    }

    private void Update()
    {
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
        if (playerData.Currency < item.price || GameManager.instance.IsNight)
        {
            return;
        }

        if (item.itemType == ItemData.ItemType.Livestock)
        {
            if (item.name.Contains("Chicken") && boughtChickens < 5)
            {
                if (!chickenCoop.activeSelf)
                {
                    chickenCoop.SetActive(true);
                }

                deliveries.Enqueue(item);
                Debug.Log("Chickens will be delivered tomorrow");
                boughtChickens++;
            }
        }
        else
        {
            deliveries.Enqueue(item);
            Debug.Log($"{item.displayName} will be delivered tomorrow");

        }

        playerData.Currency -= item.price;
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

    public void DeliverItems()
    {
        while (deliveries.Count > 0)
        {
            ItemData item = deliveries.Dequeue();

            if (item.itemType == ItemData.ItemType.Livestock)
            {
                GameObject feedingTrough = GameObject.FindWithTag("FeedingTrough");
                if (item.name.Contains("Chicken"))
                {
                    Vector3 homePoint = chickenCoop.transform.Find("ChickenCoopHome").position;
                    GameObject chicken = Instantiate(livestock[0], homePoint, Quaternion.identity);
                    Livestock livestockComponent = chicken.GetComponent<Livestock>();
                    livestockComponent.homePos = homePoint;
                    livestockComponent.FeedingTrough = feedingTrough.GetComponent<Chest>();
                }

            }
            else
            {
                AddItemToInventory(chest.PrimaryInventory, item, 1);
            }
        }
    }

    public void SellItems()
    {
        foreach (InventorySlot slot in chest.PrimaryInventory.InventorySlots)
        {
            if (slot.ItemData != null)
            {
                int amount = slot.StackSize;
                playerData.Currency += slot.ItemData.sellValue * amount;

                slot.ClearSlot();
            }

            chest.PrimaryInventory.NotifySlotChanged(slot);
        }

        ShopUI.instance.DeliverItems();
    }
}
