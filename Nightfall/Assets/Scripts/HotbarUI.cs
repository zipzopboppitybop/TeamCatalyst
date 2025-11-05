using UnityEngine;
using UnityEngine.UIElements;

public class HotbarUI : MonoBehaviour
{
    private int slotCount;
    private int selectedSlot;
    private VisualElement[] slots;
    private Inventory inventory;
    [SerializeField] GameObject player;

    private VisualElement root;
    private InventorySlot draggedSlot;
    private VisualElement draggedIcon;
    private bool isDragging => draggedSlot != null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = player.GetComponent<InventoryHolder>().PrimaryInventory;
        root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement slotsContainer = root.Q<VisualElement>("Slots");

        slotCount = inventory.InventorySize;
        slots = new VisualElement[slotCount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotsContainer.Q<VisualElement>($"Slot-{i}");
            int currentIndex = i;

            slots[i].RegisterCallback<PointerDownEvent>(e => OnSlotPointerDown(currentIndex, e));
            slots[i].RegisterCallback<PointerUpEvent>(e => OnSlotPointerUp(currentIndex, e));
        }

        inventory.OnInventorySlotChanged += OnInventorySlotChanged;

        RefreshHotbar();
        SelectSlot(0);
    }

    void Update()
    {
        HandleInput();

        if (draggedIcon != null)
        {
            Vector2 mousePos = Input.mousePosition;

            mousePos.y = Screen.height - mousePos.y;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);

            float iconWidth = draggedIcon.resolvedStyle.width;
            float iconHeight = draggedIcon.resolvedStyle.height;

            draggedIcon.style.left = panelPos.x - iconWidth / 2f;
            draggedIcon.style.top = panelPos.y - iconHeight / 2f;

            draggedIcon.pickingMode = PickingMode.Ignore;
        }
    }

    private void HandleInput()
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

    private void SelectSlot(int index)
    {
        selectedSlot = index;
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

    private void OnInventorySlotChanged(InventorySlot changedSlot)
    {
        RefreshHotbar();
    }

    private void RefreshHotbar()
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

    private void OnSlotPointerDown(int index, PointerDownEvent e)
    {
        InventorySlot slot = inventory.InventorySlots[index];
        if (slot.ItemData != null && !isDragging)
        {
            draggedSlot = new InventorySlot(slot.ItemData, slot.StackSize);

            draggedIcon = new VisualElement();
            draggedIcon.style.backgroundImage = new StyleBackground(slot.ItemData.Icon);
            draggedIcon.style.position = Position.Absolute;
            draggedIcon.style.width = 64;
            draggedIcon.style.height = 64;
            draggedIcon.style.opacity = 0.8f;
            draggedIcon.style.scale = new Scale(new Vector3(1.1f, 1.1f, 1));

            root.Add(draggedIcon);

            slot.UpdateInventorySlot(null, 0);
            RefreshHotbar();
        }
    }

    private void OnSlotPointerUp(int index, PointerUpEvent e)
    {
        if (draggedSlot == null)
        {
            return;
        }

        InventorySlot target = inventory.InventorySlots[index];
        InventorySlot source = draggedSlot;

        if (target.ItemData == null)
        {
            target.UpdateInventorySlot(source.ItemData, source.StackSize);
        }
        else if (target.ItemData == source.ItemData)
        {
            int total = target.StackSize + source.StackSize;
            int max = target.ItemData.maxStackSize;

            if (total <= max)
            {
                target.UpdateInventorySlot(target.ItemData, total);
            }
            else
            {
                int remainder = total - max;
                target.UpdateInventorySlot(target.ItemData, max);
                source.UpdateInventorySlot(source.ItemData, remainder);
                ReturnItemToFirstEmptySlot(source);
            }
        }
        else
        {
            ItemData tempItem = target.ItemData;
            int tempCount = target.StackSize;

            target.UpdateInventorySlot(source.ItemData, source.StackSize);
            ReturnItemToFirstEmptySlot(new InventorySlot(tempItem, tempCount));
        }

        EndDrag();
        RefreshHotbar();
    }

    private void ReturnItemToFirstEmptySlot(InventorySlot slotToReturn)
    {
        if (slotToReturn == null || slotToReturn.ItemData == null)
        {
            return;
        }

        foreach (InventorySlot slot in inventory.InventorySlots)
        {
            if (slot.ItemData == null)
            {
                slot.UpdateInventorySlot(slotToReturn.ItemData, slotToReturn.StackSize);
                return;
            }
        }
    }

    private void EndDrag()
    {
        if (draggedIcon != null)
        {
            root.Remove(draggedIcon);
            draggedIcon = null;
        }
        draggedSlot = null;
    }
}
