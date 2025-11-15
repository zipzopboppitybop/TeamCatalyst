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
    private VisualElement shopMenu;
    private VisualElement shopUI;
    private VisualElement[] shopSlots;
    private List<Delivery> deliveries = new List<Delivery>();
    private ScrollView itemsContainer;
    public bool shopOpen = false;
    private bool boughtDog;

    private int boughtChickens = 0;
    private int boughtCow = 0;

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
        shopMenu = root.Q<VisualElement>("ShopMenu");
        shopUI = root.Q<VisualElement>("ShopSlots");

        PopulateShop();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    if (playerInventory.toggleInventory)
        //    {
        //        playerInventory.toggleInventory = false;
        //    }

        //    root.BringToFront();
        //    ToggleShop();
        //}

        //if ((playerInventory.toggleInventory && shopOpen) || (Input.GetKeyDown(KeyCode.E) && shopOpen) || (Input.GetKeyDown(KeyCode.Escape)) && shopOpen)
        //{
        //    ToggleShop();
        //}
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
        shopUI.Clear();

        foreach (ItemData item in itemsForSale)
        {
            VisualElement slot = new VisualElement();
            slot.AddToClassList("shopSlot");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("shopIcon");
            icon.style.backgroundImage = new StyleBackground(item.Icon);
            slot.Add(icon);

            Label price = new Label($"{item.displayName}\n${item.price}");
            price.AddToClassList("shopCount");
            slot.Add(price);

            slot.RegisterCallback<PointerDownEvent>(e => BuyItem(item));

            shopUI.Add(slot);
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

        if (item.itemType == ItemData.ItemType.Livestock)
        {
            if (item.name.Contains("Chicken") && boughtChickens < 5)
            {
                boughtChickens++;
                deliveries.Add(new Delivery(item, days));
                playerData.Currency -= finalPrice;
            }
            else if (item.name.Contains("GuardDog") && !boughtDog)
            {
                boughtDog = true;
                deliveries.Add(new Delivery(item, days));
                playerData.Currency -= finalPrice;
            }
            else if (item.name.Contains("Cow") && boughtCow < 2)
            {
                boughtCow++;
                deliveries.Add(new Delivery(item, days));
                playerData.Currency -= finalPrice;
            }
        }
        else
        {
            deliveries.Add(new Delivery(item, days));
            playerData.Currency -= finalPrice;
        }


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
                if (item.name.Contains("Chicken"))
                {
                    Vector3 homePoint = chickenCoop.transform.Find("ChickenCoopHome").position;
                    GameObject chicken = Instantiate(livestock[0], homePoint, Quaternion.identity);
                    Livestock livestockComponent = chicken.GetComponent<Livestock>();
                    livestockComponent.homePos = homePoint;
                    livestockComponent.FeedingTrough = feedingTrough.GetComponent<Chest>();

                    if(boughtChickens == 1 && PlayerInventoryUI.Instance != null)
                    {
                        PlayerInventoryUI.Instance.OnLivestockOwnedAchieved();
                    }
                }
                else if (item.name.Contains("GuardDog"))
                {
                    Vector3 homePoint = doghouse.transform.Find("DogHouseHomePos").position;
                    GameObject dog = Instantiate(livestock[1], homePoint, Quaternion.identity);
                    GuardDogAI livestockComponent = dog.GetComponent<GuardDogAI>();
                    livestockComponent.homePos = homePoint;
                    livestockComponent.FeedingTrough = feedingTrough.GetComponent<Chest>();
                }
                else if (item.name.Contains("Cow"))
                {
                    Vector3 homePoint = barn.transform.Find("BarnHomePos").position;
                    GameObject cow = Instantiate(livestock[2], homePoint, Quaternion.identity);
                    Livestock livestockComponent = cow.GetComponent<Livestock>();
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
        bool soldAnything = false;

        foreach (InventorySlot slot in chest.PrimaryInventory.InventorySlots)
        {
            if (slot.ItemData != null)
            {
                float amount = slot.StackSize;
                playerData.Currency += slot.ItemData.price * amount;

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
