using Catalyst.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;




public class ShopUI : MonoBehaviour
{
    public static ShopUI instance;
    [SerializeField] private UIDocument document;
    [SerializeField] private ItemData[] itemsForSale;
    [SerializeField] private Chest chest;
    [SerializeField] private PlayerInventoryUI playerInventory;
    [SerializeField] private Catalyst.Player.PlayerData playerData;
    [SerializeField] private GameObject chickenCoop;
    [SerializeField] private GameObject doghouse;
    [SerializeField] private GameObject barn;
    [SerializeField] private GameObject[] livestock;

    private VisualElement root;
    private List<Delivery> deliveries = new List<Delivery>();
    private ScrollView itemsContainer;
    public bool shopOpen = false;

    private int boughtChickens = 0;

    [System.Serializable]
    private class Delivery
    {
        public ItemData item;
        public int daysRemaining;

        public Delivery(ItemData item, int daysRemaining)
        {
            this.item = item;
            this.daysRemaining = daysRemaining;
        }
    }

    private void Start()
    {
        instance = this;
        root = document.rootVisualElement;
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

        bool express = false;

        GameManager gm = GameManager.instance;
        if (gm != null && gm.playerController != null && gm.playerController.playerInputHandler != null)
        {
            express = gm.playerController.playerInputHandler.SprintHeld;
        }
        float finalPrice = express ? item.price * 1.5f : item.price;
        int days = express ? 0 : 1;

        deliveries.Add(new Delivery(item, days));

        playerData.Currency -= finalPrice;
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
        List<Delivery> deliveriesDone = new List<Delivery>();

        foreach (Delivery delivery in deliveries)
        {
            delivery.daysRemaining--;

            if (delivery.daysRemaining <= 0)
            {
                deliveriesDone.Add(delivery);
            }
        }

        foreach (Delivery delivery in deliveriesDone)
        {
            deliveries.Remove(delivery);

            ItemData item = delivery.item;


            if (item.itemType == ItemData.ItemType.Livestock)
            {
                GameObject feedingTrough = GameObject.FindWithTag("FeedingTrough");
                if (item.name.Contains("Chicken") && boughtChickens < 5)
                {
                    Vector3 homePoint = chickenCoop.transform.Find("ChickenCoopHome").position;
                    GameObject chicken = Instantiate(livestock[0], homePoint, Quaternion.identity);
                    Livestock livestockComponent = chicken.GetComponent<Livestock>();
                    livestockComponent.homePos = homePoint;
                    livestockComponent.FeedingTrough = feedingTrough.GetComponent<Chest>();
                    boughtChickens++;

                    if(boughtChickens == 1 && PlayerInventoryUI.Instance != null)
                    {
                        PlayerInventoryUI.Instance.OnLivestockOwnedAchieved();
                    }
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
        bool soldAnything = false;

        foreach (InventorySlot slot in chest.PrimaryInventory.InventorySlots)
        {
            if (slot.ItemData != null)
            {
                float amount = slot.StackSize;
                playerData.Currency += slot.ItemData.price / 2 * amount;

                slot.ClearSlot();
                soldAnything = true;
            }

            chest.PrimaryInventory.NotifySlotChanged(slot);
        }
        
        if (soldAnything && PlayerInventoryUI.instance !=null)
        {
            PlayerInventoryUI.Instance.OnCropSoldAchieved();
        }

        ShopUI.instance.DeliverItems();
    }
}
