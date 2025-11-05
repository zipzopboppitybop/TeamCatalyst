using UnityEngine;
using UnityEngine.UIElements;

public class HotbarUI : MonoBehaviour
{
    private int slotCount;
    private int selectedSlot;
    private VisualElement[] slots;
    private Inventory inventory;
    [SerializeField] GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = player.GetComponent<InventoryHolder>().Inventory;
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement slotsContainer = root.Q<VisualElement>("Slots");

        slotCount = inventory.InventorySize;
        slots = new VisualElement[slotCount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotsContainer.Q<VisualElement>($"Slot-{i}");
        }

        inventory.OnInventorySlotChanged += OnInventorySlotChanged;

        Debug.Log($"Hotbar initialized with {slots.Length} slots for {player.name}");

        RefreshHotbar();
        SelectSlot(0);
    }

    void Update()
    {
        HandleInput();
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
            }
            else
            {
                icon.style.backgroundImage = null;
            }

            Debug.Log($"Refreshing slot {i}: {(slotData.ItemData != null ? slotData.ItemData.name : "Empty")}");
        }
    }

}
