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
    private VisualElement hotBarUI;
    private VisualElement root;
    private VisualElement playerInventoryUI;
    private VisualElement menuSystemUI;
    private VisualElement[] hotBarSlots;
    private VisualElement[] playerInventorySlots;
    private int playerInventorySlotCount;
    private int hotBarSlotCount;
    private int selectedSlot;
    private InventorySlot selectedInventorySlot;

    private InventorySlot draggingSlotOriginal;
    private int draggingSlotIndex;
    private Inventory draggingFromInventory;
    private InputHandler inputHandler;
    public bool toggleInventory;

    private Catalyst.Player.PlayerController playerController;
    private GameObject player;
    private bool isVisible = false;

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
        menuSystemUI = root.Q<VisualElement>("MenuSystem");


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
            }
        }
    }

    private void SelectSlot(int index)
    {
        selectedSlot = index;
        selectedInventorySlot = hotBarInventory.InventorySlots[index];
        UpdateSelection();
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

    public void SetInventory(Inventory newInventory)
    {
        //if (inventory != null)
        //{
        //    inventory.OnInventorySlotChanged -= RefreshInventory;
        //}

        //inventory = newInventory;

        //if (inventory != null)
        //{
        //    inventory.OnInventorySlotChanged += RefreshInventory;
        //    BuildChestSlots();
        //    RefreshInventory();
        //}
        //else
        //{
        //    root.style.display = DisplayStyle.None;
        //    isVisible = false;
        //}
    }

    private void BuildChestSlots()
    {
        //VisualElement slotsContainer = root.Q<VisualElement>("Slots");
        //if (slotsContainer == null)
        //{
        //    return;
        //}

        //slotCount = inventory.InventorySize;
        //slots = new VisualElement[slotCount];

        //List<VisualElement> rows = slotsContainer.Query<VisualElement>(className: "row").ToList();
        //int index = 0;
        //foreach (VisualElement row in rows)
        //{
        //    foreach (VisualElement slot in row.Children())
        //    {
        //        if (!slot.ClassListContains("slot")) continue;
        //        if (index >= slotCount) break;

        //        slots[index] = slot;
        //        int currentIndex = index;
        //        RegisterInventorySlotCallbacks(currentIndex);
        //        index++;
        //    }
        //}
    }
    public void Show(bool show)
    {
        //if (root == null) return;

        //if (isChestUI)
        //{
        //    if (!show)
        //    {
        //        root.style.display = DisplayStyle.None;
        //        return;
        //    }

        //    if (inventory == null)
        //    {
        //        root.style.display = DisplayStyle.None;
        //        return;
        //    }
        //}

        //root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public bool IsVisible()
    {
        return isVisible && root.style.display == DisplayStyle.Flex;
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
        //if (selectedInventorySlot == null || selectedInventorySlot.ItemData == null)
        //{
        //    return;
        //}

        //GameObject dropPrefab = selectedInventorySlot.ItemData.dropPrefab;
        //if (dropPrefab != null)
        //{
        //    Vector3 dropPosition = player.transform.position + player.transform.forward * 1.5f;
        //    GameObject.Instantiate(dropPrefab, dropPosition, Quaternion.identity);
        //}

        //selectedInventorySlot.RemoveFromStack(1);

        //if (selectedInventorySlot.StackSize <= 0)
        //{
        //    selectedInventorySlot.UpdateInventorySlot(null, 0);
        //}

        //inventory.OnInventorySlotChanged?.Invoke(selectedInventorySlot);
        //RefreshInventory();
    }
}