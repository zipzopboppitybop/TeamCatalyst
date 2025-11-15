using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Catalyst.Player;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;

    public Inventory hotBarInventory;
    public Inventory playerInventory;
    public Inventory chestInventory;
    private VisualElement hotBarUI;
    private VisualElement root;
    private VisualElement playerInventoryUI;
    private VisualElement playerInventoryMenu;
    private VisualElement chestInventoryUI;
    private VisualElement chestInventoryMenu;
    private VisualElement shopMenu;
    private VisualElement achievementsMenu;
    private VisualElement menuSystemUI;
    private VisualElement[] hotBarSlots;
    private VisualElement[] playerInventorySlots;
    private VisualElement[] chestSlots;
    private int playerInventorySlotCount;
    private int chestSlotCount;
    private int hotBarSlotCount;
    private int selectedSlot;
    private InventorySlot selectedInventorySlot;
    public bool isChestOpen;
    private InventorySlot draggingSlotOriginal;
    private int draggingSlotIndex;
    private Inventory draggingFromInventory;
    private InputHandler inputHandler;
    public bool toggleInventory;

    private Catalyst.Player.PlayerController playerController;
    private GameObject player;

    public event System.Action<ItemData> OnSelectedItemChanged;

    void Start()
    {
        player = GameManager.instance.player;
        playerController = player.GetComponent<Catalyst.Player.PlayerController>();
        inputHandler = playerController.playerInputHandler;
        hotBarInventory = playerController.GetComponent<PlayerInventoryHolder>().PrimaryInventory;
        playerInventory = playerController.GetComponent <PlayerInventoryHolder>().SecondaryInventory;
        if (document == null)
        {
            document = GetComponent<UIDocument>();
        }

        root = document.rootVisualElement;
        hotBarUI = root.Q<VisualElement>("HotBar");
        playerInventoryUI = root.Q<VisualElement>("InventorySlots");
        playerInventoryMenu = root.Q<VisualElement>("InventoryMenu");
        chestInventoryUI = root.Q<VisualElement>("ChestInventorySlots");
        chestInventoryMenu = root.Q<VisualElement>("ChestInventoryMenu");
        menuSystemUI = root.Q<VisualElement>("MenuSystem");
        shopMenu = root.Q<VisualElement>("ShopMenu");
        achievementsMenu = root.Q<VisualElement>("AchieveMenu");

        InventoryHolder.OnDynamicInventoryDisplayRequested += OpenChest;

        hotBarUI.style.display = DisplayStyle.Flex;
        menuSystemUI.style.display = DisplayStyle.None;

        hotBarSlotCount = hotBarInventory.InventorySize;
        hotBarSlots = new VisualElement[hotBarSlotCount];
        playerInventorySlotCount = playerInventory.InventorySize;
        playerInventorySlots = new VisualElement[playerInventorySlotCount];

        for (int i = 0; i < hotBarSlotCount; i++)
        {
            hotBarSlots[i] = hotBarUI.Q<VisualElement>($"HotBarSlot-{i}");
            RegisterHotBarSlotCallbacks(i);
        }

        for (int i = 0; i < playerInventorySlotCount; i++)
        {
            playerInventorySlots[i] = playerInventoryUI.Q<VisualElement>($"InventoryItems-{i}");
            RegisterInventorySlotCallbacks(i);
        }

        hotBarInventory.OnInventorySlotChanged += RefreshHotBar;
        RefreshHotBar();
        playerInventory.OnInventorySlotChanged += RefreshInventory;
        RefreshInventory();

        SelectSlot(0);
    }

    void Update()
    {
        HandleHotBarInput();
        HandleInventoryInput();

        if (InventoryDragManager.draggedIcon != null)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);

            InventoryDragManager.draggedIcon.style.left = panelPos.x - InventoryDragManager.draggedIcon.resolvedStyle.width / 2f;
            InventoryDragManager.draggedIcon.style.top = panelPos.y - InventoryDragManager.draggedIcon.resolvedStyle.height / 2f;
            InventoryDragManager.draggedIcon.pickingMode = PickingMode.Ignore;
        }

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    DropSelectedItem();
        //}
    }

    private void HandleHotBarInput()
    {
        for (int i = 0; i < hotBarSlotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                return;
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0)
        {
            SelectSlot((selectedSlot - 1 + hotBarSlotCount) % hotBarSlotCount);
        }
        else if (scroll < 0)
        {
            SelectSlot((selectedSlot + 1) % hotBarSlotCount);
        }
    }

    private void HandleInventoryInput()
    {
        if (inputHandler == null)
        {
            return;
        }
          
        if (inputHandler.ToggleInventoryTriggered)
        {
            toggleInventory = !toggleInventory;
            playerController.isInventoryOpen = toggleInventory;

            menuSystemUI.style.display = toggleInventory ? DisplayStyle.Flex : DisplayStyle.None;

            if (toggleInventory)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;

                if (isChestOpen)
                {
                    CloseChest();
                    inputHandler.InteractTriggered = false; 
                }
            }

            inputHandler.ToggleInventoryTriggered = false;
        }

        if (isChestOpen && inputHandler.InteractTriggered)
        {
            CloseChest();
            inputHandler.InteractTriggered = false; 
        }
    }

    private void SelectSlot(int index)
    {
        selectedSlot = index;
        selectedInventorySlot = hotBarInventory.InventorySlots[index];
        UpdateSelection();

        OnSelectedItemChanged?.Invoke(selectedInventorySlot?.ItemData);
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < hotBarSlotCount; i++)
        {
            if (hotBarSlots[i] != null)
            {
                hotBarSlots[i].EnableInClassList("selected", i == selectedSlot);
            }
        }
    }

    private void RegisterInventorySlotCallbacks(int index)
    {
        Inventory inventoryRef = playerInventory;
        playerInventorySlots[index].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(index, inventoryRef));
        playerInventorySlots[index].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(index, inventoryRef));
    }

    private void RegisterHotBarSlotCallbacks(int index)
    {
        Inventory inventoryRef = hotBarInventory;
        hotBarSlots[index].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(index, inventoryRef));
        hotBarSlots[index].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(index, inventoryRef));
    }

    private void RegisterChestSlotCallbacks(int index)
    {
        Inventory inventoryRef = chestInventory;
        chestSlots[index].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(index, inventoryRef));
        chestSlots[index].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(index, inventoryRef));
    }

    public void RefreshHotBar(InventorySlot _ = null)
    {
        for (int i = 0; i < hotBarSlotCount; i++)
        {
            VisualElement slotElement = hotBarSlots[i];
            InventorySlot slotData = hotBarInventory.InventorySlots[i];

            Label countLabel = slotElement.Q<Label>("Count");
            countLabel.text = slotData.StackSize > 1 ? slotData.StackSize.ToString() : "";

            VisualElement icon = slotElement.Q<VisualElement>("Icon");
            if (slotData.ItemData != null)
            {
                icon.style.backgroundImage = new StyleBackground(slotData.ItemData.Icon);
                icon.style.opacity = 1f;
            }
            else
            {
                icon.style.backgroundImage = null;
                icon.style.opacity = 0f;
            }
        }
    }

    public void RefreshInventory(InventorySlot _ = null)
    {
        for (int i = 0; i < playerInventorySlotCount; i++)
        {
            VisualElement slotElement = playerInventorySlots[i];
            InventorySlot slotData = playerInventory.InventorySlots[i];

            Label countLabel = slotElement.Q<Label>("Count");
            countLabel.text = slotData.StackSize > 1 ? slotData.StackSize.ToString() : "";

            VisualElement icon = slotElement.Q<VisualElement>("Icon");
            if (slotData.ItemData != null)
            {
                icon.style.backgroundImage = new StyleBackground(slotData.ItemData.Icon);
                icon.style.opacity = 1f;
            }
            else
            {
                icon.style.backgroundImage = null;
                icon.style.opacity = 0f;
            }
        }
    }

    public void RefreshChest(InventorySlot _ = null)
    {
        for (int i = 0; i < chestSlotCount; i++)
        {
            VisualElement slotElement = chestSlots[i];
            InventorySlot slotData = chestInventory.InventorySlots[i];

            Label countLabel = slotElement.Q<Label>("Count");
            countLabel.text = slotData.StackSize > 1 ? slotData.StackSize.ToString() : "";

            VisualElement icon = slotElement.Q<VisualElement>("Icon");
            if (slotData.ItemData != null)
            {
                icon.style.backgroundImage = new StyleBackground(slotData.ItemData.Icon);
                icon.style.opacity = 1f;
            }
            else
            {
                icon.style.backgroundImage = null;
                icon.style.opacity = 0f;
            }
        }
    }

    private void OnSlotPointerDown(int index, Inventory inventoryRef)
    {
        InventorySlot slot = inventoryRef.InventorySlots[index];
        if (slot.ItemData == null || InventoryDragManager.IsDragging)
        {
            return;
        }

        draggingSlotOriginal = slot;
        draggingSlotIndex = index;
        draggingFromInventory = inventoryRef;

        VisualElement icon = new VisualElement
        {
            style =
            {
                width = 64,
                height = 64,
                opacity = 0.8f,
                position = Position.Absolute,
                backgroundImage = new StyleBackground(slot.ItemData.Icon)
            }
        };

        InventoryDragManager.BeginDrag(slot, icon, inventoryRef, root);
    }

    private void OnSlotPointerUp(int index, Inventory inventoryRef)
    {
        if (!InventoryDragManager.IsDragging) return;

        InventorySlot sourceSlot = InventoryDragManager.draggedSlot;
        Inventory sourceInventory = InventoryDragManager.draggedFromInventory;

        InventorySlot targetSlot = inventoryRef.InventorySlots[index];
        Inventory targetInventory = inventoryRef;

        if (sourceInventory == targetInventory && sourceSlot == targetSlot)
        {
            InventoryDragManager.EndDrag();
            return;
        }

        if (targetSlot.ItemData == null)
        {
            targetSlot.UpdateInventorySlot(sourceSlot.ItemData, sourceSlot.StackSize);
            sourceSlot.UpdateInventorySlot(null, 0);
        }
        else if (targetSlot.ItemData == sourceSlot.ItemData)
        {
            int total = targetSlot.StackSize + sourceSlot.StackSize;
            int max = targetSlot.ItemData.maxStackSize;

            if (total <= max)
            {
                targetSlot.UpdateInventorySlot(targetSlot.ItemData, total);
                sourceSlot.UpdateInventorySlot(null, 0);
            }
            else
            {
                targetSlot.UpdateInventorySlot(targetSlot.ItemData, max);
                sourceSlot.UpdateInventorySlot(sourceSlot.ItemData, total - max);
            }
        }
        else
        {
            ItemData tempItem = targetSlot.ItemData;
            int tempCount = targetSlot.StackSize;

            targetSlot.UpdateInventorySlot(sourceSlot.ItemData, sourceSlot.StackSize);
            sourceSlot.UpdateInventorySlot(tempItem, tempCount);
        }

        InventoryDragManager.EndDrag();

        sourceInventory.OnInventorySlotChanged?.Invoke(sourceSlot);
        targetInventory.OnInventorySlotChanged?.Invoke(targetSlot);

        draggingSlotOriginal = null;
        draggingSlotIndex = -1;
        draggingFromInventory = null;

        RefreshHotBar();
        RefreshInventory();
    }

    private void OpenChest(Inventory chest)
    {
        chestInventory = chest;
        isChestOpen = true;

        toggleInventory = true; 
        playerController.isInventoryOpen = true;

        menuSystemUI.style.display = DisplayStyle.Flex;
        chestInventoryMenu.style.display = DisplayStyle.Flex;
        playerInventoryMenu.style.left = new Length(5, LengthUnit.Percent);
        shopMenu.style.left = new Length(5, LengthUnit.Percent);
        achievementsMenu.style.left = new Length(5, LengthUnit.Percent);

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        chestSlotCount = chestInventory.InventorySize;
        chestSlots = new VisualElement[chestSlotCount];

        for (int i = 0; i < chestSlotCount; i++)
        {
            chestSlots[i] = chestInventoryUI.Q<VisualElement>($"ChestItems-{i}");
            RegisterChestSlotCallbacks(i);
        }

        chestInventory.OnInventorySlotChanged += RefreshChest;
        RefreshChest();
    }

    public void CloseChest()
    {
        if (!isChestOpen) return;
        Debug.Log("Closing CHests");
        chestInventory.OnInventorySlotChanged -= RefreshChest;
        chestInventory = null;
        isChestOpen = false;

        toggleInventory = false; 
        playerController.isInventoryOpen = false;
        playerInventoryMenu.style.left = new Length(30, LengthUnit.Percent);
        shopMenu.style.left = new Length(30, LengthUnit.Percent);
        achievementsMenu.style.left = new Length(30, LengthUnit.Percent);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        chestInventoryMenu.style.display = DisplayStyle.None;
        menuSystemUI.style.display = DisplayStyle.None;
    }

    public InventorySlot GetSelectedSlot()
    {
        return selectedInventorySlot;
    }

    public ItemData GetSelectedItem()
    {
        return selectedInventorySlot?.ItemData;
    }


    public void DropSelectedItem()
    {
        if (selectedInventorySlot == null || selectedInventorySlot.ItemData == null)
        {
            return;
        }

        GameObject dropPrefab = selectedInventorySlot.ItemData.dropPrefab;
        if (dropPrefab != null)
        {
            Vector3 dropPosition = player.transform.position + player.transform.forward * 1.5f;
            GameObject.Instantiate(dropPrefab, dropPosition, Quaternion.identity);
        }

        selectedInventorySlot.RemoveFromStack(1);

        if (selectedInventorySlot.StackSize <= 0)
        {
            selectedInventorySlot.UpdateInventorySlot(null, 0);
        }

        hotBarInventory.OnInventorySlotChanged?.Invoke(selectedInventorySlot);
        RefreshInventory();
    }
}