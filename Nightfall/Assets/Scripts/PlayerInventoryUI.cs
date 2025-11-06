using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private bool isHotbar;

    private Inventory inventory;
    private VisualElement root;
    private VisualElement[] slots;
    private int slotCount;

    private InventorySlot draggingSlotOriginal;
    private int draggingSlotIndex;
    private Inventory draggingFromInventory;

    void Start()
    {
        inventory = isHotbar ? player.GetComponent<PlayerInventoryHolder>().PrimaryInventory : player.GetComponent<PlayerInventoryHolder>().SecondaryInventory;

        root = GetComponent<UIDocument>().rootVisualElement;

        VisualElement slotsContainer = root.Q<VisualElement>("Slots");

        slotCount = inventory.InventorySize;
        slots = new VisualElement[slotCount];

        if (isHotbar)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = slotsContainer.Q<VisualElement>($"Slot-{i}");
                int index = i;
                RegisterSlotCallbacks(index);
            }
        }
        else
        {
            var rows = slotsContainer.Query<VisualElement>(className: "row").ToList();
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

        inventory.OnInventorySlotChanged += RefreshInventory;
        RefreshInventory();
    }

    void Update()
    {
        if (InventoryDragManager.draggedIcon != null)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);

            InventoryDragManager.draggedIcon.style.left = panelPos.x - InventoryDragManager.draggedIcon.resolvedStyle.width / 2f;
            InventoryDragManager.draggedIcon.style.top = panelPos.y - InventoryDragManager.draggedIcon.resolvedStyle.height / 2f;
            InventoryDragManager.draggedIcon.pickingMode = PickingMode.Ignore;
        }
    }

    private void RegisterSlotCallbacks(int index)
    {
        slots[index].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(index));
        slots[index].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(index));
    }

    private void RefreshInventory(InventorySlot _ = null)
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
}