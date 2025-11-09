using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Catalyst.Player;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private bool isHotbar;
    [SerializeField] private PlayerInventoryUI hotbarUI;
    [SerializeField] private UIDocument document;

    public Inventory inventory;
    private VisualElement root;
    private VisualElement[] slots;
    private int slotCount;
    private int selectedSlot;
    private InventorySlot selectedInventorySlot;

    private InventorySlot draggingSlotOriginal;
    private int draggingSlotIndex;
    private Inventory draggingFromInventory;
    private InputHandler inputHandler;
    public bool toggleInventory;
    public bool isChestUI;

    private Catalyst.Player.PlayerController playerController;
    private GameObject player;
    private bool isVisible = false;

    void Start()
    {
        player = GameManager.instance.player;
        playerController = player.GetComponent<Catalyst.Player.PlayerController>();
        inputHandler = playerController.playerInputHandler;
        if (document == null)
        {
            document = GetComponent<UIDocument>();
        }


        root = document.rootVisualElement;

        root.pickingMode = PickingMode.Ignore;

        if (!isChestUI)
        {
            inventory = isHotbar ? GameManager.instance.player.GetComponent<PlayerInventoryHolder>().PrimaryInventory : GameManager.instance.player.GetComponent<PlayerInventoryHolder>().SecondaryInventory;
        }

        if (!isHotbar)
        {
            root.style.display = DisplayStyle.None;
        }

        VisualElement slotsContainer = root.Q<VisualElement>("Slots");
        slots = null;

        if (inventory == null)
        {
            return;
        }

        slotCount = inventory.InventorySize;
        slots = new VisualElement[slotCount];

        if (isHotbar)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = slotsContainer.Q<VisualElement>($"Slot-{i}");
                RegisterSlotCallbacks(i);
            }
        }
        else
        {
            List<VisualElement> rows = slotsContainer.Query<VisualElement>(className: "row").ToList();
            int index = 0;
            foreach (VisualElement row in rows)
            {
                foreach (VisualElement slot in row.Children())
                {
                    if (!slot.ClassListContains("slot")) continue;
                    if (index >= slotCount) break;

                    slots[index] = slot;
                    int currentIndex = index;
                    RegisterSlotCallbacks(currentIndex);
                    index++;
                }
            }

            root.style.display = DisplayStyle.None;
        }

        inventory.OnInventorySlotChanged += RefreshInventory;
        RefreshInventory();

        if (isHotbar)
        {
            SelectSlot(0);
        }
    }

    void Update()
    {
        if (isHotbar)
        {
            HandleHotBarInput();
        }

        if (!isHotbar && !isChestUI)
        {
            HandleInventoryInput();
        }

        if (InventoryDragManager.draggedIcon != null)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);

            InventoryDragManager.draggedIcon.style.left = panelPos.x - InventoryDragManager.draggedIcon.resolvedStyle.width / 2f;
            InventoryDragManager.draggedIcon.style.top = panelPos.y - InventoryDragManager.draggedIcon.resolvedStyle.height / 2f;
            InventoryDragManager.draggedIcon.pickingMode = PickingMode.Ignore;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DropSelectedItem();
        }
    }

    private void HandleHotBarInput()
    {
        for (int i = 0; i < slotCount; i++)
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
            SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
        }
        else if (scroll < 0)
        {
            SelectSlot((selectedSlot + 1) % slotCount);
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
            root.style.display = toggleInventory ? DisplayStyle.Flex : DisplayStyle.None;

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
        selectedInventorySlot = inventory.InventorySlots[index];
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] != null)
            {
                slots[i].EnableInClassList("selected", i == selectedSlot);
            }
        }
    }

    private void RegisterSlotCallbacks(int index)
    {
        slots[index].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(index));
        slots[index].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(index));
    }

    public void RefreshInventory(InventorySlot _ = null)
    {
        for (int i = 0; i < slotCount; i++)
        {
            VisualElement slotElement = slots[i];
            InventorySlot slotData = inventory.InventorySlots[i];

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

    private void OnSlotPointerDown(int index)
    {
        InventorySlot slot = inventory.InventorySlots[index];
        if (slot.ItemData == null || InventoryDragManager.IsDragging)
        {
            return;
        }

        draggingSlotOriginal = slot;
        draggingSlotIndex = index;
        draggingFromInventory = inventory;

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

        InventoryDragManager.BeginDrag(slot, icon, inventory, root);
    }

    private void OnSlotPointerUp(int index)
    {
        if (!InventoryDragManager.IsDragging) return;

        InventorySlot sourceSlot = InventoryDragManager.draggedSlot;
        Inventory sourceInventory = InventoryDragManager.draggedFromInventory;

        InventorySlot targetSlot = inventory.InventorySlots[index];
        Inventory targetInventory = inventory;

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
    }

    public void SetInventory(Inventory newInventory)
    {
        if (inventory != null)
        {
            inventory.OnInventorySlotChanged -= RefreshInventory;
        }

        inventory = newInventory;

        if (inventory != null)
        {
            inventory.OnInventorySlotChanged += RefreshInventory;
            BuildChestSlots();
            RefreshInventory();
        }
        else
        {
            root.style.display = DisplayStyle.None;
            isVisible = false;
        }
    }

    private void BuildChestSlots()
    {
        VisualElement slotsContainer = root.Q<VisualElement>("Slots");
        if (slotsContainer == null)
        {
            return;
        }

        slotCount = inventory.InventorySize;
        slots = new VisualElement[slotCount];

        List<VisualElement> rows = slotsContainer.Query<VisualElement>(className: "row").ToList();
        int index = 0;
        foreach (VisualElement row in rows)
        {
            foreach (VisualElement slot in row.Children())
            {
                if (!slot.ClassListContains("slot")) continue;
                if (index >= slotCount) break;

                slots[index] = slot;
                int currentIndex = index;
                RegisterSlotCallbacks(currentIndex);
                index++;
            }
        }
    }
    public void Show(bool show)
    {
        if (root == null) return;

        if (isChestUI)
        {
            if (!show)
            {
                root.style.display = DisplayStyle.None;
                return;
            }

            if (inventory == null)
            {
                root.style.display = DisplayStyle.None;
                return;
            }
        }

        root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
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

        inventory.OnInventorySlotChanged?.Invoke(selectedInventorySlot);
        RefreshInventory();
    }
}